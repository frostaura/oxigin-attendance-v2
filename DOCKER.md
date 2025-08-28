# Docker Setup for Oxigin Attendance v2

This document provides instructions for running the Oxigin Attendance application using Docker.

## Prerequisites

- Docker and Docker Compose
- Git (to clone the repository)

## Quick Start

1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   cd oxigin-attendance-v2
   ```

2. **Start the application with Docker Compose:**
   ```bash
   docker compose up --build
   ```

   This will start:
   - PostgreSQL database on port 5432
   - Backend API on http://localhost:5000
   - Frontend React app on http://localhost:3000

3. **Access the application:**
   - Frontend: http://localhost:3000
   - Backend API: http://localhost:5000
   - Swagger API docs: http://localhost:5000/swagger

## Available Docker Compose Files

### Production Build (`docker-compose.yml`)
- Uses optimized production builds
- Nginx serves static frontend files
- Suitable for production deployment

```bash
docker compose up --build
```

### Development Build (`docker-compose.dev.yml`)
- Includes live reload for both frontend and backend
- Mounts source code as volumes for development
- Suitable for active development

```bash
docker compose -f docker-compose.dev.yml up --build
```

## Environment Variables

The application uses the following environment variables:

### Backend
- `ASPNETCORE_ENVIRONMENT`: Set to "Development" or "Production"
- `ConnectionStrings__DefaultConnection`: PostgreSQL connection string
- `JwtSettings__SecretKey`: JWT signing key

### Frontend
- `REACT_APP_API_URL`: Backend API URL (defaults to http://localhost:5000/api)

## Database Setup

The PostgreSQL database is automatically initialized when starting with Docker Compose. The backend will automatically run Entity Framework migrations on startup.

### Default Admin Account
- **Email**: admin@oxigin.com  
- **Password**: Admin@123

## Docker Commands

### Start all services
```bash
docker compose up --build
```

### Start in background
```bash
docker compose up -d --build
```

### Stop all services
```bash
docker compose down
```

### Stop and remove volumes (clean database)
```bash
docker compose down -v
```

### View logs
```bash
docker compose logs -f [service-name]
```

### Rebuild a specific service
```bash
docker compose build [service-name]
docker compose up --no-deps [service-name]
```

## Development Workflow

For active development, use the development compose file:

1. **Start development environment:**
   ```bash
   docker compose -f docker-compose.dev.yml up --build
   ```

2. **Make changes to your code** - changes will be automatically reloaded

3. **View logs for debugging:**
   ```bash
   docker compose -f docker-compose.dev.yml logs -f backend
   docker compose -f docker-compose.dev.yml logs -f frontend
   ```

## Troubleshooting

### Port Conflicts
If you get port conflicts, ensure ports 3000, 5000, and 5432 are available, or modify the ports in the docker-compose files.

### Database Connection Issues
- Ensure PostgreSQL container is healthy: `docker compose ps`
- Check database logs: `docker compose logs postgres`
- The backend waits for the database to be ready via health checks

### Build Issues
- Clear Docker cache: `docker system prune -a`
- Remove volumes: `docker compose down -v`
- Rebuild from scratch: `docker compose build --no-cache`

## Production Deployment

For production deployment:

1. **Update environment variables** in `docker-compose.yml`
2. **Use production builds:**
   ```bash
   docker compose -f docker-compose.yml up --build -d
   ```
3. **Set up reverse proxy** (nginx, Apache, etc.) if needed
4. **Configure SSL/TLS** certificates
5. **Set up database backups**

## File Structure

```
├── backend/
│   └── OxiginAttendance.Api/
│       ├── Dockerfile          # Production backend image
│       ├── Dockerfile.dev      # Development backend image
│       └── .dockerignore
├── frontend/
│   ├── Dockerfile              # Production frontend image  
│   ├── Dockerfile.dev          # Development frontend image
│   ├── nginx.conf              # Nginx configuration
│   └── .dockerignore
├── docker-compose.yml          # Production compose file
├── docker-compose.dev.yml      # Development compose file
└── DOCKER.md                   # This documentation
```