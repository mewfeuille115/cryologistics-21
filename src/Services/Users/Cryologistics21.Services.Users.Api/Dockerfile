# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN apt-get update \
	&& apt-get install -y --no-install-recommends --only-upgrade apt libapt-pkg6.0t64 libpcre2-8-0 libc6 libc-bin perl-base \
	&& rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["src/Services/Users/Cryologistics21.Services.Users.Api/Cryologistics21.Services.Users.Api.csproj", "src/Services/Users/Cryologistics21.Services.Users.Api/"]
COPY ["src/Common/Cryologistics21.Common.ServiceDefaults/Cryologistics21.Common.ServiceDefaults.csproj", "src/Common/Cryologistics21.Common.ServiceDefaults/"]
COPY ["src/Services/Users/Cryologistics21.Services.Users.Infrastructure/Cryologistics21.Services.Users.Infrastructure.csproj", "src/Services/Users/Cryologistics21.Services.Users.Infrastructure/"]
COPY ["src/Common/Cryologistics21.Common.Infrastructure/Cryologistics21.Common.Infrastructure.csproj", "src/Common/Cryologistics21.Common.Infrastructure/"]
COPY ["src/Common/Cryologistics21.Common.Application/Cryologistics21.Common.Application.csproj", "src/Common/Cryologistics21.Common.Application/"]
COPY ["src/Common/Cryologistics21.Common.Domain/Cryologistics21.Common.Domain.csproj", "src/Common/Cryologistics21.Common.Domain/"]
COPY ["src/Services/Users/Cryologistics21.Services.Users.Presentation/Cryologistics21.Services.Users.Presentation.csproj", "src/Services/Users/Cryologistics21.Services.Users.Presentation/"]
COPY ["src/Common/Cryologistics21.Common.Presentation/Cryologistics21.Common.Presentation.csproj", "src/Common/Cryologistics21.Common.Presentation/"]
COPY ["src/Services/Users/Cryologistics21.Services.Users.Application/Cryologistics21.Services.Users.Application.csproj", "src/Services/Users/Cryologistics21.Services.Users.Application/"]
COPY ["src/Services/Users/Cryologistics21.Services.Users.Domain/Cryologistics21.Services.Users.Domain.csproj", "src/Services/Users/Cryologistics21.Services.Users.Domain/"]
RUN dotnet restore "./src/Services/Users/Cryologistics21.Services.Users.Api/Cryologistics21.Services.Users.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/Users/Cryologistics21.Services.Users.Api"
RUN dotnet build "./Cryologistics21.Services.Users.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Cryologistics21.Services.Users.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cryologistics21.Services.Users.Api.dll"]
