using FunkyChat.Protos;
using FunkyChat.Server.Infrastructure;
using FunkyChat.Server.Models.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace FunkyChat.Server.Handlers
{
    public class EchoNotificationHandler : INotificationHandler<EchoNotification>
    {
        private readonly ILogger<EchoNotificationHandler> _logger;
        private readonly ConnectionRepository _repository;

        public EchoNotificationHandler(ILogger<EchoNotificationHandler> logger, ConnectionRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task Handle(EchoNotification notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Echoing back: {Message}", notification.Command.Message);
            var response = new Response
            {
                Echo = new EchoResponse
                {
                    Message = notification.Command.Message
                }
            };

            await _repository.Send(notification.UserId, response, cancellationToken);
        }
    }
}
