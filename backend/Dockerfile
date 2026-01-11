# Use the official .NET SDK image as base for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy all project files for dependency resolution
COPY TaskManagement.Domain/*.csproj ./TaskManagement.Domain/
COPY TaskManagement.Application/*.csproj ./TaskManagement.Application/
COPY TaskManagement.Infrastructure/*.csproj ./TaskManagement.Infrastructure/
COPY TaskManagement.API/*.csproj ./TaskManagement.API/

# Restore dependencies
RUN dotnet restore TaskManagement.API/TaskManagement.API.csproj

# Copy all source files
COPY TaskManagement.Domain ./TaskManagement.Domain
COPY TaskManagement.Application ./TaskManagement.Application
COPY TaskManagement.Infrastructure ./TaskManagement.Infrastructure
COPY TaskManagement.API ./TaskManagement.API

# Build and publish the application
WORKDIR /src/TaskManagement.API
RUN dotnet build -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManagement.API.dll"]

