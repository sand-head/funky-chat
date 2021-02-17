using Google.Protobuf;
using MediatR;
using System.IO.Pipelines;

namespace FunkyChat.Server.Models.Commands
{
    interface ICommandContext<TProto> : INotification
        where TProto : IMessage<TProto>
    {
        TProto Command { get; }
        PipeWriter Output { get; }
    }
}
