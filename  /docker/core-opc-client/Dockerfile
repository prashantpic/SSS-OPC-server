# Stage 1: Build the application
# This stage uses the .NET SDK to build and publish the application.
# It leverages layer caching by copying the project file first and restoring dependencies.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
# This is a separate layer to optimize Docker's build cache
COPY ["src/Services/CoreOpcClient/CoreOpcClient.csproj", "src/Services/CoreOpcClient/"]
# If a solution file is used, it should be copied and restored here as well.
# COPY ["SSS-OPC-Platform.sln", "./"]
RUN dotnet restore "src/Services/CoreOpcClient/CoreOpcClient.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/src/Services/CoreOpcClient"

# Build the application in Release configuration
RUN dotnet build "CoreOpcClient.csproj" -c Release -o /app/build

# Stage 2: Publish the application
# This stage creates the final, optimized output for running the application.
FROM build AS publish
RUN dotnet publish "CoreOpcClient.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Create the final runtime image
# This stage uses the lightweight ASP.NET runtime image for a smaller footprint.
# It includes security best practices like creating and using a non-root user.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy the published application from the 'publish' stage
COPY --from=publish /app/publish .

# Create a non-root user for security purposes
# Running as a non-root user is a critical security measure for containers.
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Set the entry point for the container
ENTRYPOINT ["dotnet", "CoreOpcClient.dll"]