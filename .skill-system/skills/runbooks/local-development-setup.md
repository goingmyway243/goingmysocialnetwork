# SKILL: runbook-local-development-setup

## Purpose
Provide step-by-step operational procedures for developers to set up complete GoingMy development environment locally using .NET Aspire orchestration.

## Use When
- New team member joins
- Setting up development machine
- Troubleshooting environment setup issues
- Documenting .NET Aspire-based local development

## Prerequisites
- Git installed
- .NET SDK 10.0+ (with .NET Aspire workload)
- Node.js 22+
- Docker Desktop (required for Aspire orchestration)
- PostgreSQL client tools (pgAdmin, psql, or DBeaver) - optional
- VS Code or Visual Studio 2022

## Step-by-Step Setup

### Phase 1: Repository Setup

**Task 1.1: Clone Repository**
```bash
# Clone main repository
git clone https://github.com/yourusername/goingmysocialnetwork.git
cd goingmysocialnetwork

# Verify structure
ls -la
# Should show: /services, /web, /docs, docker-compose.yml, etc.
```

**Task 1.2: Install Dependencies**
```bash
# Backend - per service
cd services/GoingMy.UserService
dotnet restore
cd ../..

# Frontend
cd web
npm install
cd ..
```

**Task 1.3: Configure Environment Variables**
```bash
# Create .env files in root
cp .env.example .env

# Edit .env with your values
# POSTGRES_PASSWORD=your_secure_password
# JWT_SECRET=your_jwt_secret
# ASPNETCORE_ENVIRONMENT=Development
```

### Phase 2: Database Setup

**Task 2.1: Start PostgreSQL Container**
```bash
# Start only database
docker-compose up postgres -d

# Verify connection
docker-compose exec postgres psql -U postgres -d goingmy -c "\dt"
```

**Task 2.2: Run Migrations**
```bash
# For User Service
cd services/GoingMy.UserService
dotnet ef database update -s src/GoingMy.User.API
cd ../..

# For Post Service
cd services/GoingMy.PostService
dotnet ef database update -s src/GoingMy.Post.API
cd ../..
```

**Task 2.3: Verify Database**
```bash
# Connect to database
docker-compose exec postgres psql -U postgres -d goingmy

# Check tables
\dt

# Sample query
SELECT * FROM users;

# Exit
\q
```

### Phase 3: Backend Services via Aspire

**Task 3.1: Services Start Automatically**
```bash
# When you run 'dotnet run' from AppHost directory,
# all configured services start automatically:
# - PostgreSQL database
# - Redis cache
# - User Service (typically http://localhost:5001)
# - Post Service (typically http://localhost:5002)
# - Other configured services

# Aspire manages container orchestration for you!
```

**Task 3.2: Monitor Services via Aspire Dashboard**
```bash
# Open browser to Aspire Dashboard
# http://localhost:18888

# You can see:
# - Service health status
# - Resource consumption (CPU, Memory)
# - Log streams from each service
# - Trace details (if instrumented)
```

**Task 3.3: Verify Service Endpoints**
```bash
# User Service health check
curl http://localhost:5001/api/v1/health

# Post Service health check
curl http://localhost:5002/api/v1/health

# All endpoints visible in Aspire Dashboard under Resources
```

### Phase 4: Frontend Setup

**Task 4.1: Install Angular Dependencies**
```bash
cd web

# Install
npm install

# Verify Angular CLI
ng --version

# Return to root
cd ..
```

**Task 4.2: Configure API Endpoints**
```bash
# Edit environment.ts
cd web
nano src/environments/environment.ts

# Set API_URL (Aspire handles service discovery)
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000'  // Or gateway URL from Aspire
};

cd ..
```

**Task 4.3: Start Angular Development Server (Separate Terminal)**
```bash
# From web directory (while Aspire is running in another terminal)
cd web
ng serve

# Or with custom port
ng serve --port 4200

# Verify: Open browser
# http://localhost:4200
```

### Phase 5: Integration Testing

**Task 5.1: Verify All Services Running via Dashboard**
```bash
# Open Aspire Dashboard
# http://localhost:18888

# Expected to see:
# postgres        - Running (healthy)
# redis           - Running (healthy)
# UserService     - Running (healthy)
# PostService     - Running (healthy)
# Frontend        - Running (if configured)
```

**Task 5.2: Test API Endpoints**
```bash
# Health checks (from Aspire resource list)
curl http://localhost:5001/api/v1/health    # User Service
curl http://localhost:5002/api/v1/health    # Post Service

# Create test user
curl -X POST http://localhost:5001/api/v1/users \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "name": "Test User"
  }'
```

**Task 5.3: Test Frontend Integration**
```bash
# Navigate to http://localhost:4200
# Should see login screen
# Try creating account or logging in
# Check Aspire Dashboard logs for backend activity
```

## Troubleshooting

### Aspire-Related Issues

**Problem: Aspire Dashboard Not Starting**
```bash
# Verify .NET Aspire is installed
dotnet workload list | grep aspire

# Reinstall if needed
dotnet workload install aspire

# Check port 18888 is available
# Default Aspire Dashboard runs on http://localhost:18888
```

**Problem: Service Not Visible in Dashboard**
```bash
# Verify service is configured in AppHost Program.cs
# Example:
var userService = builder.AddProject<Projects.GoingMy_User_API>("user-service")
    .WithReference(postgres)
    .WithHttpEndpoint(port: 5001);

# Restart Aspire host
# Ctrl+C in AppHost terminal
# Run: dotnet run again
```

**Problem: Container Port Conflicts**
```bash
# Aspire auto-manages ports, but if conflicts occur:
# Check Docker status
docker ps

# Stop conflicting containers
docker stop {container_id}

# Restart Aspire
cd AppHost && dotnet run
```

### Database Issues

**Problem: PostgreSQL Not Starting via Aspire**
```bash
# Check AppHost configuration includes PostgreSQL
# In AppHost Program.cs, ensure:
var postgres = builder.AddPostgres("postgres")
    .WithVolume("postgres-data", "/var/lib/postgresql/data");

# Check Aspire logs for database errors
# View in Dashboard under postgres resource logs
```

**Problem: Migrations Failed**
```bash
# Check migration history
cd services/GoingMy.UserService
dotnet ef migrations list

# Rollback last migration
dotnet ef database update {previous-migration-name}

# Remove failed migrations
dotnet ef migrations remove
```

### Backend Issues

**Problem: Service Health Check Failing**
```bash
# View service logs in Aspire Dashboard
# Click service resource → View logs

# Or check via API
curl http://localhost:5001/api/v1/health

# Check dependencies in AppHost configuration
```

**Problem: Service-to-Service Communication Issues**
```bash
# Aspire provides automatic service discovery
# Ensure service references configured in AppHost:
var userService = builder.AddProject<Projects.GoingMy_User_API>("user-service")
    .WithReference(postgres);  // Add all dependencies
```

**Problem: Dependency Errors**
```bash
# Clean and rebuild
cd services/GoingMy.UserService
dotnet clean
dotnet build
```

### Frontend Issues

**Problem: Port 4200 Already in Use**
```bash
# Use different port
ng serve --port 4201
```

**Problem: Module Not Found**
```bash
# Reinstall dependencies
rm -rf node_modules package-lock.json
npm install
```

**Problem: API Connection Failures**
```bash
# Verify services running in Aspire Dashboard
# Ensure API_URL in environment.ts matches service port
# Check CORS configuration in API services
```

## Quick Start Script

```bash
#!/bin/bash
# quick-setup.sh - One-command Aspire-based setup

set -e

echo "🚀 Starting GoingMy Setup with .NET Aspire..."

# Environment
echo "📋 Setting up environment..."
cp .env.example .env

# .NET Aspire
echo "⚙️  Installing .NET Aspire workload..."
dotnet workload install aspire

# Restore dependencies
echo "📦 Restoring NuGet packages..."
dotnet restore AppHost
cd services/GoingMy.UserService
dotnet restore
cd ../GoingMy.PostService
dotnet restore
cd ../..

# Frontend
echo "📦 Installing frontend dependencies..."
cd web
npm install
cd ..

echo ""
echo "✅ Setup complete!"
echo ""
echo "Starting services via Aspire..."
echo ""
echo "To start development:"
echo "  Terminal 1: cd AppHost && dotnet run"
echo "  (This starts all services: PostgreSQL, Redis, APIs)"
echo ""
echo "  Terminal 2: cd web && ng serve"
echo ""
echo "Then open:"
echo "  Frontend:  http://localhost:4200"
echo "  Dashboard: http://localhost:18888"
```

## Daily Development Commands

```bash
# Start all services via Aspire (Terminal 1)
cd AppHost
dotnet run

# In another terminal, start frontend (Terminal 2)
cd web
ng serve

# Access services
Frontend:       http://localhost:4200
Dashboard:      http://localhost:18888
User Service:   http://localhost:5001
Post Service:   http://localhost:5002

# View logs from Aspire Dashboard
# Click on each service resource → View logs

# Run tests
dotnet test

# Stop everything (Ctrl+C in AppHost terminal)
# Aspire automatically cleans up containers
```

## Quality Criteria
- Setup completes in < 15 minutes
- All services healthy after setup
- API endpoints responding
- Frontend loads successfully
- Database connected and migrated

## Verification Checklist
- [ ] Git repository cloned
- [ ] All dependencies installed
- [ ] Environment variables configured
- [ ] Docker containers started
- [ ] Database migrations completed
- [ ] Backend services running
- [ ] Frontend serves on localhost:4200
- [ ] API endpoints responding
- [ ] No console errors

## References
- Setup Troubleshooting Guide
- Team Wiki (if available)
- Architecture Documentation

## Additional Resources

### AppHost Project Structure
```
AppHost/
├── Program.cs           # Service orchestration configuration
├── AppHost.csproj       # Aspire host project
└── Properties/
    └── launchSettings.json
```

### Example AppHost Program.cs
```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Add infrastructure
var postgres = builder.AddPostgres("postgres")
    .WithVolume("postgres-data", "/var/lib/postgresql/data")
    .AddDatabase("goingmy-db");

var redis = builder.AddRedis("redis");

// Add services
builder.AddProject<Projects.GoingMy_User_API>("user-service")
    .WithReference(postgres)
    .WithReference(redis)
    .WithHttpEndpoint(port: 5001);

builder.AddProject<Projects.GoingMy_Post_API>("post-service")
    .WithReference(postgres)
    .WithReference(redis)
    .WithHttpEndpoint(port: 5002);

// Add frontend
builder.AddNpmApp("web", projectDirectory: "../web")
    .WithHttpEndpoint(port: 4200)
    .WithEnvironment("API_URL", "http://localhost:5000");

await builder.Build().RunAsync();
```

## Changelog
- v2.0: Migrated to .NET Aspire orchestration
- v1.1: Added quick-start script
- v1.0: Initial development setup runbook
