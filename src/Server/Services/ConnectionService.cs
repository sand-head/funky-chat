using FunkyChat.Protos;
using FunkyChat.Server.Models;
using FunkyChat.Server.Models.Commands;
using Google.Protobuf;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
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
        private Socket _listenSocket;

        public ConnectionService(ILogger<ConnectionService> logger, IMediator mediator, NameGenerationService nameGeneration)
        {
            _logger = logger;
            _mediator = mediator;
            _nameGeneration = nameGeneration;
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
                    var connection = new ChatConnection(new NetworkStream(socket), _nameGeneration.Generate());
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

                    switch (command.CommandCase)
                    {
                        case CommandType.Echo:
                            await _mediator.Publish(new EchoCommandContext(connection, command.Echo), cancellationToken);
                            break;
                    }

                    connection.Input.AdvanceTo(result.Buffer.End);
                }
            }
            catch (IOException e)
            {
                _logger.LogInformation("Connection with client {Id} closed: {Message}", connection.ConnectionId, e.Message);
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
