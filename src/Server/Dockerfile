#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ARG PROTOBUF_VERSION=3.15.6
# ARG ARCH=x86_64

# add protoc manually (Grpc.Tools doesn't support ARM... yet)
WORKDIR /protoc
RUN apt-get update && apt-get install -y unzip
RUN case "$TARGETARCH" in \
        "arm64") arch="aarch_64" ;; \
        *) arch="x86_64" ;; \
    esac \
    && wget "https://github.com/google/protobuf/releases/download/v$PROTOBUF_VERSION/protoc-$PROTOBUF_VERSION-linux-$arch.zip" -O protoc.zip \
    && unzip protoc.zip \
    && chmod a+rx ./bin/protoc
# trick Grpc.Tools to use it
ENV PROTOBUF_PROTOC=/protoc/bin/protoc

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