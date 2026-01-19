#!/bin/bash
# Development helper script - runs dotnet commands in Docker
# Usage: ./dev.sh <command>
#
# Examples:
#   ./dev.sh new console -n SoccerMatchSimulator -o src/SoccerMatchSimulator
#   ./dev.sh build
#   ./dev.sh run --project src/SoccerMatchSimulator
#   ./dev.sh test
#   ./dev.sh add package Spectre.Console --project src/SoccerMatchSimulator

set -e

PROJECT_DIR="$(cd "$(dirname "$0")" && pwd)"
CONTAINER_NAME="soccer-simulator-dev"

# Check if container is running
if ! docker ps --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
    echo "Starting development container..."
    docker compose up -d
    sleep 2
fi

# Run dotnet command in container
# Use -t only if we have a TTY
if [ -t 0 ]; then
    docker exec -it $CONTAINER_NAME dotnet "$@"
else
    docker exec -i $CONTAINER_NAME dotnet "$@"
fi
