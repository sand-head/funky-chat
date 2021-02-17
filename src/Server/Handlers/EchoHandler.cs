using FunkyChat.Protos;
using FunkyChat.Server.Models.Commands;
using Google.Protobuf;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace FunkyChat.Server.Handlers
{
    public class EchoHandler : INotificationHandler<EchoCommandContext>
    {
        private readonly ILogger<EchoHandler> _logger;

        public EchoHandler(ILogger<EchoHandler> logger)
        {
            _logger = logger;
        }

        public async Task Handle(EchoCommandContext notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Echoing back: {Message}", notification.Message.Message);
            var command = new Command
            {
                Echo = new EchoCommand
                {
                    Message = notification.Message.Message
                }
            };

            command.WriteTo(notification.Output);
            await notification.Output.FlushAsync(cancellationToken);
        }
    }
}
