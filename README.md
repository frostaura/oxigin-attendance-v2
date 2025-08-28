# Oxigin Attendance v2

A full-stack time and attendance management system built with modern technologies.

## Tech Stack

- **Backend**: .NET 8.0 Web API with Entity Framework Core
- **Database**: PostgreSQL
- **Frontend**: React TypeScript with Redux Toolkit
- **UI**: Material-UI (MUI)
- **Authentication**: JWT Bearer tokens
- **Containerization**: Docker & Docker Compose

## Features

### Completed Features
- ✅ User authentication (Login/Register)
- ✅ JWT-based authorization with roles (Employee, Manager, Administrator)
- ✅ Clock in/out functionality with real-time tracking
- ✅ Automatic overtime calculation (8+ hours per day)
- ✅ Break time tracking
- ✅ Time entry management with status tracking
- ✅ Responsive Material-UI design
- ✅ Role-based navigation and access control
- ✅ PostgreSQL database integration
- ✅ Comprehensive API documentation with Swagger

### Backend API Endpoints

#### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `GET /api/auth/me` - Get current user info
- `GET /api/auth/validate` - Validate JWT token

#### Time Entries
- `POST /api/timeentry/clock-in` - Clock in
- `POST /api/timeentry/clock-out` - Clock out
- `GET /api/timeentry/active` - Get active time entry
- `GET /api/timeentry` - Get time entries with filtering
- `GET /api/timeentry/report` - Get time report for user
- `GET /api/timeentry/report/all` - Get all employees' time reports (Manager/Admin only)

### Models & Database Schema

#### ApplicationUser (extends IdentityUser)
- User authentication and profile information
- Employee details (ID, department, job title)
- Relationships to time entries and leave requests

#### TimeEntry
- Clock in/out times with automatic calculation
- Break time and overtime tracking
- Status management (Active, Completed, Cancelled)
- Location and notes support

#### LeaveRequest (Model created, API endpoints pending)
- Leave type management
- Approval workflow
- Date range tracking

## Getting Started

### Option 1: Using Docker (Recommended)

**Prerequisites:**
- Docker and Docker Compose

**Quick Start:**
```bash
git clone <repository-url>
cd oxigin-attendance-v2

# Set up environment files (required for security)
cp .env.docker.example .env.docker
cp .env.docker.dev.example .env.docker.dev

# Edit .env.docker and .env.docker.dev with your secure credentials
# Replace placeholder values with actual secure JWT keys and passwords

# Start the application
docker compose up --build

# For development with live reload
docker compose -f docker-compose.dev.yml up --build
```

**Access the application:**
- Frontend: http://localhost:3000
- Backend API: http://localhost:5000
- API Documentation: http://localhost:5000/swagger

For detailed Docker setup instructions, see [DOCKER.md](DOCKER.md).

### Option 2: Local Development Setup

### Prerequisites
- .NET 8.0 SDK
- Node.js 18+ and npm
- PostgreSQL database

### Backend Setup

1. Navigate to the backend directory:
   ```bash
   cd backend/OxiginAttendance.Api
   ```

2. Install dependencies:
   ```bash
   dotnet restore
   ```

3. Update the connection string in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=oxigin_attendance;Username=postgres;Password=your_password"
     }
   }
   ```

4. Create and run database migrations:
   ```bash
   dotnet ef database update
   ```

5. Run the backend:
   ```bash
   dotnet run
   ```

The API will be available at `https://localhost:7017` with Swagger documentation at `https://localhost:7017/swagger`.

### Frontend Setup

1. Navigate to the frontend directory:
   ```bash
   cd frontend
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Update the API URL in `.env` if needed:
   ```
   REACT_APP_API_URL=https://localhost:7017/api
   ```

4. Start the development server:
   ```bash
   npm start
   ```

The React app will be available at `http://localhost:3000`.

### Default Admin Account

The system creates a default administrator account on first run:
- **Email**: admin@oxigin.com
- **Password**: Admin@123

## Project Structure

### Backend Structure
```
backend/OxiginAttendance.Api/
├── Controllers/          # API controllers
├── Data/                # Entity Framework DbContext
├── DTOs/                # Data Transfer Objects
├── Models/              # Domain models
├── Services/            # Business logic services
└── Program.cs           # Application configuration
```

### Frontend Structure
```
frontend/src/
├── components/          # React components
│   ├── auth/           # Authentication components
│   ├── dashboard/      # Dashboard components
│   ├── layout/         # Layout components
│   └── common/         # Shared components
├── store/              # Redux store and slices
├── services/           # API client services
├── types/              # TypeScript type definitions
└── utils/              # Utility functions
```

## Development Notes

### Authentication Flow
1. User registers or logs in via the frontend
2. Backend validates credentials and returns JWT token
3. Frontend stores token and includes it in API requests
4. Backend validates token for protected endpoints

### Time Tracking Logic
1. Clock In: Creates active TimeEntry record
2. Clock Out: Updates TimeEntry with end time and calculations
3. Automatic calculation of total hours, break time, and overtime
4. Standard work day is 8 hours (configurable in TimeEntryService)

### Role-Based Access
- **Employee**: Basic time tracking and personal reports
- **Manager**: Employee management and team reports
- **Administrator**: Full system access and user management

## Future Enhancements

### Planned Features
- [ ] Leave request management system
- [ ] Advanced reporting with charts and analytics
- [ ] Employee management interface
- [ ] Bulk time entry operations
- [ ] Export functionality (PDF, Excel)
- [ ] Email notifications
- [ ] Mobile-responsive improvements
- [ ] Real-time notifications
- [ ] Audit trail and logging
- [ ] Advanced filtering and search

### Technical Improvements
- [ ] Unit and integration tests
- [ ] Docker containerization
- [ ] CI/CD pipeline setup
- [ ] Database migrations management
- [ ] Error handling and logging improvements
- [ ] Performance optimization
- [ ] Security enhancements

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Commit your changes
6. Create a pull request

## License

This project is licensed under the MIT License.
