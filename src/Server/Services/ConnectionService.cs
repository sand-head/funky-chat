using FunkyChat.Protos;
using FunkyChat.Server.Infrastructure;
using FunkyChat.Server.Models;
using FunkyChat.Server.Models.Notifications;
using Google.Protobuf;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CommandType = FunkyChat.Protos.Command.CommandOneofCase;

namespace FunkyChat.Server.Services
{
    public class ConnectionService : BackgroundService
    {
        private readonly ILogger<ConnectionService> _logger;
        private readonly IMediator _mediator;
        private readonly NameGenerationService _nameGeneration;
        private readonly ConnectionRepository _connectionRepository;
        private Socket _listenSocket;

        public ConnectionService(ILogger<ConnectionService> logger, IMediator mediator, NameGenerationService nameGeneration, ConnectionRepository connectionRepository)
        {
            _logger = logger;
            _mediator = mediator;
            _nameGeneration = nameGeneration;
            _connectionRepository = connectionRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(new IPEndPoint(IPAddress.Any, 13337));
            _listenSocket.Listen();
            _logger.LogInformation("Listening for clients...");

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var socket = await _listenSocket.AcceptAsync();

                    string name = _nameGeneration.Generate();
                    while (_connectionRepository.ContainsId(name))
                    {
                        // generate names until there is no longer a conflict
                        name = _nameGeneration.Generate();
                    }

                    // create connection and broadcast join message
                    var connection = new ChatConnection(new NetworkStream(socket), name);
                    var response = new Response
                    {
                        Join = new JoinResponse
                        {
                            UserId = name
                        }
                    };
                    await _connectionRepository.SendToAll(response, cancellationToken);

                    // add connection to repository, send welcome message, and read from socket
                    _connectionRepository.Add(connection);
                    _logger.LogInformation("Connected to client \"{Username}\" (Id: {ConnectionId})", connection.UserId, connection.ConnectionId);
                    await SendWelcomeMessageAsync(connection, cancellationToken);
                    _ = ReadIncomingAsync(connection, cancellationToken);
                }
            }
            finally
            {
                _logger.LogInformation("Closing connections...");
                _listenSocket.Close();
            }
        }

        private async Task SendWelcomeMessageAsync(ChatConnection connection, CancellationToken cancellationToken)
        {
            var response = new Response
            {
                Welcome = new WelcomeResponse
                {
                    UserId = connection.UserId
                }
            };

            foreach (var userId in _connectionRepository.UserIds.Where(id => id != connection.UserId))
            {
                response.Welcome.ConnectedUsers.Add(userId);
            }

            response.WriteTo(connection.Output);
            await connection.Output.FlushAsync(cancellationToken);
        }

        private async Task ReadIncomingAsync(ChatConnection connection, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var result = await connection.Input.ReadAsync(cancellationToken);
                    var command = Command.Parser.ParseFrom(result.Buffer);
                    if (command.CommandCase == CommandType.Exit)
                    {
                        _logger.LogInformation("Client {Id} disconnected", connection.UserId);
                        break;
                    }

                    // todo: implement the chat command types
                    await _mediator.Publish(command.CommandCase switch
                    {
                        CommandType.Echo => new EchoNotification
                        {
                            Command = command.Echo,
                            UserId = connection.UserId
                        },
                        CommandType.Chat => new ChatNotification
                        {
                            Command = command.Chat,
                            UserId = connection.UserId
                        },
                        CommandType.DirectChat => new DirectChatNotification
                        {
                            Command = command.DirectChat,
                            UserId = connection.UserId
                        },
                        _ => throw new Exception($"Unknown command type received: {command.CommandCase}"),
                    }, cancellationToken);

                    connection.Input.AdvanceTo(result.Buffer.End);
                }
            }
            catch (IOException e)
            {
                _logger.LogInformation("Connection with client {Id} unexpectedly closed: {Message}", connection.UserId, e.Message);
            }
            finally
            {
                // close connection, remove from repository
                await connection.CloseAsync();
                _connectionRepository.Remove(connection.UserId);

                // broadcast leave message
                var response = new Response
                {
                    Leave = new LeaveResponse
                    {
                        UserId = connection.UserId
                    }
                };
                await _connectionRepository.SendToAll(response, cancellationToken);
            }
        }
    }
}
