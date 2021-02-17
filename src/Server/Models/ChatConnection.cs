using System;
using System.IO.Pipelines;
using System.Net.Sockets;

namespace FunkyChat.Server.Models
{
    public record ChatConnection : IDuplexPipe
    {
        private readonly NetworkStream _clientStream;

        public ChatConnection(NetworkStream clientStream, string username)
        {
            _clientStream = clientStream;

            ConnectionId = Guid.NewGuid();
            Username = username;
            Input = PipeReader.Create(_clientStream);
            Output = PipeWriter.Create(_clientStream);
        }

        public Guid ConnectionId { get; }

        public string Username { get; set; }

        public PipeReader Input { get; }

        public PipeWriter Output { get; }

        public void Close() => _clientStream.Close();
    }
}
