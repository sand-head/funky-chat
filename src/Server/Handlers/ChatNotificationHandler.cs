using FunkyChat.Protos;
using FunkyChat.Server.Infrastructure;
using FunkyChat.Server.Models.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace FunkyChat.Server.Handlers
{
    public class ChatNotificationHandler : INotificationHandler<ChatNotification>
    {
        private readonly ILogger<ChatNotificationHandler> _logger;
        private readonly ConnectionRepository _repository;

        public ChatNotificationHandler(ILogger<ChatNotificationHandler> logger, ConnectionRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task Handle(ChatNotification notification, CancellationToken cancellationToken)
        {
            _logger.LogDebug("User {UserId} said {Message}.", notification.UserId, notification.Command.Message);
            var response = new Response
            {
                Chat = new ChatResponse
                {
                    UserId = notification.UserId,
                    Message = notification.Command.Message,
                    IsDirect = false
                }
            };

            await _repository.SendToAll(response, cancellationToken);
        }
    }
}
