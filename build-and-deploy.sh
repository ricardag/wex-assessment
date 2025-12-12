#!/bin/bash

set -e  # Exit on error

# Get script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo "=== Wex Build and Deploy Script ==="
echo ""

# Start Docker Desktop if not running
echo "Starting Docker Desktop..."
open -a Docker
echo "Waiting for Docker daemon to be ready..."
while ! docker info >/dev/null 2>&1; do
    sleep 1
done
echo "Docker is ready"
echo ""

# Step 1: Clean docker directories
echo "Step 1: Cleaning docker directories..."
rm -rf docker/backend/*
rm -rf docker/frontend/*
mkdir -p docker/backend
mkdir -p docker/frontend
echo "Directories cleaned"
echo ""

# Step 2: Build .NET Backend
echo "Step 2: Building .NET Backend (Release)..."
cd Backend
dotnet publish -c Release -o ../docker/backend
if [ $? -eq 0 ]; then
    echo "Backend built successfully"
    echo ""
else
    echo "Backend build failed"
    exit 1
fi
cd ..

# Step 3: Build Angular Frontend
echo "Step 3: Building Angular Frontend (Production)..."
cd Frontend
npm run build
if [ $? -eq 0 ]; then
    echo "Frontend built successfully"
    echo ""
else
    echo "Frontend build failed"
    exit 1
fi
cd ..

# Step 4: Copy Frontend artifacts
echo "Step 4: Copying Frontend artifacts..."
cp -r Frontend/dist/frontend/browser/* docker/frontend/
if [ $? -eq 0 ]; then
    echo "Frontend artifacts copied"
    echo ""
else
    echo "Failed to copy frontend artifacts"
    exit 1
fi

# Step 5: Build and start Docker containers
echo "Step 5: Building and starting Docker containers..."

# Load environment variables
if [ ! -f .env ]; then
    echo "Error: .env file not found"
    exit 1
fi

if [ ! -r .env ]; then
    echo "Error: .env file is not readable"
    exit 1
fi

# Export environment variables
set -a
source .env
set +a

# Stop and remove existing containers
cd docker
docker-compose down -v 2>/dev/null || true

# Force remove containers by name if they still exist
docker rm -f dotnet_backend nginx_frontend mariadb_db 2>/dev/null || true

docker-compose build
docker-compose up -d

if [ $? -eq 0 ]; then
    echo ""
    echo "Docker containers started successfully"
    echo ""
    echo "=== Deployment Complete ==="
    echo "Frontend: http://localhost:8100"
    echo "Backend API: http://localhost:5190"
    echo "MariaDB: localhost:3306"
    echo ""
    echo "To view logs: cd docker && docker-compose logs -f"
    echo "To stop: cd docker && docker-compose down"
else
    echo "Failed to start Docker containers"
    exit 1
fi
cd ..
