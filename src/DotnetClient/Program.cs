using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace FunkyChat.Client
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
                });
    }
}
