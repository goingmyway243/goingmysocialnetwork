# GoingMy Social Network

**A modern social network platform with microservices architecture, real-time chat, and glassmorphic UI.**

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](../LICENSE) 
![Tech Stack: .NET 10 + Angular 20](https://img.shields.io/badge/Tech%20Stack-_.NET%2010%20%2B%20Angular%2020_%3B%20PostgreSQL-red)
![Architecture: Microservices + Event-Driven](https://img.shields.io/badge/Architecture-Microservices%20%2B%20Event%20Driven-green)

---

## Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Services](#services)
- [Quick Start](#quick-start)
- [Commands](#commands)
- [Documentation](#documentation)
- [Contributing](#contributing)
- [License](#license)

---

## Features

- **User Management**: Registration, profiles, followers, avatars, bios
- **Social Posts**: Create, like, comment on posts with media support
- **Real-Time Chat**: Private one-to-one messaging via SignalR
- **Push Notifications**: Real-time in-app notifications for likes, comments, and follows via SignalR
- **Media Uploads**: File upload, validation, and storage management with saga-based validation orchestration
- **Search**: Full-text search on users and posts via Elasticsearch; trending posts and suggestions
- **Admin Dashboard**: System-wide analytics and user management
- **Authentication**: OpenIddict PKCE OAuth 2.0 with JWT tokens
- **Rate Limiting**: 100 req/10 sec per IP at API Gateway
- **Event-Driven**: RabbitMQ-powered service integration
- **Glassmorphism UI**: Apple-inspired design system with PrimeNG

## Architecture

```
┌──────────────────────────────────────────────────────────────┐
│         Angular 20 Frontend (Glassmorphic UI)                │
│    Feed · Profile · Messages · Admin Dashboard              │
└─────────────────────────┬────────────────────────────────────┘
                          │ JWT + SignalR WebSocket
┌─────────────────────────▼────────────────────────────────────┐
│            API Gateway (YARP Reverse Proxy on :7000)         │
│     JWT Validation · Rate Limiting · CORS · Claim Forward   │
└─────────────────────────┬────────────────────────────────────┘
                          │
  ┌───────────┬───────────┼───────────┬──────────────┬──────────┬─────────┐
  ▼           ▼           ▼           ▼              ▼          ▼         ▼
 Auth      UserProfile  Posts       Chat        Notification  Upload   Search
(.API      (.API       (.API      (.API +       (.API +       (.API    (.API
 :5001)     :5002)     :5003)    SignalR       SignalR        :5006)   :5007)
                                   :5004)        :5005)
  │           │           │           │              │          │         │
  └───────────┴───────────┴───────────┴──────┬───────┴──────────┴─────────┘
                                             ▼
                                    RabbitMQ EventBus
                    PostgreSQL + MongoDB + Elasticsearch
```

**Architecture Principles:**
- **Single Entry Point**: All requests through API Gateway (YARP)
- **JWT at Edge**: Gateway validates tokens before forwarding
- **Service Isolation**: Each microservice owns its database
- **CQRS Pattern**: Commands & queries via MediatR for clean separation
- **Event-Driven**: Services communicate asynchronously via RabbitMQ
- **Defense in Depth**: Downstream services re-validate tokens
- **OIDC Exception**: Angular login calls Auth Service directly for OAuth flow

Each service follows **Clean Architecture**:
- **Domain**: Business logic, entities, value objects
- **Application**: DTOs, commands/queries (MediatR), validators  
- **Infrastructure**: EF Core, repositories, external integrations
- **API**: REST controllers, middleware, configuration

## Tech Stack

### Backend
| Component | Technology | License |
|-----------|-----------|---------|
| Framework | .NET 10 + ASP.NET Core | MIT |
| Architecture | Microservices + CQRS (MediatR) | MIT |
| Database | PostgreSQL 17 + MongoDB | Open Source |
| Search | Elasticsearch | Elastic License 2.0 |
| ORM | Entity Framework Core + Efcore.MongoDb | MIT |
| Auth | OpenIddict PKCE OAuth 2.0 | MIT |
| Messaging | RabbitMQ + MassTransit | Apache 2.0 |
| API Gateway | YARP Reverse Proxy | MIT |
| Interop | .NET Aspire (orchestration) | MIT |

### Frontend
| Component | Technology | License |
|-----------|-----------|---------|
| Framework | Angular 20 | MIT |
| UI Library | PrimeNG + Angular Material | MIT |
| State | Angular Signals | MIT |
| HTTP | Angular HttpClient | MIT |
| Language | TypeScript (strict mode) | Apache 2.0 |
| Styling | CSS + Glassmorphism design tokens | Custom |

### Infrastructure
| Component | Technology | License |
|-----------|-----------|---------|
| Containers | Docker + Docker Compose | Apache 2.0 |
| Orchestration | .NET Aspire (local dev) | MIT |
| Observability | OpenTelemetry + structured logging | Apache 2.0 |
| CI/CD | GitHub Actions | Included |

## Services

| Service | Port | Endpoint | Responsibility |
|---------|------|----------|-----------------|
| **Identity** | 5001 | — | Authentication, JWT tokens, admin accounts (OpenIddict OIDC) |
| **UserProfile** | 5002 | `/api/userprofiles` | User profiles, followers, avatars, bios |
| **Posts** | 5003 | `/api/posts` | Posts, comments, likes, reactions |
| **Chat** | 5004 | `/hubs/chat` | Private conversations, real-time messaging (SignalR) |
| **Notification** | 5005 | `/api/notifications`, `/hubs/notification` | Real-time push notifications (SignalR); RabbitMQ consumers for likes, comments, follows |
| **Upload** | 5006 | `/api/uploads` | Media file upload, validation, storage management; saga-based post-with-media orchestration |
| **Search** | 5007 | `/api/search` | Full-text search, suggestions, trending posts; Elasticsearch index consumer |
| **API Gateway** | 5000 (HTTP) / 7000 (HTTPS) | `/api/*` | Centralized entry point, JWT validation, rate limiting |
| **Web (Angular SPA)** | 4200 | — | Glassmorphic UI, dashboard, feeds |
| **Aspire Dashboard** | 17277 (HTTPS) | — | Service orchestration & monitoring |

## Quick Start

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Setup

#### 1. Clone repository
```bash
git clone https://github.com/goingmyway243/goingmysocialnetwork.git && cd goingmysocialnetwork
```

#### 2. Configure API keys & secrets
- Add your Gemini API key to:
  [Post.API appsettings.json](src/GoingMy.PostService/src/GoingMy.Post.API/appsettings.json)
  → Set `"OpenAI": { "ApiKey": "your-gemini-key" }`

#### 3. (Optional) Configure frontend environment
Edit if needed: [environment.ts](src/GoingMy.Web/src/environments/environment.ts)
  → Adjust authConfig, API Gateway URL, or OAuth settings

#### 4. Run everything via scripts
```bash
./run.ps1              # Windows (PowerShell)
./run.sh              # macOS/Linux
```

**Or run services manually:**

```bash
# Terminal 1: Start backend services
cd src && dotnet run --project GoingMy.AppHost

# Terminal 2: Start frontend
cd src/GoingMy.Web && npm install && npm start
```

> **Note:** Before running, ensure you've added your **Gemini API key** to `src/GoingMy.PostService/src/GoingMy.Post.API/appsettings.json` and reviewed frontend config in `src/GoingMy.Web/src/environments/environment.ts` (if needed).

**Access the Application:**
| Component | URL |
|-----------|-----|
| Frontend | http://localhost:4200 |
| API Gateway | https://localhost:7000 |
| Aspire Dashboard | http://localhost:17277 |

## Commands

### Backend (from `src/GoingMy.{ServiceName}` directory)
```bash
dotnet build GoingMy.{ServiceName}.slnx          # Build service
dotnet test GoingMy.{ServiceName}.slnx           # Run tests
dotnet watch run --project src/GoingMy.{Service}.API  # Dev watch
dotnet ef migrations add {Name} -s {ProjectPath} # Add migration
dotnet ef database update -s {ProjectPath}       # Apply migration
```

### Frontend (from `src/GoingMy.Web`)
```bash
npm install              # Install dependencies
npm start               # Dev server (http://localhost:4200)
npm run build           # Production build
npm test               # Run tests
ng generate component  # Create component
ng generate service    # Create service
npm run lint           # ESLint
```

### Full Stack (from workspace root)
```bash
.\run.ps1              # Windows (all services + web)
./run.sh              # macOS/Linux (all services + web)
.\run.ps1 web         # Windows (web only)
.\run.ps1 services    # Windows (services only)
```

---

## Documentation

- [CHANGELOG](docs/CHANGELOG.md) — Version history and recent changes
- [Architecture Guide](docs/ARCHITECTURE.md) — System design, patterns, deployment
- [API Contracts](docs/API.md) — REST endpoints, request/response schemas
- [Development Guide](docs/DEVELOPMENT.md) — Setup, testing, debugging

**For AI Agents / Copilot Users**: 
- See [.github/copilot-instructions.md](.github/copilot-instructions.md) for AI agent guidelines, skill file references, and Context7 MCP usage
- Backend developers: [.github/instructions/backend.instructions.md](.github/instructions/backend.instructions.md)
- Frontend developers: [.github/instructions/frontend.instructions.md](.github/instructions/frontend.instructions.md)

---

## Contributing

Contributions welcome! Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/YourFeature`)
3. Follow coding standards from backend.instructions.md or frontend.instructions.md
4. Write tests for new functionality
5. Submit a pull request with a clear description

---

## License

[MIT License](LICENSE) — Free for commercial and personal use.

**Educational Project Disclaimer**: This is an educational project for learning full-stack development with microservices architecture. It is not intended for production use with real user data without proper security and compliance review.
