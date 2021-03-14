using FunkyChat.Server.Models.Commands;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace FunkyChat.Server.Handlers
{
    public class ExitCommandHandler : INotificationHandler<ExitCommandContext>
    {
        private readonly ILogger<ExitCommandHandler> _logger;

        public ExitCommandHandler(ILogger<ExitCommandHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(ExitCommandContext notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Closing connection for client...");
            notification.Close();
            return Task.CompletedTask;
        }
    }
}
