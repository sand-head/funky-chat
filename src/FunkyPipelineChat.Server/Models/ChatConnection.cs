using System;
using System.IO.Pipelines;
using System.Net.Sockets;

namespace FunkyPipelineChat.Server.Models
{
    public record ChatConnection : IDuplexPipe
    {
        private readonly NetworkStream _clientStream;

        public ChatConnection(NetworkStream clientStream)
        {
            _clientStream = clientStream;

            ConnectionId = Guid.NewGuid();
            Input = PipeReader.Create(_clientStream);
            Output = PipeWriter.Create(_clientStream);
        }

        public Guid ConnectionId { get; }

        public PipeReader Input { get; }

        public PipeWriter Output { get; }
    }
}
