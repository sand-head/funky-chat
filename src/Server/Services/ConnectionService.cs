using FunkyChat.Protos;
using FunkyChat.Server.Models;
using FunkyChat.Server.Models.Commands;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await connection.Input.ReadAsync(cancellationToken);
                // todo: maybe have a wrapper around the messages?
                // like have a protobuf "Command" message
                // that contains "oneof" the other message types
                var message = EchoMessage.Parser.ParseFrom(result.Buffer);
                
                if (message is EchoMessage echoMessage)
                {
                    await _mediator.Publish(new EchoCommandContext(connection, echoMessage));
                }

                connection.Input.AdvanceTo(result.Buffer.End);
            }
        }
    }
}
