# MimironsGoldOMatic.Backend.Api — Linux container image for GHCR releases.
# Build context must be the repository `src/` directory (see release workflow).
# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY MimironsGoldOMatic.Shared/MimironsGoldOMatic.Shared.csproj MimironsGoldOMatic.Shared/
COPY MimironsGoldOMatic.Backend.Abstract/MimironsGoldOMatic.Backend.Abstract.csproj MimironsGoldOMatic.Backend.Abstract/
COPY MimironsGoldOMatic.Backend.Shared/MimironsGoldOMatic.Backend.Shared.csproj MimironsGoldOMatic.Backend.Shared/
COPY MimironsGoldOMatic.Backend.Domain/MimironsGoldOMatic.Backend.Domain.csproj MimironsGoldOMatic.Backend.Domain/
COPY MimironsGoldOMatic.Backend.DataAccess/MimironsGoldOMatic.Backend.DataAccess.csproj MimironsGoldOMatic.Backend.DataAccess/
COPY MimironsGoldOMatic.Backend.Services/MimironsGoldOMatic.Backend.Services.csproj MimironsGoldOMatic.Backend.Services/
COPY MimironsGoldOMatic.Backend.Infrastructure/MimironsGoldOMatic.Backend.Infrastructure.csproj MimironsGoldOMatic.Backend.Infrastructure/
COPY MimironsGoldOMatic.Backend.Api/MimironsGoldOMatic.Backend.Api.csproj MimironsGoldOMatic.Backend.Api/

RUN dotnet restore MimironsGoldOMatic.Backend.Api/MimironsGoldOMatic.Backend.Api.csproj

COPY MimironsGoldOMatic.Shared/ MimironsGoldOMatic.Shared/
COPY MimironsGoldOMatic.Backend.Abstract/ MimironsGoldOMatic.Backend.Abstract/
COPY MimironsGoldOMatic.Backend.Shared/ MimironsGoldOMatic.Backend.Shared/
COPY MimironsGoldOMatic.Backend.Domain/ MimironsGoldOMatic.Backend.Domain/
COPY MimironsGoldOMatic.Backend.DataAccess/ MimironsGoldOMatic.Backend.DataAccess/
COPY MimironsGoldOMatic.Backend.Services/ MimironsGoldOMatic.Backend.Services/
COPY MimironsGoldOMatic.Backend.Infrastructure/ MimironsGoldOMatic.Backend.Infrastructure/
COPY MimironsGoldOMatic.Backend.Api/ MimironsGoldOMatic.Backend.Api/

RUN dotnet publish MimironsGoldOMatic.Backend.Api/MimironsGoldOMatic.Backend.Api.csproj \
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
