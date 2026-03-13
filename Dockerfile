# =========================
# Build Stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Website.Api/Website.Api.csproj Website.Api/
COPY ObserveTool/ObserveTool.csproj ObserveTool/
RUN dotnet restore "Website.Api/Website.Api.csproj"


COPY . .
RUN dotnet publish "Website.Api/Website.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false


# =========================
# Runtime Stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Download curl
RUN apt-get update \
    && apt-get install -y curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Website.Api.dll"]