# Use the official .NET SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /app

# Copy the project files
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application files
COPY . .

# Copy and rename appsettings.Docker.json as appsettings.json
COPY appsettings.Docker.json ./appsettings.json

# Build the application
RUN dotnet publish -c Release -o out

# Use the official .NET runtime image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Expose port
EXPOSE 80

# Start the application
ENTRYPOINT ["dotnet", "MockTestApi.dll"]
