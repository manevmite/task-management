# Clean Architecture Documentation

This project follows Clean Architecture principles with proper separation of concerns and dependency injection.

## Project Structure

The backend is organized into four main projects following Clean Architecture:

```
backend/
├── TaskManagement.Domain/           # Core Domain Layer
├── TaskManagement.Application/      # Application/Business Logic Layer  
├── TaskManagement.Infrastructure/   # Infrastructure/Data Access Layer
└── TaskManagement.API/              # Presentation/API Layer
```

## Layer Descriptions

### 1. Domain Layer (`TaskManagement.Domain`)
**Purpose**: Contains the core business entities and interfaces (no dependencies on other layers)

**Contents**:
- `Entities/` - Domain entities (TaskItem, User)
- `Interfaces/` - Repository interfaces (IRepository, ITaskRepository, IUserRepository)

**Dependencies**: None (pure domain logic)

### 2. Application Layer (`TaskManagement.Application`)
**Purpose**: Contains business logic, DTOs, and application services

**Contents**:
- `DTOs/` - Data Transfer Objects (Request/Response models)
  - `Auth/` - Authentication DTOs
  - `Tasks/` - Task DTOs
- `Interfaces/` - Service interfaces (IAuthService, ITaskService)
- `Services/` - Business logic services (AuthService, TaskService)
- `DependencyInjection.cs` - DI configuration for Application layer

**Dependencies**: 
- Domain Layer only
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Logging

### 3. Infrastructure Layer (`TaskManagement.Infrastructure`)
**Purpose**: Implements data access, external services, and infrastructure concerns

**Contents**:
- `Data/` - DbContext (ApplicationDbContext)
- `Repositories/` - Repository implementations
  - `Repository.cs` - Generic repository base class
  - `TaskRepository.cs` - Task-specific repository
  - `UserRepository.cs` - User-specific repository
- `DependencyInjection.cs` - DI configuration for Infrastructure layer

**Dependencies**:
- Domain Layer
- Application Layer (implicit through Domain interfaces)
- Entity Framework Core
- SQLite

### 4. API Layer (`TaskManagement.API`)
**Purpose**: Contains controllers, middleware, and API configuration

**Contents**:
- `Controllers/` - API endpoints (AuthController, TasksController)
- `Program.cs` - Application startup and configuration
- `appsettings.json` - Configuration files
- `Properties/` - Launch settings

**Dependencies**:
- Application Layer (for services and DTOs)
- Infrastructure Layer (registered through DI)
- ASP.NET Core packages
- JWT Authentication
- Swagger/OpenAPI

## Dependency Flow

```
API Layer
    ↓ (depends on)
Application Layer
    ↓ (depends on)
Domain Layer
    ↑ (implements)
Infrastructure Layer
```

**Key Principle**: Dependencies always point inward. The Domain layer has no dependencies, and the outer layers depend on inner layers.

## Dependency Injection

### Application Layer DI (`TaskManagement.Application/DependencyInjection.cs`)
```csharp
services.AddApplication(); // Registers IAuthService, ITaskService
```

### Infrastructure Layer DI (`TaskManagement.Infrastructure/DependencyInjection.cs`)
```csharp
services.AddInfrastructure(configuration); // Registers DbContext and Repositories
```

### API Layer Configuration (`TaskManagement.API/Program.cs`)
```csharp
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
```

## Benefits of This Architecture

1. **Separation of Concerns**: Each layer has a single, well-defined responsibility
2. **Testability**: Business logic (Application layer) can be tested independently of infrastructure
3. **Maintainability**: Changes in one layer don't cascade to others
4. **Reusability**: Domain and Application layers can be reused with different presentation layers
5. **Dependency Inversion**: High-level modules don't depend on low-level modules; both depend on abstractions
6. **SOLID Principles**: Follows all SOLID principles, especially:
   - Single Responsibility Principle
   - Dependency Inversion Principle
   - Open/Closed Principle

## Best Practices Followed

✅ **Dependency Rule**: Dependencies point inward only  
✅ **Interface Segregation**: Interfaces are specific and focused  
✅ **Repository Pattern**: Data access is abstracted through interfaces  
✅ **Service Layer**: Business logic is separated from data access  
✅ **DTO Pattern**: Data transfer objects prevent exposing domain entities  
✅ **Dependency Injection**: All dependencies are injected, not instantiated  
✅ **Extension Methods**: DI configuration uses extension methods for clean code  

## Adding New Features

When adding new features:

1. **Domain Layer**: Add entities and interfaces
2. **Application Layer**: Add DTOs, service interfaces, and service implementations
3. **Infrastructure Layer**: Implement repository interfaces if needed
4. **API Layer**: Add controllers and configure endpoints

## Example: Adding a New Feature

To add a "Projects" feature:

1. **Domain**: Create `Project.cs` entity and `IProjectRepository` interface
2. **Application**: Create Project DTOs, `IProjectService` interface, and `ProjectService` implementation
3. **Infrastructure**: Implement `ProjectRepository` inheriting from `Repository<Project>`
4. **API**: Create `ProjectsController` using `IProjectService`
5. **DI**: Register `IProjectRepository` and `IProjectService` in respective DI files

This structure ensures clean separation and maintainable code.

