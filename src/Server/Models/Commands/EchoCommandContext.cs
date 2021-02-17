using FunkyChat.Protos;
using System.IO.Pipelines;

namespace FunkyChat.Server.Models.Commands
{
    public class EchoCommandContext : ICommandContext<EchoCommand>
    {
        private readonly ChatConnection _connection;

        public EchoCommandContext(ChatConnection connection, EchoCommand message)
        {
            _connection = connection;
            Message = message;
        }

        public EchoCommand Message { get; }

        public PipeWriter Output => _connection.Output;
    }
}
