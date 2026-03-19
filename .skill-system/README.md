# GoingMy Skill System Guide

Welcome to the GoingMy Social Network Project Skill System! This directory contains structured operational knowledge for building, maintaining, and deploying our .NET + Angular microservices social network with Apple glassmorphism UI.

## 📚 What is This?

This skill system is built on a 5-layer model:

```
Layer 1: Intent     → What problem are we solving?
Layer 2: Knowledge  → What do we need to know?
Layer 3: Execution  → How do we do it? (scripts, templates)
Layer 4: Verify     → How do we check it's right?
Layer 5: Evolve     → What did we learn? (gotchas, lessons)
```

Each skill is a **folder**, not a single file, containing:
- `SKILL.md` - The main skill documentation
- `examples/` - Sample implementations
- `templates/` - Ready-to-use templates
- `verification/` - Checklists and tests
- `changelog.md` - Evolution history

## 🗂️ Directory Structure

```
.skill-system/
├── context/              # Project context & definitions
│   ├── business-goal.md
│   ├── stakeholders.md
│   ├── scope.md
│   └── terminology.md
│
├── skills/               # Organized skill modules
│   ├── knowledge/        # Foundational knowledge
│   │   ├── microservices-architecture.md
│   │   ├── dotnet-coding-standards.md
│   │   ├── angular-component-architecture.md
│   │   └── glassmorphism-design-system.md
│   │
│   ├── scaffolding/      # Project structure templates
│   │   ├── scaffold-netcore-microservice.md
│   │   └── scaffold-angular-feature-module.md
│   │
│   ├── verification/     # Quality assurance
│   │   ├── verify-code-quality-dotnet.md
│   │   ├── verify-api-contracts.md
│   │   └── verify-ui-consistency.md
│   │
│   ├── automation/       # Automation scripts
│   │   └── setup-docker-microservices.md
│   │
│   ├── runbooks/         # Step-by-step procedures
│   │   ├── local-development-setup.md
│   │   └── deploy-production.md
│   │
│   └── review/           # Review & approval
│       ├── architecture-decisions.md
│       └── pull-requests.md
│
├── templates/            # Reusable code templates
├── examples/             # Example implementations
├── lessons/              # Learning from failures
└── README.md             # This file
```

## 🎯 Quick Start

### New to the Project?

1. **Start here:** [context/business-goal.md](context/business-goal.md)
   - Understand what we're building
   - Why we're building it
   - Key stakeholders

2. **Understand the terminology:** [context/terminology.md](context/terminology.md)
   - Microservices concepts
   - Domain terms
   - Design principles

3. **Learn core principles:**
   - [skills/knowledge/microservices-architecture.md](skills/knowledge/microservices-architecture.md)
   - [skills/knowledge/dotnet-coding-standards.md](skills/knowledge/dotnet-coding-standards.md)
   - [skills/knowledge/angular-component-architecture.md](skills/knowledge/angular-component-architecture.md)

### Setting Up Development Environment?

**Follow:** [skills/runbooks/local-development-setup.md](skills/runbooks/local-development-setup.md)

This will guide you through:
- Repository cloning
- Dependency installation
- Database setup
- Service startup
- Troubleshooting

Takes ~15 minutes for experienced developers.

### Creating a New Microservice?

**Use:** [skills/scaffolding/scaffold-netcore-microservice.md](skills/scaffolding/scaffold-netcore-microservice.md)

Includes:
- Project structure template
- Multi-layer architecture setup
- API controller patterns
- Test configuration
- Docker support

### Building Angular Components?

**Follow:** [skills/scaffolding/scaffold-angular-feature-module.md](skills/scaffolding/scaffold-angular-feature-module.md)

Covers:
- Feature module structure
- Smart/dumb component pattern
- RxJS reactive patterns
- Lazy loading setup
- Service architecture

### Code Review?

**Use:** [skills/review/pull-requests.md](skills/review/pull-requests.md)

6-layer review approach covering:
- Context & scope
- Architecture alignment
- Implementation quality
- Test coverage
- Performance & security
- Documentation

### Before Deploying to Production?

**Follow:** [skills/runbooks/deploy-production.md](skills/runbooks/deploy-production.md)

Complete deployment checklist including:
- Pre-deployment validation
- Database migration strategy
- Blue-green deployment
- Smoke testing
- Rollback procedures

### Verifying Code Quality?

**Use these verification skills:**
- [verify-code-quality-dotnet.md](skills/verification/verify-code-quality-dotnet.md) - Backend code review
- [verify-api-contracts.md](skills/verification/verify-api-contracts.md) - API consistency
- [verify-ui-consistency.md](skills/verification/verify-ui-consistency.md) - Frontend quality

## 📖 How to Use Each Skill

Each skill is structured with 5 sections:

### 1. **Purpose** 
What is this skill for? When should you use it?

### 2. **Knowledge Area**
The foundational concepts and principles you need to understand.

### 3. **Execution**
Step-by-step procedures, templates, scripts, and commands you can actually use.

### 4. **Verification**
Checklists and criteria to verify the output is correct.

### 5. **Evolution**
Lessons learned, edge cases, gotchas from real usage.

## 🔍 Example Workflow

**Scenario: You're creating a new Post Service**

```
1. Understand architecture:
   → Read: skills/knowledge/microservices-architecture.md

2. Design the service:
   → Review: skills/review/architecture-decisions.md
   → Consider: service boundaries, data persistence, APIs

3. Scaffold the project:
   → Follow: skills/scaffolding/scaffold-netcore-microservice.md
   → Run the commands to create folder structure

4. Implement the service:
   → Reference: skills/knowledge/dotnet-coding-standards.md
   → Follow SOLID principles and patterns

5. Verify implementation:
   → Use: skills/verification/verify-code-quality-dotnet.md
   → Use: skills/verification/verify-api-contracts.md

6. Containerize:
   → Follow: skills/automation/setup-docker-microservices.md
   → Create Dockerfile and add to docker-compose.yml

7. Test locally:
   → Use: skills/runbooks/local-development-setup.md
   → Verify service starts and responds

8. Code review:
   → Use: skills/review/pull-requests.md
   → Comprehensive review checklist

9. Deploy:
   → Follow: skills/runbooks/deploy-production.md
   → Blue-green deployment with validation
```

## 🎓 Learning Paths

### Backend Engineer Learning Path
1. microservices-architecture.md
2. dotnet-coding-standards.md
3. scaffold-netcore-microservice.md
4. verify-code-quality-dotnet.md
5. verify-api-contracts.md
6. local-development-setup.md
7. deploy-production.md

### Frontend Engineer Learning Path
1. microservices-architecture.md (to understand backend)
2. angular-component-architecture.md
3. glassmorphism-design-system.md
4. scaffold-angular-feature-module.md
5. verify-ui-consistency.md
6. local-development-setup.md
7. pull-requests.md (for reviews)

### DevOps/Infra Engineer Learning Path
1. microservices-architecture.md
2. setup-docker-microservices.md
3. local-development-setup.md
4. deploy-production.md
5. architecture-decisions.md

### Tech Lead/Architect Path
1. All context files (business-goal, stakeholders, scope, terminology)
2. architecture-decisions.md
3. microservices-architecture.md
4. All verification skills
5. All review skills
6. deploy-production.md

## ✅ Quality Framework: 4Cs

All outputs should pass the 4C verification:

### **Correctness** - Is it right?
- Logic correct
- No edge case bugs
- Error handling complete
- Performance acceptable

### **Completeness** - Is nothing missing?
- Tests > 80% coverage
- Documentation present
- Configuration externalized
- All paths covered

### **Context-Fit** - Is it appropriate?
- Follows domain terminology
- Aligns with architecture
- Respects service boundaries
- Follows code standards

### **Consequence** - Will it work in production?
- No data loss risk
- Recoverable from failures
- Securely implemented
- Scalable design

## 🚀 Tips for Best Results

### ✅ DO:
- Read the entire skill before acting
- Follow the execution steps in order
- Use provided templates and scripts
- Verify output using checklists
- Document your learnings in lessons/

### ❌ DON'T:
- Skip the knowledge section
- Modify verification criteria
- Hard-code values that should be configurable
- Commit without running verification checklist
- Ignore edge cases mentioned in skills

## 🐛 Found an Issue?

When you encounter:
- A skill that's incomplete
- A broken script
- A missing edge case
- An outdated procedure

**Update it:**
1. Note the issue
2. Test the fix
3. Update the skill file
4. Record the lesson in changelog.md
5. Share with team

## 📝 Adding New Skills

When creating new operational knowledge:

1. **Identify the need** - What task keeps requiring explanation?
2. **Choose a category** - Knowledge, Scaffolding, Verification, Automation, Runbook, or Review?
3. **Use the template** - Follow the structure: Purpose, Knowledge, Execution, Verification, Evolution
4. **Add examples** - Include real code, scripts, or walkthroughs
5. **Create verifications** - How will others know they did it right?
6. **Document learnings** - Capture gotchas and edge cases

## 🔗 Related Documents

- **Architecture Documentation** - src/ARCHITECTURE.md
- **API Documentation** - services/*/API.md
- **Database Schema** - docs/DATABASE.md
- **CI/CD Pipeline** - .github/workflows/
- **Team Wiki** - (if available)

## 👥 Team Contacts

- **Tech Lead:** [Name] - Architecture decisions, complex problems
- **Backend Lead:** [Name] - .NET services, database design
- **Frontend Lead:** [Name] - Angular, UI/UX
- **DevOps:** [Name] - Infrastructure, deployments

## 📊 Skill Usage Statistics

Track skill adoption:
- Most used skills → keep updated
- Least used skills → might need simplification
- High error rates → need clarification
- New gotchas → add to evolution

## 🎯 Success Metrics

This skill system works when:

- ✅ New developers productive within 1 day
- ✅ Services ship with > 80% test coverage
- ✅ Code reviews < 2 hours turnaround
- ✅ Deployments complete < 20 minutes
- ✅ Zero unplanned downtime each sprint
- ✅ Team refers to skills instead of asking
- ✅ Lessons captured and shared automatically

## 📚 External Resources

- [Sam Newman - Building Microservices](https://samnewman.io/books/building_microservices_2nd_edition/)
- [Microsoft .NET Architecture](https://docs.microsoft.com/en-us/dotnet/architecture/)
- [Angular Best Practices](https://angular.io/guide/styleguide)
- [Apple Design Guidelines](https://developer.apple.com/design/human-interface-guidelines/)\n\n---\n\n**Version:** 1.0  \n**Last Updated:** March 2024  \n**Maintained By:** GoingMy Development Team
