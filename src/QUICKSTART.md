# Quick Start - GoingMy Microservices with Aspire

## 🚀 Get Started in 3 Steps

### Step 1: Build All Services
```powershell
cd src
dotnet build GoingMy.Services.slnx
```

### Step 2: Run Aspire Orchestrator
```powershell
cd GoingMy.AppHost
dotnet run
```

This automatically:
- ✅ Starts PostgreSQL database
- ✅ Launches Auth API (http://localhost:5001)
- ✅ Launches Post API (http://localhost:5002)  
- ✅ Opens Aspire Dashboard (http://localhost:18888)

### Step 3: Access Your Services

| Service | URL | Purpose |
|---------|-----|---------|
| **Auth API** | http://localhost:5001/swagger | User authentication |
| **Post API** | http://localhost:5002/swagger | Post management |
| **Aspire Dashboard** | http://localhost:18888 | Monitoring & logs |
| **Health Check (Auth)** | http://localhost:5001/health | Service status |
| **Health Check (Post)** | http://localhost:5002/health | Service status |

## 📊 Solution Structure

```
GoingMy.Services.slnx              ← OPEN THIS IN VS/VS Code
├── GoingMy.AppHost                ← Run this to start everything
├── GoingMy.ServiceDefaults        ← Shared Aspire setup
├── GoingMy.AuthService/
│   ├── GoingMy.Auth.Domain
│   ├── GoingMy.Auth.Application
│   ├── GoingMy.Auth.Infrastructure
│   ├── GoingMy.Auth.API            ← Auth microservice
│   ├── GoingMy.Auth.Tests
│   └── GoingMy.Auth.IntegrationTests
└── GoingMy.PostService/
    ├── GoingMy.Post.Domain
    ├── GoingMy.Post.Application
    ├── GoingMy.Post.Infrastructure
    ├── GoingMy.Post.API            ← Post microservice
    ├── GoingMy.Post.Tests
    └── GoingMy.Post.IntegrationTests
```

## 💻 Common Commands

```powershell
# Navigate to services
cd src

# Open solution in Visual Studio
start GoingMy.Services.slnx

# Build everything
dotnet build GoingMy.Services.slnx

# Run specific service in isolation
dotnet run --project GoingMy.AuthService/src/GoingMy.Auth.API/GoingMy.Auth.API.csproj

# Run all tests
dotnet test GoingMy.Services.slnx

# Run only unit tests
dotnet test GoingMy.Services.slnx --filter "FullyQualifiedName~Tests"

# Run only integration tests
dotnet test GoingMy.Services.slnx --filter "FullyQualifiedName~IntegrationTests"

# Clean build artifacts
dotnet clean GoingMy.Services.slnx

# Format code
dotnet format GoingMy.Services.slnx
```

## 🏗️ Project Organization

### Clean Architecture Layers (Each Service)

```
Auth Service:
  GoingMy.Auth.Domain              ← Business logic only
  GoingMy.Auth.Application         ← Application services
  GoingMy.Auth.Infrastructure      ← Database & persistence
  GoingMy.Auth.API                 ← REST endpoints
  GoingMy.Auth.Tests               ← Unit tests
  GoingMy.Auth.IntegrationTests    ← API tests

Post Service: (Same structure)
  GoingMy.Post.Domain
  GoingMy.Post.Application
  GoingMy.Post.Infrastructure
  GoingMy.Post.API
  GoingMy.Post.Tests
  GoingMy.Post.IntegrationTests
```

## 🎯 What's Already Configured

✅ **Service Discovery** - Services find each other automatically  
✅ **Health Checks** - `/health` endpoint for monitoring  
✅ **Distributed Tracing** - OpenTelemetry integration  
✅ **PostgreSQL** - Automatic database provisioning  
✅ **Aspire Dashboard** - Real-time monitoring  
✅ **Documentation** - Auto-generated Swagger/OpenAPI  

## 📚 Documentation

| File | Content |
|------|---------|
| [ASPIRE-GUIDE.md](ASPIRE-GUIDE.md) | Detailed Aspire setup & configuration |
| [README.md](README.md) | Architecture & service overview |
| [.skill-system/](../.skill-system/) | Team knowledge & standards |

## 🐛 Troubleshooting

### Services won't start?
```powershell
# Check if ports are in use
netstat -ano | findstr :5001
netstat -ano | findstr :5002

# Kill process if needed
taskkill /PID <PID> /F
```

### Database issues?
```powershell
# List Docker containers
docker ps -a

# View PostgreSQL logs
docker logs <postgres_container_id>

# Connect to database
docker exec -it <postgres_container_id> psql -U postgres
```

### Aspire Dashboard not loading?
1. Check AppHost is running (should show "Listening on http://localhost:18888")
2. Try http://localhost:18888 directly
3. Check firewall settings allow port 18888

## 🚀 Next Steps

1. **Implement Domain Models**
   - Add entities to `Domain` projects
   - Define business rules

2. **Set Up Database**
   - Create DbContext in Infrastructure
   - Add Entity Framework migrations
   - Seed test data

3. **Build APIs**
   - Add controllers
   - Implement endpoints
   - Add validation

4. **Test Everything**
   - Write unit tests
   - Write integration tests
   - Test service-to-service communication

5. **Deploy**
   - Build Docker images
   - Deploy to Kubernetes or Docker Compose
   - Monitor with Aspire

## 📞 Need Help?

1. **Check Aspire Dashboard** - `http://localhost:18888`
2. **Review ASPIRE-GUIDE.md** - Detailed configuration
3. **Check .skill-system/README.md** - Team standards
4. **Review service Program.cs** - See configuration pattern

## ⚡ Pro Tips

- **Development Mode**: Services auto-reload on code changes (with dotnet watch)
- **Health Endpoint**: All services expose `/health` for monitoring
- **Service URLs**: Use service names (e.g., `http://auth-api`) for inter-service calls
- **Logs**: View all service logs in Aspire Dashboard > Console tab
- **Metrics**: Monitor performance in Aspire Dashboard > Metrics tab

---

**Version**: 1.0  
**Framework**: .NET Aspire 13.1+  
**Last Updated**: March 19, 2026
