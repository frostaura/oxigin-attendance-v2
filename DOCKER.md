# Docker Setup for Oxigin Attendance v2

This guide explains how to run the Oxigin Attendance v2 application using Docker and Docker Compose.

## Prerequisites

- Docker and Docker Compose installed
- Git

## Quick Start

### Development Environment

1. Clone the repository:
```bash
git clone https://github.com/frostaura/oxigin-attendance-v2.git
cd oxigin-attendance-v2
```

2. Run the application:
```bash
docker-compose up -d
```

3. Access the application:
- Frontend: http://localhost:3000
- Backend API: http://localhost:5000
- Database: localhost:5432

### Production Environment

1. Copy environment variables:
```bash
cp .env.example .env
```

2. Update the `.env` file with production values:
```bash
# Set secure database password
DB_PASSWORD=your_secure_database_password

# Set secure JWT secret key (minimum 32 characters)
JWT_SECRET_KEY=your-production-jwt-secret-key-minimum-32-characters

# Set your API URL
API_URL=http://your-domain.com/api
```

3. Run with production configuration:
```bash
docker-compose -f docker-compose.prod.yml up -d
```

## Services

### Backend (.NET API)
- **Port**: 5000
- **Health Check**: GET http://localhost:5000/health
- **Technology**: .NET 8.0, Entity Framework Core, PostgreSQL

### Frontend (React)
- **Port**: 3000
- **Technology**: React 19, TypeScript, Tailwind CSS
- **Served by**: Nginx

### Database (PostgreSQL)
- **Port**: 5432
- **Database**: oxigin_attendance
- **User**: oxigin_user

## Environment Variables

### Required Production Variables

```bash
# Database
DB_PASSWORD=secure_password_here

# JWT Authentication
JWT_SECRET_KEY=minimum-32-character-secret-key

# API Configuration
API_URL=http://localhost:5000/api

# Application
APP_NAME="Oxigin Attendance v2"
COMPANY_NAME="Your Company Name"
```

### Frontend Environment Variables

The frontend supports additional configuration via environment variables:

```bash
REACT_APP_API_URL=http://localhost:5000/api
REACT_APP_API_TIMEOUT=30000
REACT_APP_APP_NAME="Oxigin Attendance v2"
REACT_APP_VERSION=2.0.0
REACT_APP_COMPANY_NAME="Your Company Name"
REACT_APP_ENABLE_LOCATION_TRACKING=true
REACT_APP_ENABLE_BREAK_TRACKING=true
REACT_APP_DEFAULT_TIMEZONE=America/New_York
REACT_APP_THEME_PRIMARY_COLOR=#1976d2
REACT_APP_ITEMS_PER_PAGE=10
```

## Development

### Hot Reload Development

For active development with hot reload:

```bash
# Start only the database
docker-compose up postgres -d

# Run backend locally
cd backend/OxiginAttendance.Api
dotnet run

# Run frontend locally (in another terminal)
cd frontend
npm start
```

### Building Individual Services

```bash
# Build backend image
cd backend/OxiginAttendance.Api
docker build -t oxigin-backend .

# Build frontend image
cd frontend
docker build -t oxigin-frontend .
```

## Database Management

### Database Migrations

Run Entity Framework migrations:

```bash
# Generate migration
docker-compose exec backend dotnet ef migrations add InitialCreate

# Apply migrations
docker-compose exec backend dotnet ef database update
```

### Database Backup

```bash
# Backup database
docker-compose exec postgres pg_dump -U oxigin_user oxigin_attendance > backup.sql

# Restore database
docker-compose exec -T postgres psql -U oxigin_user oxigin_attendance < backup.sql
```

## Logs and Monitoring

### View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f backend
docker-compose logs -f frontend
docker-compose logs -f postgres
```

### Health Checks

```bash
# Check service health
docker-compose ps

# Test API health
curl http://localhost:5000/health
```

## Troubleshooting

### Common Issues

1. **Port conflicts**: Change ports in docker-compose.yml if needed
2. **Permission issues**: Ensure Docker has proper permissions
3. **Build failures**: Check .dockerignore and ensure all required files are included

### Reset Everything

```bash
# Stop and remove all containers, networks, and volumes
docker-compose down -v
docker system prune -f

# Rebuild and start fresh
docker-compose up --build -d
```

## Security Notes

- Never commit `.env` files with real credentials
- Use strong passwords for production
- Keep JWT secret keys secure and at least 32 characters long
- Run with non-root users in production
- Use Docker secrets for sensitive data in production environments

## Performance Tips

- Use multi-stage builds to reduce image sizes
- Enable Docker BuildKit for faster builds
- Use .dockerignore to exclude unnecessary files
- Consider using Alpine Linux images for smaller footprint