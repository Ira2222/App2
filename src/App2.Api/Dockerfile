# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/App2.Api/*.csproj src/App2.Api/
COPY src/App2.Domain/*.csproj src/App2.Domain/
COPY src/App2.Application/*.csproj src/App2.Application/
COPY src/App2.Infrastructure/*.csproj src/App2.Infrastructure/
RUN dotnet restore src/App2.Api/App2.Api.csproj

COPY . .
RUN dotnet publish src/App2.Api/App2.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Install curl to enable container health probes
RUN apt-get update \
 && apt-get install -y --no-install-recommends curl \
 && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_EnableDiagnostics=0
EXPOSE 8080
HEALTHCHECK --interval=10s --timeout=3s --start-period=20s --retries=5 \
  CMD curl -fsS http://localhost:8080/healthz/ready || exit 1
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "App2.Api.dll"]
