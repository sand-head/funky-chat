using Google.Protobuf;
using MediatR;

namespace FunkyChat.Server.Models.Commands
{
    interface ICommandContext<TProto> : INotification
        where TProto : IMessage<TProto>
    {
        TProto Command { get; }
        ChatConnection Connection { get; }
    }
}
