FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Chet.WebApi.Template.Api/Chet.WebApi.Template.Api.csproj", "Chet.WebApi.Template.Api/"]
COPY ["Chet.WebApi.Template.Core/Chet.WebApi.Template.Contracts/Chet.WebApi.Template.Contracts.csproj", "Chet.WebApi.Template.Core/Chet.WebApi.Template.Contracts/"]
COPY ["Chet.WebApi.Template.Core/Chet.WebApi.Template.Domain/Chet.WebApi.Template.Domain.csproj", "Chet.WebApi.Template.Core/Chet.WebApi.Template.Domain/"]
COPY ["Chet.WebApi.Template.Core/Chet.WebApi.Template.Shared/Chet.WebApi.Template.Shared.csproj", "Chet.WebApi.Template.Core/Chet.WebApi.Template.Shared/"]
COPY ["Chet.WebApi.Template.Infrastructure/Chet.WebApi.Template.Caching/Chet.WebApi.Template.Caching.csproj", "Chet.WebApi.Template.Infrastructure/Chet.WebApi.Template.Caching/"]
COPY ["Chet.WebApi.Template.Infrastructure/Chet.WebApi.Template.Configuration/Chet.WebApi.Template.Configuration.csproj", "Chet.WebApi.Template.Infrastructure/Chet.WebApi.Template.Configuration/"]
COPY ["Chet.WebApi.Template.Infrastructure/Chet.WebApi.Template.Data/Chet.WebApi.Template.Data.csproj", "Chet.WebApi.Template.Infrastructure/Chet.WebApi.Template.Data/"]
COPY ["Chet.WebApi.Template.Infrastructure/Chet.WebApi.Template.Logging/Chet.WebApi.Template.Logging.csproj", "Chet.WebApi.Template.Infrastructure/Chet.WebApi.Template.Logging/"]
COPY ["Chet.WebApi.Template.Application/Chet.WebApi.Template.DTOs/Chet.WebApi.Template.DTOs.csproj", "Chet.WebApi.Template.Application/Chet.WebApi.Template.DTOs/"]
COPY ["Chet.WebApi.Template.Application/Chet.WebApi.Template.Mapping/Chet.WebApi.Template.Mapping.csproj", "Chet.WebApi.Template.Application/Chet.WebApi.Template.Mapping/"]
COPY ["Chet.WebApi.Template.Application/Chet.WebApi.Template.Services/Chet.WebApi.Template.Services.csproj", "Chet.WebApi.Template.Application/Chet.WebApi.Template.Services/"]
RUN dotnet restore "Chet.WebApi.Template.Api/Chet.WebApi.Template.Api.csproj"
COPY . .
WORKDIR "/src/Chet.WebApi.Template.Api"
RUN dotnet build "Chet.WebApi.Template.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Chet.WebApi.Template.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/api/v1/health || exit 1

ENTRYPOINT ["dotnet", "Chet.WebApi.Template.Api.dll"]
