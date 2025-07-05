# This is a multi-stage Dockerfile for building the API Gateway.
# Stage 1: Build the application
# We use the .NET SDK image which contains all the tools needed to build and publish the app.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file first to leverage Docker's layer caching.
# If the project file (and its dependencies) don't change, this layer won't be rebuilt.
COPY ["src/Gateways/ApiGateway/ApiGateway.csproj", "src/Gateways/ApiGateway/"]

# Restore NuGet packages for the project.
RUN dotnet restore "src/Gateways/ApiGateway/ApiGateway.csproj"

# Copy the rest of the source code into the container.
COPY . .

# Build and publish the application for release.
# The output is placed in the /app/publish directory.
WORKDIR "/src/src/Gateways/ApiGateway"
RUN dotnet publish "ApiGateway.csproj" -c Release -o /app/publish --no-restore

# Stage 2: Create the final, smaller runtime image
# We use the ASP.NET runtime image which is much smaller than the SDK image.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy the published artifacts from the build stage.
COPY --from=build /app/publish .

# Expose port 80 (HTTP) and 443 (HTTPS) for the Kestrel web server.
# The actual port mapping is done when running the container (e.g., docker run -p 8080:80).
EXPOSE 80
EXPOSE 443

# Define the entry point for the container. This command will be executed when the container starts.
ENTRYPOINT ["dotnet", "ApiGateway.dll"]