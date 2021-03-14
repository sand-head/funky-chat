using FunkyChat.Protos;
using FunkyChat.Server.Models.Commands;
using Google.Protobuf;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace FunkyChat.Server.Handlers
{
    public class EchoCommandHandler : INotificationHandler<EchoCommandContext>
    {
        private readonly ILogger<EchoCommandHandler> _logger;

        public EchoCommandHandler(ILogger<EchoCommandHandler> logger)
        {
            _logger = logger;
        }

        public async Task Handle(EchoCommandContext notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Echoing back: {Message}", notification.Command.Message);
            var response = new Response
            {
                Echo = new EchoResponse
                {
                    Message = notification.Command.Message
                }
            };

            response.WriteTo(notification.Connection.Output);
            await notification.Connection.Output.FlushAsync(cancellationToken);
        }
    }
}
