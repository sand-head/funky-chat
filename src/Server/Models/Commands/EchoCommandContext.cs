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
            Command = message;
        }

        public EchoCommand Command { get; }

        public PipeWriter Output => _connection.Output;
    }
}
