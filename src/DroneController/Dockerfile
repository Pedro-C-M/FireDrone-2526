# DroneController Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/DroneController/DroneController.csproj", "src/DroneController/"]

# Restore dependencies
RUN dotnet restore "src/DroneController/DroneController.csproj"

# Copy only the source directories required for the build
COPY src/DroneController/ src/DroneController/
COPY src/ControlBackend/ src/ControlBackend/

# Build the project
WORKDIR "/src/src/DroneController"
RUN dotnet build "DroneController.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DroneController.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER app

# Default command with arguments (can be overridden in docker-compose)
ENTRYPOINT ["dotnet", "DroneController.dll"]
CMD ["1", "DroneSimulator"]
