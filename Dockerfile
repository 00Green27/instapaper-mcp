# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.slnx ./
COPY Directory.Build.props ./
COPY Directory.Packages.props ./
COPY src/Instapaper.Mcp.Server/Instapaper.Mcp.Server.csproj ./src/Instapaper.Mcp.Server/
COPY tests/Instapaper.Mcp.Server.Tests/Instapaper.Mcp.Server.Tests.csproj ./tests/Instapaper.Mcp.Server.Tests/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY src/Instapaper.Mcp.Server/ ./src/Instapaper.Mcp.Server/
COPY tests/Instapaper.Mcp.Server.Tests/ ./tests/Instapaper.Mcp.Server.Tests/

# Build and publish
WORKDIR /src/src/Instapaper.Mcp.Server
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user
RUN useradd -m -u 1000 appuser

# Copy published app
COPY --from=build /app/publish .

# Set ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Set entry point
ENTRYPOINT ["dotnet", "Instapaper.Mcp.Server.dll"]
