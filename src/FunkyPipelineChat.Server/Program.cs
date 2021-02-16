using FunkyPipelineChat.Server.Infrastructure;
using FunkyPipelineChat.Server.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace FunkyPipelineChat.Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            await host.RunAsync();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // configure services here
                    services.AddSingleton<ConnectionRepository>();
                    services.AddHostedService<IncomingConnectionService>();
                });
    }
}
