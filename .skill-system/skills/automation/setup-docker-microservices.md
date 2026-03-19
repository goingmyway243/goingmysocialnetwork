# SKILL: setup-docker-microservices

## Purpose
Automate Docker containerization and orchestration setup for GoingMy microservices to enable consistent development, testing, and deployment environments.

## Use When
- Setting up development environment
- Containerizing a new service
- Configuring multi-service development stack
- Preparing for production deployment

## Required Inputs
- Service names (User, Post, Notification, etc.)
- Database requirements
- Environment configurations

## Expected Output
- Dockerfile for each service
- docker-compose.yml for local development
- Multi-stage builds optimized for production
- Health check configurations

## Execution Approach

### Step 1: Create Dockerfile for .NET Service

```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copy csproj files
COPY ["src/GoingMy.User.API/GoingMy.User.API.csproj", "src/GoingMy.User.API/"]
COPY ["src/GoingMy.User.Domain/GoingMy.User.Domain.csproj", "src/GoingMy.User.Domain/"]
COPY ["src/GoingMy.User.Application/GoingMy.User.Application.csproj", "src/GoingMy.User.Application/"]
COPY ["src/GoingMy.User.Infrastructure/GoingMy.User.Infrastructure.csproj", "src/GoingMy.User.Infrastructure/"]

# Restore
RUN dotnet restore "src/GoingMy.User.API/GoingMy.User.API.csproj"

# Copy source
COPY . .

# Build
RUN dotnet build "src/GoingMy.User.API/GoingMy.User.API.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "src/GoingMy.User.API/GoingMy.User.API.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD dotnet /app/HealthCheck.dll

# Expose port
EXPOSE 80

# Run
ENTRYPOINT ["dotnet", "GoingMy.User.API.dll"]
```

### Step 2: Create Dockerfile for Angular App

```dockerfile
# Stage 1: Build
FROM node:18-alpine AS build
WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci

# Copy source
COPY . .

# Build
RUN npm run build -- --configuration production

# Stage 2: Serve
FROM nginx:alpine
COPY --from=build /app/dist/goingmy /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

### Step 3: Create docker-compose.yml

```yaml
version: '3.8'

services:
  # Database
  postgres:
    image: postgres:13
    container_name: goingmy-postgres
    environment:
      POSTGRES_DB: goingmy
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

  # Cache
  redis:
    image: redis:7-alpine
    container_name: goingmy-redis
    ports:
      - "6379:6379"
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

  # API Gateway (optional)
  api-gateway:
    build:
      context: .
      dockerfile: services/ApiGateway/Dockerfile
    container_name: goingmy-gateway
    ports:
      - "5000:80"
    depends_on:
      - user-service
      - post-service
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    networks:
      - goingmy-network

  # User Service
  user-service:
    build:
      context: .
      dockerfile: services/GoingMy.UserService/src/GoingMy.User.API/Dockerfile
    container_name: goingmy-user-service
    ports:
      - "5001:80"
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=goingmy;Username=postgres;Password=postgres
      - Redis__Host=redis
      - Redis__Port=6379
    networks:
      - goingmy-network
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  # Post Service
  post-service:
    build:
      context: .
      dockerfile: services/GoingMy.PostService/src/GoingMy.Post.API/Dockerfile
    container_name: goingmy-post-service
    ports:
      - "5002:80"
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=goingmy;Username=postgres;Password=postgres
      - Redis__Host=redis
      - Redis__Port=6379
    networks:
      - goingmy-network
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  # Frontend
  web:
    build:
      context: .
      dockerfile: web/Dockerfile
    container_name: goingmy-web
    ports:
      - "4200:80"
    depends_on:
      - api-gateway
    environment:
      - NGINX_HOST=web
      - NGINX_PORT=80
    networks:
      - goingmy-network

  # Message Queue (optional)
  rabbitmq:
    image: rabbitmq:3.11-management-alpine
    container_name: goingmy-rabbitmq
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "-q", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - goingmy-network

networks:
  goingmy-network:
    driver: bridge

volumes:
  postgres_data:
```

### Step 4: Build and Run Commands

```bash
# Build all images
docker-compose build

# Start all services
docker-compose up -d

# Check status
docker-compose ps
docker-compose logs -f

# View specific service logs
docker-compose logs user-service

# Stop all services
docker-compose down

# Remove volumes (reset data)
docker-compose down -v

# Rebuild specific service
docker-compose build user-service
docker-compose up -d user-service
```

### Step 5: Health Checks

**For .NET Service:**
```csharp
// In Program.cs
builder.Services.AddHealthChecks()
    .AddDbContextCheck<UserDbContext>()
    .AddRedis(builder.Configuration.GetConnectionString("Redis"));

app.MapHealthChecks("/health");
```

**For Nginx:**
```nginx
# In nginx.conf
location / {
    proxy_pass http://api-gateway:5000;
    proxy_http_version 1.1;
    proxy_set_header Connection keep-alive;
    proxy_set_header Host $host;
    proxy_cache_bypass $http_upgrade;
}
```

### Step 6: Development Environment Setup Script

```bash
#!/bin/bash
# setup-dev-env.sh

echo "Building GoingMy development environment..."

# Build images
docker-compose build

# Start services
docker-compose up -d

# Wait for database
echo "Waiting for PostgreSQL..."
until docker-compose exec postgres pg_isready -U postgres; do
  echo "PostgreSQL is unavailable - sleeping"
  sleep 1
done

# Run migrations
docker-compose exec user-service dotnet ef database update

# Display status
echo ""
echo "✅ Development environment started!"
echo ""
echo "Services running:"
docker-compose ps
echo ""
echo "Access points:"
echo "  Frontend:  http://localhost:4200"
echo "  API Gateway: http://localhost:5000"
echo "  User Service: http://localhost:5001"
echo "  Post Service: http://localhost:5002"
echo "  RabbitMQ: http://localhost:15672"
echo "  PostgreSQL: localhost:5432"
echo ""
```

## Quality Criteria
- All services containerized correctly
- Docker images lean and optimized
- Health checks configured
- Environment variables externalized
- Volume mounting for development
- Network isolation proper

## Verification Checklist
- [ ] `docker-compose build` succeeds
- [ ] `docker-compose up -d` starts all services
- [ ] Health checks pass for all services
- [ ] Database accessible from containers
- [ ] Services communicate properly
- [ ] Logs accessible via docker-compose logs
- [ ] Volumes persist data correctly

## Edge Cases
- Service startup order (use depends_on)
- Port conflicts on host machine
- Database migrations on container start
- Environment variable secrets

## References
- Docker Best Practices
- Docker Compose Documentation
- Multi-stage Build Guide

## Changelog
- v1.0: Docker setup for microservices
