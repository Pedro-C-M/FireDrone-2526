# CentralBackend Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5306

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/Models/Models.csproj", "src/Models/"]
COPY ["src/CentralBackend/CentralBackend.csproj", "src/CentralBackend/"]

# Restore dependencies
RUN dotnet restore "src/CentralBackend/CentralBackend.csproj"

# Copy all source files
COPY Controllers/ ./Controllers/
COPY Services/ ./Services/
COPY Middleware/ ./Middleware/
COPY Exceptions/ ./Exceptions/
COPY Hubs/ ./Hubs/
COPY Migrations/ ./Migrations/
COPY Properties/ ./Properties/

COPY *.cs ./
COPY appsettings.json ./
COPY libman.json ./

# Build the project
WORKDIR "/src/src/CentralBackend"
RUN dotnet build "CentralBackend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CentralBackend.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER root
# Create directory for SQLite database
RUN mkdir -p /app/data && chown -R app:app /app/data
USER app
ENV DATABASE_PATH=/app/data/FireDrone.db

ENTRYPOINT ["dotnet", "CentralBackend.dll"]
