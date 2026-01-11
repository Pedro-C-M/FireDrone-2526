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
COPY . .

# Build the project
WORKDIR "/src/src/CentralBackend"
RUN dotnet build "CentralBackend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CentralBackend.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create directory for SQLite database
RUN mkdir -p /app/data

ENTRYPOINT ["dotnet", "CentralBackend.dll"]
