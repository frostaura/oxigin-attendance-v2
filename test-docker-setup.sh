#!/bin/bash

# Docker Setup Test Script for Oxigin Attendance v2
# This script tests the Docker containerization setup

set -e

echo "🚀 Testing Docker Setup for Oxigin Attendance v2"
echo "=================================================="

# Check prerequisites
echo "📋 Checking prerequisites..."

if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed. Please install Docker first."
    exit 1
fi

if ! command -v docker compose &> /dev/null; then
    echo "❌ Docker Compose is not installed. Please install Docker Compose first."
    exit 1
fi

echo "✅ Docker and Docker Compose are available"
echo "Docker version: $(docker --version)"
echo "Docker Compose version: $(docker compose version)"
echo ""

# Test building individual Docker images
echo "🔨 Testing Docker image builds..."

echo "Building backend image..."
if docker build -t oxigin-backend-test ./backend/OxiginAttendance.Api; then
    echo "✅ Backend image built successfully"
else
    echo "❌ Backend image build failed"
    exit 1
fi

echo "Building frontend image..."
if docker build -t oxigin-frontend-test ./frontend; then
    echo "✅ Frontend image built successfully"  
else
    echo "❌ Frontend image build failed"
    exit 1
fi

echo ""

# Test Docker Compose configuration
echo "🧪 Testing Docker Compose configuration..."

if docker compose config > /dev/null; then
    echo "✅ docker-compose.yml is valid"
else
    echo "❌ docker-compose.yml has configuration errors"
    exit 1
fi

if docker compose -f docker-compose.dev.yml config > /dev/null; then
    echo "✅ docker-compose.dev.yml is valid"
else
    echo "❌ docker-compose.dev.yml has configuration errors"
    exit 1
fi

echo ""

# Test environment files
echo "🔧 Testing environment configuration..."

# Check for example files (these should exist in the repo)
if [[ -f .env.docker.example ]]; then
    echo "✅ .env.docker.example found"
else
    echo "❌ .env.docker.example not found"
    exit 1
fi

if [[ -f .env.docker.dev.example ]]; then
    echo "✅ .env.docker.dev.example found"
else
    echo "❌ .env.docker.dev.example not found"
    exit 1
fi

# Check if user has set up their environment files
if [[ -f .env.docker ]]; then
    echo "✅ .env.docker found"
else
    echo "⚠️  .env.docker not found - you need to copy from .env.docker.example and configure"
    echo "   Run: cp .env.docker.example .env.docker"
    echo "   Then edit .env.docker with your secure credentials"
fi

if [[ -f .env.docker.dev ]]; then
    echo "✅ .env.docker.dev found"
else
    echo "⚠️  .env.docker.dev not found - you need to copy from .env.docker.dev.example and configure"
    echo "   Run: cp .env.docker.dev.example .env.docker.dev"
    echo "   Then edit .env.docker.dev with your secure credentials"
fi

# Security check - warn if environment files might contain default values
if [[ -f .env.docker ]]; then
    if grep -q "YOUR_" .env.docker; then
        echo "⚠️  Warning: .env.docker contains placeholder values - please replace with secure credentials"
    fi
fi

if [[ -f .env.docker.dev ]]; then
    if grep -q "YOUR_" .env.docker.dev; then
        echo "⚠️  Warning: .env.docker.dev contains placeholder values - please replace with secure credentials"
    fi
fi

echo ""

# Test that all required files exist
echo "📁 Checking required Docker files..."

declare -a required_files=(
    "backend/OxiginAttendance.Api/Dockerfile"
    "backend/OxiginAttendance.Api/Dockerfile.dev"
    "backend/OxiginAttendance.Api/.dockerignore"
    "frontend/Dockerfile"
    "frontend/Dockerfile.dev"
    "frontend/.dockerignore"
    "frontend/nginx.conf"
    "docker-compose.yml"
    "docker-compose.dev.yml"
    ".env.docker.example"
    ".env.docker.dev.example"
    "DOCKER.md"
)

for file in "${required_files[@]}"; do
    if [[ -f "$file" ]]; then
        echo "✅ $file exists"
    else
        echo "❌ $file is missing"
        exit 1
    fi
done

echo ""

# Cleanup test images
echo "🧹 Cleaning up test images..."
docker rmi oxigin-backend-test oxigin-frontend-test || true

echo ""
echo "🎉 All Docker setup tests passed!"
echo ""
echo "📖 Next steps:"
echo "   1. Set up environment files:"
echo "      cp .env.docker.example .env.docker"
echo "      cp .env.docker.dev.example .env.docker.dev"
echo "      Edit both files with secure credentials"
echo "   2. Start the application: docker compose up --build"
echo "   3. For development: docker compose -f docker-compose.dev.yml up --build"
echo "   4. Access frontend at: http://localhost:3000"
echo "   5. Access backend at: http://localhost:5000"
echo "   6. View API docs at: http://localhost:5000/swagger"
echo ""
echo "📚 For more information, see DOCKER.md"