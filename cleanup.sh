#!/bin/bash
# Cleanup script - removes all Docker resources created by this project
# Run this when you're done and want to remove everything

set -e

echo "🧹 Cleaning up Soccer Match Simulator Docker resources..."

# Stop and remove container
echo "Stopping container..."
docker compose down -v 2>/dev/null || true

# Remove the Docker image (optional - uncomment if you want to remove it)
echo "Removing .NET SDK image..."
docker rmi mcr.microsoft.com/dotnet/sdk:8.0 2>/dev/null || true

# Clean up any dangling images/volumes
echo "Pruning unused Docker resources..."
docker system prune -f 2>/dev/null || true

echo ""
echo "✅ Docker cleanup complete!"
echo ""
echo "To also delete the project files, run:"
echo "  rm -rf /Users/danielmurillo/Documents/Personal/SoccerMatchSimulator"
