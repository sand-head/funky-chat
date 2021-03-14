using FunkyChat.Protos;
using FunkyChat.Server.Models;
using FunkyChat.Server.Models.Commands;
using Google.Protobuf;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        private readonly Dictionary<string, ChatConnection> _clients;
        private Socket _listenSocket;

        public ConnectionService(ILogger<ConnectionService> logger, IMediator mediator, NameGenerationService nameGeneration)
        {
            _logger = logger;
            _mediator = mediator;
            _nameGeneration = nameGeneration;
            _clients = new Dictionary<string, ChatConnection>();
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(new IPEndPoint(IPAddress.Loopback, 13337));
            _listenSocket.Listen();
            _logger.LogInformation("Listening for clients...");

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var socket = await _listenSocket.AcceptAsync();

                    string name = _nameGeneration.Generate();
                    while (_clients.ContainsKey(name))
                    {
                        // generate names until there is no longer a conflict
                        name = _nameGeneration.Generate();
                    }

                    var connection = new ChatConnection(new NetworkStream(socket), name);
                    _clients.Add(name, connection);
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

            foreach (var userId in _clients.Keys.Where(id => id != connection.UserId))
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
                        CommandType.Echo => new EchoCommandContext
                        {
                            Command = command.Echo,
                            Connection = connection
                        },
                        CommandType.Chat => throw new System.NotImplementedException(),
                        CommandType.DirectChat => throw new System.NotImplementedException(),
                        _ => throw new System.NotImplementedException(),
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
                await connection.CloseAsync();
                _clients.Remove(connection.UserId);
            }
        }
    }
}
