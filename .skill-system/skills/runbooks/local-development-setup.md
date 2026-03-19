# SKILL: runbook-local-development-setup

## Purpose
Provide step-by-step operational procedures for developers to set up complete GoingMy development environment locally, from cloning repository to running services.

## Use When
- New team member joins
- Setting up development machine
- Troubleshooting environment setup issues
- Documenting setup process

## Prerequisites
- Git installed
- .NET SDK 10.0+
- Node.js 22+
- Docker and Docker Compose
- PostgreSQL client tools (pgAdmin, psql, or DBeaver)
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

### Phase 3: Backend Services

**Task 3.1: Start Redis Cache**
```bash
docker-compose up redis -d

# Verify
docker-compose exec redis redis-cli ping
# Should output: PONG
```

**Task 3.2: Run User Service**
```bash
cd services/GoingMy.UserService/src/GoingMy.User.API

# Run with hot reload
dotnet watch run

# Or without watch
dotnet run

# In another terminal, verify
curl http://localhost:5001/api/v1/health
```

**Task 3.3: Run Post Service**
```bash
cd services/GoingMy.PostService/src/GoingMy.Post.API

dotnet watch run

# Verify
curl http://localhost:5002/api/v1/health
```

### Phase 4: Frontend Setup

**Task 4.1: Install Angular Dependencies**
```bash
cd web

# Install
npm install

# Verify Angular CLI
ng --version
```

**Task 4.2: Configure API Endpoints**
```bash
# Edit environment.ts
nano src/environments/environment.ts

# Set API_URL
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000'
};
```

**Task 4.3: Start Angular Development Server**
```bash
# From web directory
ng serve

# Or with custom port
ng serve --port 4200

# Verify: Open browser
# http://localhost:4200
```

### Phase 5: Integration Testing

**Task 5.1: Verify All Services Running**
```bash
# Check all containers
docker-compose ps

# Expected output:
# postgres    - Up (healthy)
# redis       - Up (healthy)
# API services - Up (healthy)
```

**Task 5.2: Test API Endpoints**
```bash
# Health checks
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

**Task 5.3: Test Frontend**
```bash
# Navigate to http://localhost:4200
# Should see login screen
# Try creating account or logging in
```

## Troubleshooting

### Database Issues

**Problem: PostgreSQL Connection Refused**
```bash
# Solution 1: Check container is running
docker-compose ps postgres

# Solution 2: Check logs
docker-compose logs postgres

# Solution 3: Restart
docker-compose restart postgres

# Solution 4: Reset (WARNING: deletes data)
docker-compose down -v
docker-compose up postgres -d
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

**Problem: Service Won't Start (Port in Use)**
```bash
# Find process using port
lsof -i :5001  # Linux/Mac
netstat -ano | findstr :5001  # Windows

# Kill process
kill -9 {PID}  # Linux/Mac
taskkill /PID {PID} /F  # Windows
```

**Problem: Dependency Errors**
```bash
# Clean and rebuild
cd services/GoingMy.UserService
dotnet clean
dotnet build
```

**Problem: Runtime Errors**
```bash
# Check logs
dotnet run 2>&1 | tee app.log

# Debug with detailed messages
ASPNETCORE_ENVIRONMENT=Development dotnet run --verbose
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

**Problem: Compilation Errors**
```bash
# Clear cache and rebuild
ng serve --poll 2000  # Enable polling
```

## Quick Start Script

```bash
#!/bin/bash
# quick-setup.sh - One-command setup

set -e

echo "🚀 Starting GoingMy Setup..."

# Environment
echo "📋 Setting up environment..."
cp .env.example .env

# Docker
echo "🐳 Starting Docker services..."
docker-compose up -d postgres redis

# Wait for database
echo "⏳ Waiting for PostgreSQL..."
sleep 5

# Backend
echo "🔧 Running database migrations..."
cd services/GoingMy.UserService
dotnet ef database update -s src/GoingMy.User.API
cd ../GoingMy.PostService
dotnet ef database update -s src/GoingMy.Post.API
cd ../..

# Frontend
echo "📦 Installing frontend dependencies..."
cd web
npm install
cd ..

echo ""
echo "✅ Setup complete!"
echo ""
echo "Starting services..."
echo ""
echo "To start development:"
echo "  Terminal 1: cd services/GoingMy.UserService && dotnet watch run"
echo "  Terminal 2: cd services/GoingMy.PostService && dotnet watch run"
echo "  Terminal 3: cd web && ng serve"
echo ""
echo "Then open: http://localhost:4200"
```

## Daily Development Commands

```bash
# Start development environment
docker-compose up -d postgres redis

# Start backend services
cd services/GoingMy.UserService && dotnet watch run &
cd services/GoingMy.PostService && dotnet watch run &

# Start frontend
cd web && ng serve

# Run tests
dotnet test

# View logs
docker-compose logs -f

# Stop everything
docker-compose down
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

## Changelog
- v1.0: Initial development setup runbook
- v1.1: Added quick-start script
