# PandesalExpress

A comprehensive bakery management system built with .NET 9 and PostgreSQL, designed to streamline operations across multiple store locations, commissary production, and delivery
services.

## 🏗️ Architecture

**Modular Monolith** - Each business domain is organized as a separate project with clear boundaries and responsibilities.

### Core Modules

- **Auth** - Authentication and authorization
- **Stores** - Store management and operations
- **Commissary** - Production planning and management
- **Cashier** - Point of sale operations
- **Products** - Product catalog and inventory
- **Management** - Administrative functions
- **PDND** - Pick-up and delivery services

### Infrastructure

- **Host** - Main web application entry point
- **Infrastructure** - Shared data access, Entity Framework context, and services
- **Shared** - Common DTOs, events, and utilities

## 🛠️ Technology Stack

- **.NET 9.0** - Primary framework
- **ASP.NET Core** - Web API
- **Entity Framework Core** - ORM with PostgreSQL provider
- **PostgreSQL** - Primary database
- **Redis** - Caching and session storage
- **SignalR** - Real-time notifications
- **ASP.NET Core Identity** - Authentication and user management
- **JWT Bearer** - API authentication
- **Swagger/OpenAPI** - API documentation

## 🚀 Getting Started

### Prerequisites

- .NET 9.0 SDK
- PostgreSQL 12+
- Redis (for caching)

### Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd PandesalExpress
   ```

2. **Configure database connection**
   Initialize user secrets for secure local development:
   ```bash
   dotnet user-secrets init --project PandesalExpress.Host
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=pandesal_express;Username=your_user;Password=your_password" --project PandesalExpress.Host
   ```
   
   **Why user secrets?** This approach keeps sensitive connection strings out of source control and appsettings.json, following security best practices for local development.

3. **Run database migrations**
   ```bash
   dotnet ef database update --project PandesalExpress.Infrastructure --startup-project PandesalExpress.Host
   ```

4. **Start the application**
   ```bash
   dotnet run --project PandesalExpress.Host
   ```

5. **Access the application**
    - API: `http://localhost:5010`
    - Swagger UI: `http://localhost:5010/swagger`

## 📁 Project Structure

```
PandesalExpress/
├── PandesalExpress.Host/              # Main web application
├── PandesalExpress.Infrastructure/    # Data access and shared services
├── Shared/                           # Common DTOs, events, utilities
├── PandesalExpress.Auth/             # Authentication module
├── PandesalExpress.Stores/           # Store management
├── PandesalExpress.Commissary/       # Production management
├── PandesalExpress.Cashier/          # Point of sale
├── PandesalExpress.Products/         # Product catalog
├── PandesalExpress.Management/       # Administrative functions
└── PandesalExpress.PDND/            # Pick-up and delivery
```

### Module Structure Pattern

Each business module follows this structure:

```
PandesalExpress.{Module}/
├── Controllers/          # API controllers
├── Features/            # CQRS commands/queries with handlers
├── Dtos/               # Data transfer objects
├── Services/           # Business logic services
├── Tests/              # Unit tests
├── {Module}ModuleServiceExtension.cs
└── AssemblyReference.cs
```

## 🏛️ Key Design Patterns

### Custom CQRS and Mediator Implementation

- **No MediatR dependency** - Uses custom lightweight CQRS and Mediator abstractions
- **Command/Query Handlers** - Located in `Features/` directory
- **Interfaces** - `ICommandHandler<TCommand, TResponse>` and `IQueryHandler<TQuery, TResponse>`

### Module Communication

- **Event-driven** - Modules communicate via domain events using `IEventBus`
- **Shared abstractions** - Common interfaces in `Infrastructure/Abstractions`
- **No direct module references** - Maintains loose coupling

### Data Models

- **Base Model** - All entities inherit from `Model` class with `Ulid` primary keys
- **Audit fields** - Automatic `CreatedAt`, `UpdatedAt`, and `RowVersion` tracking
- **PostgreSQL optimized** - Uses PostgreSQL-specific features and data types

## 🔧 Development Guidelines

### Code Organization

- **DRY Principle** - Check `Shared` and `Infrastructure` before creating new implementations
- **SOLID Principles** - Follow single responsibility and dependency inversion
- **Consistent naming** - Use established patterns: `{Action}{Entity}Handler.cs`

### Performance

- **Aggressive caching** - Implement Redis caching for frequently accessed data
- **Async operations** - All database operations must be asynchronous
- **Query optimization** - Use projections and proper indexing

### Module Registration

Each module registers services via extension methods:

```csharp
public static IServiceCollection Add{Module}Module(this IServiceCollection services)
```

## 🧪 Testing

Run tests for all modules:

```bash
dotnet test
```

Run tests for specific module:

```bash
dotnet test PandesalExpress.{Module}.Tests
```

## 📝 API Documentation

- **Swagger UI** - Available at `/swagger` when running in development
- **HTTP files** - Test requests in `PandesalExpress.Host.http`

## 🤝 Contributing

1. Follow the established module structure and naming conventions
2. Implement proper error handling and logging
3. Add unit tests for new features
4. Update documentation for significant changes if needed
5. Use the custom CQRS pattern for all API endpoints
6. Ensure database migrations are included for schema changes
