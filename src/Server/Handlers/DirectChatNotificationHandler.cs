using FunkyChat.Protos;
using FunkyChat.Server.Infrastructure;
using FunkyChat.Server.Models.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace FunkyChat.Server.Handlers
{
    public class DirectChatNotificationHandler : INotificationHandler<DirectChatNotification>
    {
        private readonly ILogger<DirectChatNotificationHandler> _logger;
        private readonly ConnectionRepository _repository;

        public DirectChatNotificationHandler(ILogger<DirectChatNotificationHandler> logger, ConnectionRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task Handle(DirectChatNotification notification, CancellationToken cancellationToken)
        {
            // check to make sure that the message recipient exists
            if (!_repository.ContainsId(notification.Command.UserId))
            {
                // if not, let's just send the user a DM from the "Server" to notify them
                var failResponse = new Response
                {
                    Chat = new ChatResponse
                    {
                        UserId = "Server",
                        Message = "The user you tried to message either is offline or does not exist, sorry!",
                        IsDirect = true
                    }
                };

                await _repository.Send(notification.UserId, failResponse, cancellationToken);
                return;
            }

            _logger.LogDebug("User {UserId} DMed {OtherUser}: {Message}", notification.UserId, notification.Command.UserId, notification.Command.Message);
            var response = new Response
            {
                Chat = new ChatResponse
                {
                    UserId = notification.UserId,
                    Message = notification.Command.Message,
                    IsDirect = true
                }
            };

            // send direct message to both connections, for consistency
            await Task.WhenAll(
                _repository.Send(notification.Command.UserId, response, cancellationToken),
                _repository.Send(notification.UserId, response, cancellationToken));
        }
    }
}
