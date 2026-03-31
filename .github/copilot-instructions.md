# GoingMy Social Network - AI Agent Instructions

## Project Overview
GoingMy is a modern social network platform built with microservice architecture principles. The project emphasizes scalability, maintainability, and contemporary UI/UX design patterns inspired by Apple's glassmorphism aesthetic.

---

## Skill Files � Load Before Working

The project has dedicated instruction files per tech stack. **Always load the relevant skill file before making any changes.**

| When working on... | Load this file |
|--------------------|----------------|
| Any `.cs`, `.csproj`, `.slnx`, `.http` file | [.github/instructions/backend.instructions.md](instructions/backend.instructions.md) |
| Any `.ts`, `.html`, `.css`, `.scss` file inside `src/GoingMy.Web/` | [.github/instructions/frontend.instructions.md](instructions/frontend.instructions.md) |

> These files are auto-applied by GitHub Copilot via their `applyTo` frontmatter. When working manually or in chat, explicitly reference and follow the appropriate file for the task at hand.

---

## Tech Stack Summary

### Backend
- **Framework**: .NET 10
- **Architecture**: Microservices
- **Orchestration**: .NET Aspire
- **Patterns**: CQRS via MediatR, Repository pattern, Dependency Injection

### Frontend
- **Framework**: Angular 20
- **UI Library**: PrimeNG
- **State Management**: Angular Signals
- **Design System**: Apple glassmorphism aesthetic

---

## Services Map

Go directly to the service location � do not search the solution explorer.

| Service | Responsibility | Location |
|---------|----------------|----------|
| AuthService | User authentication & authorization | `src/GoingMy.AuthService/` |
| PostService | Social content & interactions | `src/GoingMy.PostService/` |
| ChatService | Real-time chat & messaging | `src/GoingMy.ChatService/` |
| ServiceDefaults | Shared configurations & extensions | `src/GoingMy.ServiceDefaults/` |
| AppHost | Aspire orchestration host | `src/GoingMy.AppHost/` |
| Web | Angular frontend application | `src/GoingMy.Web/` |

---

## Architecture Principles

- Each microservice owns its own database and API contract
- Services communicate via HTTP/REST; auth tokens passed in `Authorization` headers
- Authentication is centralized in AuthService (JWT)
- All services run with health checks and structured logging
- Use .NET Aspire (`GoingMy.AppHost`) for local development orchestration

---

## AI Agent Guidelines

### Context7 MCP Usage
**Always use Context7 MCP for up-to-date library documentation** � do not rely on training data alone.

1. `resolve-library-id` ? get the Context7 library ID
2. `get-library-docs` ? fetch current documentation
   - `mode='code'` for API references and code examples
   - `mode='info'` for conceptual guides and architecture

Each skill file lists the specific libraries to fetch for that tech stack.

---

## File Locations

- **Workspace Root**: repository root
- **Source**: `src/`
- **Backend Services**: `src/GoingMy.AuthService/`, `src/GoingMy.PostService/`, `src/GoingMy.ChatService/`
- **Frontend**: `src/GoingMy.Web/`
- **Shared Defaults**: `src/GoingMy.ServiceDefaults/`
- **Orchestration**: `src/GoingMy.AppHost/`
- **Documentation**: `./docs/` � changelogs, architecture guides, API contracts
- **Skill Files**: `.github/instructions/` � tech-stack-specific rules

> No markdown files should be created outside of `./docs/` for documentation purposes.

---

## Documentation Structure

Refer to `./docs/` for:
- **CHANGELOG.md** � version history and recent changes
- Architecture guides, API documentation, and important notes
