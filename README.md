# Task Management System

A full-stack Task Management System built with .NET 9 Web API backend and React frontend. This application allows users to create, view, update, and delete tasks with features like task filtering, completion status, and user authentication.

## Features

### Backend (.NET 9 Web API)
- ✅ RESTful API with CRUD operations for tasks
- ✅ SQLite database for data persistence
- ✅ JWT-based authentication
- ✅ Data validation with Data Annotations
- ✅ Swagger/OpenAPI documentation
- ✅ Repository pattern and service layer (SOLID principles)
- ✅ Clean architecture and separation of concerns

### Frontend (React with TypeScript)
- ✅ Responsive UI built with React functional components and hooks
- ✅ TypeScript for type safety
- ✅ React Context API for state management
- ✅ Integration with backend APIs using Axios
- ✅ Tailwind CSS for modern, responsive styling
- ✅ Task filtering (All, Active, Completed)
- ✅ Create, Read, Update, Delete operations
- ✅ Task completion toggle
- ✅ User authentication (Login/Register)

### Additional Features
- ✅ Docker containerization support
- ✅ CORS configuration for frontend-backend communication
- ✅ Error handling and validation
- ✅ Responsive design for mobile and desktop

## Technology Stack

### Backend
- .NET 9.0
- Entity Framework Core 9.0
- SQLite Database
- JWT Authentication
- Swagger/OpenAPI

### Frontend
- React 18.2 with TypeScript
- Axios for HTTP requests
- React Context API for state management
- Tailwind CSS for styling

### Deployment
- Docker & Docker Compose
- Nginx (for frontend production build)

## Project Structure

```
task-management/
├── backend/
│   ├── TaskManagement.Domain/      # Domain Layer (Entities, Interfaces)
│   │   ├── Entities/              # Domain entities
│   │   └── Interfaces/            # Repository interfaces
│   ├── TaskManagement.Application/ # Application Layer (Business Logic)
│   │   ├── DTOs/                  # Data Transfer Objects
│   │   ├── Interfaces/            # Service interfaces
│   │   ├── Services/              # Business logic services
│   │   └── DependencyInjection.cs # DI configuration
│   ├── TaskManagement.Infrastructure/ # Infrastructure Layer (Data Access)
│   │   ├── Data/                  # DbContext
│   │   ├── Repositories/          # Repository implementations
│   │   └── DependencyInjection.cs # DI configuration
│   └── TaskManagement.API/        # Presentation Layer (API)
│       ├── Controllers/           # API controllers
│       ├── Program.cs             # Application entry point
│       └── appsettings.json       # Configuration
├── frontend/
│   ├── public/                   # Static files
│   └── src/
│       ├── components/           # React components (TypeScript)
│       ├── context/              # Context providers (TypeScript)
│       ├── services/             # API service layer (TypeScript)
│       ├── types/                # TypeScript type definitions
│       ├── App.tsx               # Main App component
│       ├── index.tsx             # Entry point
│       └── index.css             # Tailwind CSS styles
├── docker-compose.yml            # Docker orchestration
└── README.md                     # This file
```

## Prerequisites

Before running the application, ensure you have the following installed:

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js](https://nodejs.org/) (v18 or higher)
- [Docker](https://www.docker.com/get-started) (optional, for containerized deployment)
- [Git](https://git-scm.com/)

## Setup and Running

### Option 1: Run Locally (Development)

#### Backend Setup

1. Navigate to the backend directory:
   ```bash
   cd backend/TaskManagement.API
   ```

2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

   The API will be available at:
   - HTTP: `http://localhost:5000`
   - HTTPS: `https://localhost:5001`
   - Swagger UI: `https://localhost:5001/swagger`

#### Frontend Setup

1. Navigate to the frontend directory:
   ```bash
   cd frontend
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Start the development server:
   ```bash
   npm start
   ```

   The frontend will be available at `http://localhost:3000`

### Option 2: Run with Docker (Production-like)

1. Build and run all services using Docker Compose:
   ```bash
   docker-compose up --build
   ```

   This will:
   - Build the backend .NET API container
   - Build the frontend React app (production build with Nginx)
   - Start both services

2. Access the application:
   - Frontend: `http://localhost:3000`
   - Backend API: `http://localhost:5000`
   - Swagger: `http://localhost:5000/swagger` (if configured for HTTP)

3. To stop all services:
   ```bash
   docker-compose down
   ```

### Environment Variables

#### Backend
The backend uses `appsettings.json` for configuration. Key settings:
- `ConnectionStrings:DefaultConnection`: SQLite database connection string
- `JwtSettings:SecretKey`: JWT token secret key
- `JwtSettings:Issuer`: JWT issuer
- `JwtSettings:Audience`: JWT audience
- `JwtSettings:ExpirationInMinutes`: Token expiration time

#### Frontend
Create a `.env` file in the frontend directory (optional):
```env
REACT_APP_API_URL=http://localhost:5000/api
```

If not set, the default is `http://localhost:5000/api`.

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register a new user
  - Body: `{ "email": "string", "password": "string" }`
- `POST /api/auth/login` - Login user
  - Body: `{ "email": "string", "password": "string" }`

### Tasks (Requires Authentication)
- `GET /api/tasks` - Get all tasks (optional query: `?isCompleted=true/false`)
- `GET /api/tasks/{id}` - Get task by ID
- `POST /api/tasks` - Create a new task
  - Body: `{ "title": "string", "description": "string" }`
- `PUT /api/tasks/{id}` - Update a task
  - Body: `{ "title": "string", "description": "string", "isCompleted": boolean }`
- `DELETE /api/tasks/{id}` - Delete a task

All task endpoints require JWT authentication. Include the token in the Authorization header:
```
Authorization: Bearer {your_token}
```

## Default User Credentials

A default admin user is created on first run:
- **Email**: `admin@example.com`
- **Password**: `Admin123!`

You can use these credentials to log in, or register a new user.

## Development Notes

### Backend Architecture
- **Models**: Entity classes representing database tables
- **DTOs**: Data Transfer Objects for API requests/responses
- **Repositories**: Data access layer implementing repository pattern
- **Services**: Business logic layer
- **Controllers**: API endpoints handling HTTP requests

### Frontend Architecture
- **Components**: Reusable UI components
- **Context**: Global state management using React Context API
- **Services**: API integration layer using Axios

### Database
The SQLite database file (`taskmanagement.db`) is automatically created on first run. It's located in the `backend/TaskManagement.API` directory.

## Testing the API with Swagger

1. Start the backend server
2. Navigate to `https://localhost:5001/swagger` (or `http://localhost:5000/swagger`)
3. Register or login to get a JWT token
4. Click "Authorize" button in Swagger UI
5. Enter: `Bearer {your_jwt_token}`
6. Now you can test all protected endpoints directly from Swagger

## Troubleshooting

### Backend Issues

**Port already in use:**
- Change the port in `launchSettings.json` or kill the process using the port

**Database errors:**
- Delete the `taskmanagement.db` file and restart the application

**CORS errors:**
- Ensure CORS is configured in `Program.cs` for the frontend URL

### Frontend Issues

**API connection errors:**
- Verify the backend is running
- Check `REACT_APP_API_URL` environment variable
- Verify CORS settings in backend

**Build errors:**
- Delete `node_modules` and `package-lock.json`, then run `npm install` again

### Docker Issues

**Port conflicts:**
- Modify ports in `docker-compose.yml` if 3000 or 5000 are already in use

**Build failures:**
- Ensure Docker has sufficient resources allocated
- Check Docker logs: `docker-compose logs`

## Code Quality

- Follows SOLID principles
- Clean code architecture with separation of concerns
- Comprehensive error handling
- Input validation on both frontend and backend
- Meaningful comments and documentation
- RESTful API design
