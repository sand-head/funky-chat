#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Server/FunkyChat.Server.csproj", "Server/"]
RUN dotnet restore "Server/FunkyChat.Server.csproj"
COPY . .
WORKDIR "/src/Server"
RUN dotnet build "FunkyChat.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FunkyChat.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FunkyChat.Server.dll"]