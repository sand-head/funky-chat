using FunkyChat.Server.Infrastructure;
using FunkyChat.Server.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace FunkyChat.Server
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
                    var adjectives = ReadEmbeddedList("FunkyChat.Server.Data.adjectives.txt");
                    var nouns = ReadEmbeddedList("FunkyChat.Server.Data.nouns.txt");

                    services.AddSingleton(new NameGenerationService(adjectives, nouns))
                        .AddSingleton<ConnectionRepository>();
                    services.AddMediatR(typeof(Program));
                    services.AddHostedService<ConnectionService>();
                });

        private static List<string> ReadEmbeddedList(string resourceName)
        {
            var assembly = typeof(Program).GetTypeInfo().Assembly;
            var resource = assembly.GetManifestResourceStream(resourceName);

            var list = new List<string>();
            using var reader = new StreamReader(resource);
            while (reader.Peek() >= 0)
            {
                list.Add(reader.ReadLine());
            }
            return list;
        }
    }
}
