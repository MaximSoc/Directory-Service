# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
# For more information, please see https://aka.ms/containercompat

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG GITHUB_USER
ARG GITHUB_TOKEN

WORKDIR /src

COPY ["nuget.config", "."]
COPY ["DirectoryService/src/DirectoryService.Presentation/DirectoryService.Presentation.csproj", "DirectoryService/src/DirectoryService.Presentation/"]
COPY ["DirectoryService/src/DirectoryService.Application/DirectoryService.Application.csproj", "DirectoryService/src/DirectoryService.Application/"]
COPY ["DirectoryService/src/DirectoryService.Contracts/DirectoryService.Contracts.csproj", "DirectoryService/src/DirectoryService.Contracts/"]
COPY ["DirectoryService/src/DirectoryService.Domain/DirectoryService.Domain.csproj", "DirectoryService/src/DirectoryService.Domain/"]
COPY ["DirectoryService/src/DirectoryService.Infrastructure/DirectoryService.Infrastructure.csproj", "DirectoryService/src/DirectoryService.Infrastructure/"]

COPY ["DirectoryService/Directory.Build.props", "DirectoryService/"]
COPY ["DirectoryService/Directory.Packages.props", "DirectoryService/"]

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

RUN dotnet restore "./DirectoryService/src/DirectoryService.Presentation/DirectoryService.Presentation.csproj" --configfile ./nuget.config
COPY . .
WORKDIR "/src/DirectoryService/src/DirectoryService.Presentation"
RUN dotnet build "./DirectoryService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DirectoryService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DirectoryService.Presentation.dll"]