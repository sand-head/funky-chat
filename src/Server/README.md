# Server

The server application, written in C# using .NET 5.0.

## Running locally

### Command line

1. Make sure the [.NET 5.0 SDK](https://dotnet.microsoft.com/download/dotnet/5.0) is installed.
2. Run `dotnet restore` in this project's directory to restore NuGet packages.
3. Run `dotnet build -c Release` to build the server for release.
4. Finally, run `dotnet publish -c Release` to publish the server, and run the executable in the publish directory as seen in the terminal.

### Using Visual Studio

1. Install Visual Studio.
2. Open the `FunkyChat.Server.csproj` file in the project directory.
3. Hit F5 to run the server.

## Resources used
* https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-usage
* https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-5.0&tabs=visual-studio#backgroundservice-base-class
* https://www.c-sharpcorner.com/article/command-mediator-pattern-in-asp-net-core-using-mediatr2/
* https://developers.google.com/protocol-buffers/docs/csharptutorial