using FunkyChat.Server.Models;
using Google.Protobuf.WellKnownTypes;
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
                _listenSocket.Close();
            }
        }

        private async Task ReadIncomingAsync(ChatConnection connection, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await connection.Input.ReadAsync(cancellationToken);
                var message = Any.Parser.ParseFrom(result.Buffer);
                await _mediator.Publish(message);
            }
        }
    }
}
