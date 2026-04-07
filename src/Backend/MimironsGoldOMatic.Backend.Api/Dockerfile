# MimironsGoldOMatic.Backend.Api — Linux container image for GHCR releases.
# Build context must be the repository `src/` directory (see release workflow).
# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY MimironsGoldOMatic.Shared/MimironsGoldOMatic.Shared.csproj MimironsGoldOMatic.Shared/
COPY Backend/MimironsGoldOMatic.Backend.Abstract/MimironsGoldOMatic.Backend.Abstract.csproj Backend/MimironsGoldOMatic.Backend.Abstract/
COPY Backend/MimironsGoldOMatic.Backend.Shared/MimironsGoldOMatic.Backend.Shared.csproj Backend/MimironsGoldOMatic.Backend.Shared/
COPY Backend/MimironsGoldOMatic.Backend.Domain/MimironsGoldOMatic.Backend.Domain.csproj Backend/MimironsGoldOMatic.Backend.Domain/
COPY Backend/MimironsGoldOMatic.Backend.DataAccess/MimironsGoldOMatic.Backend.DataAccess.csproj Backend/MimironsGoldOMatic.Backend.DataAccess/
COPY Backend/MimironsGoldOMatic.Backend.Services/MimironsGoldOMatic.Backend.Services.csproj Backend/MimironsGoldOMatic.Backend.Services/
COPY Backend/MimironsGoldOMatic.Backend.Infrastructure/MimironsGoldOMatic.Backend.Infrastructure.csproj Backend/MimironsGoldOMatic.Backend.Infrastructure/
COPY Backend/MimironsGoldOMatic.Backend.Api/MimironsGoldOMatic.Backend.Api.csproj Backend/MimironsGoldOMatic.Backend.Api/

RUN dotnet restore Backend/MimironsGoldOMatic.Backend.Api/MimironsGoldOMatic.Backend.Api.csproj

COPY MimironsGoldOMatic.Shared/ MimironsGoldOMatic.Shared/
COPY Backend/MimironsGoldOMatic.Backend.Abstract/ Backend/MimironsGoldOMatic.Backend.Abstract/
COPY Backend/MimironsGoldOMatic.Backend.Shared/ Backend/MimironsGoldOMatic.Backend.Shared/
COPY Backend/MimironsGoldOMatic.Backend.Domain/ Backend/MimironsGoldOMatic.Backend.Domain/
COPY Backend/MimironsGoldOMatic.Backend.DataAccess/ Backend/MimironsGoldOMatic.Backend.DataAccess/
COPY Backend/MimironsGoldOMatic.Backend.Services/ Backend/MimironsGoldOMatic.Backend.Services/
COPY Backend/MimironsGoldOMatic.Backend.Infrastructure/ Backend/MimironsGoldOMatic.Backend.Infrastructure/
COPY Backend/MimironsGoldOMatic.Backend.Api/ Backend/MimironsGoldOMatic.Backend.Api/

RUN dotnet publish Backend/MimironsGoldOMatic.Backend.Api/MimironsGoldOMatic.Backend.Api.csproj \
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
