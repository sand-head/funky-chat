using FunkyPipelineChat.Server.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FunkyPipelineChat.Server.Infrastructure
{
    public class ConnectionRepository
    {
        private readonly ILogger<ConnectionRepository> _logger;
        private readonly HashSet<ChatConnection> _connections;

        public ConnectionRepository(ILogger<ConnectionRepository> logger)
        {
            _logger = logger;
        }

        public void AddConnection(ChatConnection connection)
        {
            _connections.Add(connection);
        }

        public ChatConnection GetConnectionById(Guid connectionId)
        {
            return _connections.FirstOrDefault(c => c.ConnectionId == connectionId)
                ?? throw new KeyNotFoundException("Connection could not be found by the given ID.");
        }
    }
}
