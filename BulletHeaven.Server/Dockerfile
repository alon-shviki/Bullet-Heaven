# ── Stage 1: build ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY BulletHeaven.Server/BulletHeaven.Server.csproj BulletHeaven.Server/
RUN dotnet restore BulletHeaven.Server/BulletHeaven.Server.csproj

COPY BulletHeaven.Server/ BulletHeaven.Server/
RUN dotnet publish BulletHeaven.Server/BulletHeaven.Server.csproj \
      -c Release \
      -o /publish \
      --no-restore

# ── Stage 2: runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS serve
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "BulletHeaven.Server.dll"]
