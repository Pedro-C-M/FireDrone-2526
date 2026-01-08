# CentralFrontend Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5305

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file
COPY ["src/CentralFrontend/CentralFrontend.csproj", "src/CentralFrontend/"]

# Restore dependencies
RUN dotnet restore "src/CentralFrontend/CentralFrontend.csproj"

# Copy all source files
COPY . .

# Build the project
WORKDIR "/src/src/CentralFrontend"
RUN dotnet build "CentralFrontend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CentralFrontend.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "CentralFrontend.dll"]
