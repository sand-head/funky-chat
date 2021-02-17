using FunkyChat.Protos;
using System.IO.Pipelines;

namespace FunkyChat.Server.Models.Commands
{
    public class EchoCommandContext : ICommandContext<EchoMessage>
    {
        private readonly ChatConnection _connection;

        public EchoCommandContext(ChatConnection connection, EchoMessage message)
        {
            _connection = connection;
            Message = message;
        }

        public EchoMessage Message { get; }

        public PipeWriter Output => _connection.Output;
    }
}
