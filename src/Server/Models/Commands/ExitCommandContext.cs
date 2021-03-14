using FunkyChat.Protos;
using System.IO.Pipelines;

namespace FunkyChat.Server.Models.Commands
{
    public class ExitCommandContext : ICommandContext<ExitCommand>
    {
        private readonly ChatConnection _connection;

        public ExitCommandContext(ChatConnection connection, ExitCommand message)
        {
            _connection = connection;
            Command = message;
        }

        public ExitCommand Command { get; }

        public PipeWriter Output => _connection.Output;

        public void Close()
        {
            _connection.Close();
        }
    }
}
