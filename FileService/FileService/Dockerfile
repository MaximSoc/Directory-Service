# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8002


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG GITHUB_USER
ARG GITHUB_TOKEN

WORKDIR /src
COPY ["FileService/Directory.Packages.props", "FileService/"]
COPY ["FileService/Directory.Build.props", "FileService/"]
COPY ["nuget.config", "."]
COPY ["FileService/FileService/FileService.Web.csproj", "FileService/FileService/"]
COPY ["FileService/FileService.Contracts/FileService.Contracts.csproj", "FileService/FileService.Contracts/"]
COPY ["FileService/FileService.Core/FileService.Core.csproj", "FileService/FileService.Core/"]
COPY ["FileService/FileService.Domain/FileService.Domain.csproj", "FileService/FileService.Domain/"]
COPY ["FileService/FileService.Infrastructure.Postgres/FileService.Infrastructure.Postgres.csproj", "FileService/FileService.Infrastructure.Postgres/"]
COPY ["FileService/FileService.Infrastructure.S3/FileService.Infrastructure.S3.csproj", "FileService/FileService.Infrastructure.S3/"]

RUN dotnet nuget update source github \
    --username $GITHUB_USER \
    --password $GITHUB_TOKEN \
    --store-password-in-clear-text \
    --configfile nuget.config

RUN echo '<?xml version="1.0" encoding="utf-8"?> \
<configuration> \
  <packageSources> \
    <clear /> \
    <add key="gitHub" value="https://nuget.pkg.github.com/'$GITHUB_USER'/index.json" /> \
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" /> \
  </packageSources> \
  <packageSourceMapping> \
    <packageSource key="gitHub"> \
      <package pattern="MaximSoc.*" /> \
      <package pattern="FileService.*" /> \
    </packageSource> \
    <packageSource key="nuget.org"> \
      <package pattern="*" /> \
    </packageSource> \
  </packageSourceMapping> \
  <packageSourceCredentials> \
    <gitHub> \
      <add key="Username" value="'$GITHUB_USER'" /> \
      <add key="ClearTextPassword" value="'$GITHUB_TOKEN'" /> \
    </gitHub> \
  </packageSourceCredentials> \
</configuration>' > nuget.config

RUN dotnet restore "./FileService/FileService/FileService.Web.csproj"
COPY . .
WORKDIR "/src/FileService/FileService"
RUN dotnet build "./FileService.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FileService.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileService.Web.dll"]