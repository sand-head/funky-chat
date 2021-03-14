using FunkyChat.Protos;

namespace FunkyChat.Server.Models.Commands
{
    public class EchoCommandContext : ICommandContext<EchoCommand>
    {
        public EchoCommand Command { get; init; }

        public ChatConnection Connection { get; init; }
    }
}
