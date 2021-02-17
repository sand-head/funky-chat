using Google.Protobuf;
using MediatR;
using System.IO.Pipelines;

namespace FunkyChat.Server.Models.Commands
{
    interface ICommandContext<TProto> : INotification
        where TProto : IMessage<TProto>
    {
        TProto Message { get; }
        PipeWriter Output { get; }
    }
}
