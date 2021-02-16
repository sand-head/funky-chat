using FunkyPipelineChat.Server.Infrastructure;
using FunkyPipelineChat.Server.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace FunkyPipelineChat.Server.Services
{
    public class IncomingConnectionService : BackgroundService
    {
        private readonly ILogger<IncomingConnectionService> _logger;
        private readonly ConnectionRepository _connectionRepository;
        private Socket _listenSocket;

        public IncomingConnectionService(ILogger<IncomingConnectionService> logger, ConnectionRepository connectionRepository)
        {
            _logger = logger;
            _connectionRepository = connectionRepository;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(new IPEndPoint(IPAddress.Loopback, 13337));
            _listenSocket.Listen();
            _logger.LogInformation("Listening for clients...");

            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _listenSocket.Close();

            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var socket = await _listenSocket.AcceptAsync();
                var connection = new ChatConnection(new NetworkStream(socket));
                _logger.LogInformation("Connected to client {ConnectionId}", connection.ConnectionId);
                _connectionRepository.AddConnection(connection);
            }
        }
    }
}
