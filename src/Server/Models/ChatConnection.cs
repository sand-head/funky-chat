using System;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FunkyChat.Server.Models
{
    public record ChatConnection : IDuplexPipe
    {
        private readonly NetworkStream _clientStream;

        public ChatConnection(NetworkStream clientStream, string userId)
        {
            _clientStream = clientStream;

            ConnectionId = Guid.NewGuid();
            UserId = userId;
            Input = PipeReader.Create(_clientStream);
            Output = PipeWriter.Create(_clientStream);
        }

        public Guid ConnectionId { get; }

        public string UserId { get; set; }

        public PipeReader Input { get; }

        public PipeWriter Output { get; }

        public async Task CloseAsync()
        {
            _clientStream.Socket.Shutdown(SocketShutdown.Both);
            _clientStream.Socket.Close();
            await _clientStream.DisposeAsync();
        }
    }
}
