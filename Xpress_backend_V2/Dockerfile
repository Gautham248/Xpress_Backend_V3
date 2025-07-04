# Base image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy and restore
COPY ["Xpress_backend_V2.csproj", "./"]
RUN dotnet restore "./Xpress_backend_V2.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "Xpress_backend_V2.csproj" -c Release -o /app/build

# Publish the build
RUN dotnet publish "Xpress_backend_V2.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Xpress_backend_V2.dll"]
