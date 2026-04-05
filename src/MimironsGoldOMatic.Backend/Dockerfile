# MimironsGoldOMatic.Backend — Linux container image for GHCR releases.
# Build context must be the repository `src/` directory (see release workflow).
# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY MimironsGoldOMatic.Shared/MimironsGoldOMatic.Shared.csproj MimironsGoldOMatic.Shared/
COPY MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.csproj MimironsGoldOMatic.Backend/
RUN dotnet restore MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.csproj

COPY MimironsGoldOMatic.Shared/ MimironsGoldOMatic.Shared/
COPY MimironsGoldOMatic.Backend/ MimironsGoldOMatic.Backend/

RUN dotnet publish MimironsGoldOMatic.Backend/MimironsGoldOMatic.Backend.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "MimironsGoldOMatic.Backend.dll"]
