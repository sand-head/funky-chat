using FunkyChat.Protos;
using Google.Protobuf;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace FunkyChat.Server.Handlers
{
    public class ProtobufNotification<TProto> : INotification
        where TProto : IMessage<TProto>
    {
        public ProtobufNotification(TProto message)
        {
            Message = message;
        }

        public TProto Message { get; }
    }

    public class EchoHandler : INotificationHandler<ProtobufNotification<EchoMessage>>
    {
        private readonly ILogger<EchoHandler> _logger;

        public EchoHandler(ILogger<EchoHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(ProtobufNotification<EchoMessage> notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Echo!!!");
            return Task.CompletedTask;
        }
    }
}
