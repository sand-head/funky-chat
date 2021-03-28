using FunkyChat.Protos;
using FunkyChat.Server.Models;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FunkyChat.Server.Infrastructure
{
    public class ConnectionRepository
    {
        private readonly ILogger<ConnectionRepository> _logger;
        private readonly Dictionary<string, ChatConnection> _connections;

        public ConnectionRepository(ILogger<ConnectionRepository> logger)
        {
            _logger = logger;
            _connections = new Dictionary<string, ChatConnection>();
        }

        public ICollection<string> UserIds => _connections.Keys;

        public void Add(ChatConnection connection)
        {
            if (_connections.ContainsKey(connection.UserId))
                throw new Exception("Connection with identifier already exists");
            _connections.Add(connection.UserId, connection);
        }

        public bool ContainsId(string userId)
        {
            return _connections.ContainsKey(userId);
        }

        public void Remove(string userId)
        {
            _connections.Remove(userId);
        }

        /// <summary>
        /// Sends a <see cref="Response"/> to a user based on their <paramref name="userId"/>.
        /// </summary>
        public async Task Send(string userId, Response response, CancellationToken cancellationToken = default)
        {
            if (!_connections.ContainsKey(userId))
                throw new Exception("Connection with identifier does not exist");

            var connection = _connections[userId];
            response.WriteTo(connection.Output);
            await connection.Output.FlushAsync(cancellationToken);
        }

        /// <summary>
        /// Sends a <see cref="Response"/> to all connected users.
        /// </summary>
        public async Task SendToAll(Response response, CancellationToken cancellationToken = default)
        {
            var tasks = _connections.Select(c => Send(c.Key, response, cancellationToken));
            await Task.WhenAll(tasks);
        }
    }
}
