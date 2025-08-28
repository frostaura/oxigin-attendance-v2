#!/bin/bash

# Oxigin Attendance v2 - Docker Setup Script
# This script helps you get started with the containerized application

set -e

echo "🚀 Oxigin Attendance v2 - Docker Setup"
echo "======================================"

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed. Please install Docker first."
    echo "   Visit: https://docs.docker.com/get-docker/"
    exit 1
fi

# Check if Docker Compose is available
if ! command -v docker compose &> /dev/null; then
    echo "❌ Docker Compose is not available. Please ensure you have Docker with Compose plugin."
    exit 1
fi

echo "✅ Docker is installed"

# Function to start development environment
start_dev() {
    echo ""
    echo "🔧 Starting Development Environment..."
    docker compose up -d
    echo ""
    echo "✅ Development environment started!"
    echo ""
    echo "📍 Access your application:"
    echo "   Frontend: http://localhost:3000"
    echo "   Backend API: http://localhost:5000"
    echo "   Database: localhost:5432"
    echo ""
    echo "📝 To view logs: docker compose logs -f"
    echo "🛑 To stop: docker compose down"
}

# Function to start production environment
start_prod() {
    echo ""
    if [ ! -f ".env" ]; then
        echo "⚠️  No .env file found. Copying from .env.example..."
        if [ -f ".env.example" ]; then
            cp .env.example .env
            echo "🔧 Please edit .env file with your production values:"
            echo "   - DB_PASSWORD (secure database password)"
            echo "   - JWT_SECRET_KEY (minimum 32 characters)"
            echo "   - API_URL (your API endpoint)"
            echo ""
            echo "❌ Please update .env file and run this script again."
            exit 1
        else
            echo "❌ .env.example not found. Please create environment configuration."
            exit 1
        fi
    fi
    
    echo "🏭 Starting Production Environment..."
    docker compose -f docker-compose.prod.yml up -d
    echo ""
    echo "✅ Production environment started!"
    echo ""
    echo "📍 Access your application:"
    echo "   Frontend: http://localhost:3000"
    echo "   Backend API: http://localhost:5000"
    echo ""
    echo "📝 To view logs: docker compose -f docker-compose.prod.yml logs -f"
    echo "🛑 To stop: docker compose -f docker-compose.prod.yml down"
}

# Function to build images
build_images() {
    echo ""
    echo "🏗️  Building Docker images..."
    docker compose build
    echo "✅ Images built successfully!"
}

# Function to show status
show_status() {
    echo ""
    echo "📊 Container Status:"
    docker compose ps
    echo ""
    echo "📝 Recent logs:"
    docker compose logs --tail=10
}

# Function to clean up
cleanup() {
    echo ""
    echo "🧹 Cleaning up Docker resources..."
    docker compose down -v
    docker system prune -f
    echo "✅ Cleanup completed!"
}

# Parse command line arguments
case "${1:-help}" in
    "dev" | "development")
        start_dev
        ;;
    "prod" | "production")
        start_prod
        ;;
    "build")
        build_images
        ;;
    "status")
        show_status
        ;;
    "clean" | "cleanup")
        cleanup
        ;;
    "help" | *)
        echo ""
        echo "Usage: $0 [command]"
        echo ""
        echo "Commands:"
        echo "  dev        Start development environment"
        echo "  prod       Start production environment"
        echo "  build      Build Docker images"
        echo "  status     Show container status and logs"
        echo "  clean      Clean up containers and resources"
        echo "  help       Show this help message"
        echo ""
        echo "Examples:"
        echo "  $0 dev     # Start development environment"
        echo "  $0 prod    # Start production environment"
        echo "  $0 status  # Check container status"
        echo ""
        ;;
esac