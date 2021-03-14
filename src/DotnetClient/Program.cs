using FunkyChat.Protos;
using Google.Protobuf;
using System;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FunkyChat.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            Console.WriteLine("Connecting to server at port 13337...");
            clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, 13337));

            var stream = new NetworkStream(clientSocket);
            var reader = PipeReader.Create(stream);
            var writer = PipeWriter.Create(stream);

            // read welcome response for username
            var welcomeResult = await reader.ReadAsync();
            var welcomeResponse = Response.Parser.ParseFrom(welcomeResult.Buffer);
            var userId = welcomeResponse.Welcome.UserId;
            reader.AdvanceTo(welcomeResult.Buffer.End);
            Console.WriteLine($"Connected! Welcome, {userId}!");

            // display connected users from welcome message:
            var users = welcomeResponse.Welcome.ConnectedUsers.Count > 0
                ? string.Join(", ", welcomeResponse.Welcome.ConnectedUsers)
                : "None";
            Console.WriteLine($"Online users: {users}");

            while (true)
            {
                Console.Write("> ");
                var input = Console.ReadLine();
                if (input.StartsWith(".exit"))
                {
                    Console.WriteLine("Closing connection...");
                    var exitCommand = new Command
                    {
                        Exit = new ExitCommand()
                    };
                    exitCommand.WriteTo(writer);
                    await writer.FlushAsync();
                    break;
                }

                var command = new Command
                {
                    Echo = new EchoCommand
                    {
                        Message = input
                    }
                };
                command.WriteTo(writer);
                await writer.FlushAsync();

                var result = await reader.ReadAsync();
                var response = Response.Parser.ParseFrom(result.Buffer);
                reader.AdvanceTo(result.Buffer.End);
                Console.WriteLine($"< {response.Echo.Message}");
            }
        }
    }
}
