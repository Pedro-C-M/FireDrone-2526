# ControlBackend Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 5307

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/ControlBackend/ControlBackend.csproj", "src/ControlBackend/"]

# Restore dependencies
RUN dotnet restore "src/ControlBackend/ControlBackend.csproj"

# Copy all source files
COPY src/ControlBackend/ src/ControlBackend/

# Build the project
WORKDIR "/src/src/ControlBackend"
RUN dotnet build "ControlBackend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ControlBackend.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ControlBackend.dll"]
