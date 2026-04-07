# MimironsGoldOMatic.Backend.Api — Linux container image for GHCR releases.
# Build context must be the repository `src/` directory (see release workflow).
# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY MimironsGoldOMatic.Shared/MimironsGoldOMatic.Shared.csproj MimironsGoldOMatic.Shared/
COPY MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Configuration/MimironsGoldOMatic.Backend.Configuration.csproj MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Configuration/
COPY MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Common/MimironsGoldOMatic.Backend.Common.csproj MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Common/
COPY MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Domain/MimironsGoldOMatic.Backend.Domain.csproj MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Domain/
COPY MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.DataAccess/MimironsGoldOMatic.Backend.DataAccess.csproj MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.DataAccess/
COPY MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Services/MimironsGoldOMatic.Backend.Services.csproj MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Services/
COPY MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Infrastructure/MimironsGoldOMatic.Backend.Infrastructure.csproj MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Infrastructure/
COPY MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Api/MimironsGoldOMatic.Backend.Api.csproj MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Api/

RUN dotnet restore MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Api/MimironsGoldOMatic.Backend.Api.csproj

COPY MimironsGoldOMatic.Shared/ MimironsGoldOMatic.Shared/
COPY MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Configuration/ MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Configuration/
COPY MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Common/ MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Common/
COPY MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Domain/ MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Domain/
COPY MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.DataAccess/ MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.DataAccess/
COPY MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Services/ MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Services/
COPY MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Infrastructure/ MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Infrastructure/
COPY MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Api/ MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Api/

RUN dotnet publish MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.Api/MimironsGoldOMatic.Backend.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:SkipGenerateTypeScriptApiClient=true

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "MimironsGoldOMatic.Backend.Api.dll"]
