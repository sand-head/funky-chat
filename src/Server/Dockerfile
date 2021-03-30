#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
# add buster-backports source
RUN echo "deb http://deb.debian.org/debian buster-backports main" | tee /etc/apt/sources.list.d/backports.list
# add protoc manually (Grpc.Tools doesn't support ARM)
RUN apt update && apt install -y -t buster-backports protobuf-compiler-grpc
# trick Grpc.Tools to use it
ENV PROTOBUF_PROTOC=/usr/bin/protoc
RUN ln -s /usr/bin ~/.nuget/packages/grpc.tools/2.35.0/tools/linux_
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