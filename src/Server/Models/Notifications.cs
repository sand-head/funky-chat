using FunkyChat.Protos;
using Google.Protobuf;
using MediatR;

namespace FunkyChat.Server.Models.Notifications
{
    public abstract record CommandNotification<TProto> : INotification
        where TProto : IMessage<TProto>
    {
        public TProto Command { get; init; }
        public string UserId { get; init; }
    }

    public record EchoNotification : CommandNotification<EchoCommand>;
    public record ChatNotification : CommandNotification<ChatCommand>;
    public record DirectChatNotification : CommandNotification<DirectChatCommand>;
}
