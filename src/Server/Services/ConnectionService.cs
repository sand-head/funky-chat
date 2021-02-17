using FunkyChat.Protos;
using FunkyChat.Server.Models;
using FunkyChat.Server.Models.Commands;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace FunkyChat.Server.Services
{
    public class ConnectionService : BackgroundService
    {
        private readonly ILogger<ConnectionService> _logger;
        private readonly IMediator _mediator;
        private Socket _listenSocket;

        public ConnectionService(ILogger<ConnectionService> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
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
                    var connection = new ChatConnection(new NetworkStream(socket));
                    _logger.LogInformation("Connected to client {ConnectionId}", connection.ConnectionId);
                    // todo: find a better way to do this:
                    _ = ReadIncomingAsync(connection, cancellationToken);
                }
            }
            finally
            {
                _logger.LogInformation("Closing connections...");
                _listenSocket.Close();
            }
        }

        private async Task ReadIncomingAsync(ChatConnection connection, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var result = await connection.Input.ReadAsync(cancellationToken);
                    // todo: maybe have a wrapper around the messages?
                    // like have a protobuf "Command" message
                    // that contains "oneof" the other message types
                    var command = Command.Parser.ParseFrom(result.Buffer);

                    switch (command.CommandCase)
                    {
                        case Command.CommandOneofCase.Echo:
                            await _mediator.Publish(new EchoCommandContext(connection, command.Echo));
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
