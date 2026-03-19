# SKILL: review-architecture-decisions

## Purpose
Review and validate architectural decisions for GoingMy microservices to ensure scalability, maintainability, and alignment with project goals.

## Use When
- Evaluating new service designs
- Planning major features
- Resolving architectural conflicts
- Assessing technical debt
- Onboarding new team members to architecture

## Architectural Decision Review Framework

### 1. Service Boundary Evaluation

**Criteria:**
- [ ] Service has single business capability
- [ ] Clear, well-defined domain boundaries
- [ ] Minimal inter-service dependencies
- [ ] Independent deployment possible
- [ ] Scalability axis clear

**Bad Example:**
```
UserService handles:
- User profiles
- Authentication
- Post management
- Notifications
❌ Too many responsibilities
```

**Good Example:**
```
UserService handles:
- User profiles
- Authentication
- Follower relationships
✅ Single business capability
```

### 2. Data Consistency Pattern Review

**Questions:**
- [ ] Database isolation strategy clear?
- [ ] How are distributed transactions handled?
- [ ] Event-driven sync or request-response?
- [ ] Eventual consistency acceptable?
- [ ] Conflict resolution strategy documented?

**Patterns to Evaluate:**

**Synchronous (Request-Response):**
```
User Service → Post Service (HTTP call)
✅ Strong consistency
❌ Tight coupling
❌ Service dependency chain
```

**Asynchronous (Event-Driven):**
```
Post Service → Event Bus → Notification Service
✅ Loose coupling
✅ Better scalability
⚠️ Eventual consistency
```

### 3. API Contract Evaluation

**Review Points:**
- [ ] REST conventions followed
- [ ] Versioning strategy defined
- [ ] Error handling standardized
- [ ] Security patterns consistent
- [ ] Documentation complete

**Check Example:**
```
POST /api/v1/users/post       ❌ Verb in URL
POST /api/v1/posts            ✅ Resource-oriented
POST /api/v1/posts/{id}/like  ❌ Non-standard
POST /api/v1/posts/{id}/likes ✅ Standard
```

### 4. Performance & Scalability Assessment

**Key Metrics:**
- [ ] Expected throughput defined
- [ ] Latency targets met
- [ ] Database indexing strategy
- [ ] Caching layers identified
- [ ] Load testing planned

**Questions to Ask:**
- What happens at 1000 requests/second?
- Can the service scale horizontally?
- Are there single points of failure?
- What's the database bottleneck?
- Is caching considered?

### 5. Security Review

**Checklist:**
- [ ] Authentication method defined (JWT, OAuth)
- [ ] Authorization patterns consistent
- [ ] Data encryption at rest and in transit
- [ ] API rate limiting configured
- [ ] Secrets management strategy
- [ ] No secrets in code/configs

**Questions:**
- How are inter-service calls authenticated?
- What data needs encryption?
- How are API keys managed?
- Is HTTPS enforced?
- What about DDoS protection?

### 6. Resilience Patterns

**Patterns Required:**
- [ ] Circuit breaker for service calls
- [ ] Retry logic with exponential backoff
- [ ] Timeout configuration
- [ ] Bulkhead pattern (isolation)
- [ ] Fallback strategies
- [ ] Health checks configured

**Review:**
```csharp
// ❌ Bad: No resilience
var result = await httpClient.GetAsync(serviceUrl);

// ✅ Good: With circuit breaker
var policy = Policy
    .Handle<HttpRequestException>()
    .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
    .CircuitBreaker(3, TimeSpan.FromSeconds(30));

var result = await policy.ExecuteAsync(() => 
    httpClient.GetAsync(serviceUrl)
);
```

### 7. Technology Stack Alignment

**Questions:**
- [ ] Technology choices justified?
- [ ] Does each service use appropriate tech?
- [ ] Dependencies are stable/maintained?
- [ ] Team has expertise?
- [ ] Switching cost acceptable if needed?

**Example Evaluation:**
```
✅ User Service in .NET Core
  - Standard for API services
  - Team expertise
  - Good performance
  - Rich library ecosystem

✅ Frontend in Angular
  - Enterprise-grade framework
  - Strong typing
  - Good for complex UIs

✅ Cache layer in Redis
  - Proven for caching
  - Simple API
  - Excellent performance
```

## Architecture Review Checklist

### Design
- [ ] Service boundaries clear
- [ ] Data ownership defined
- [ ] Communication patterns decided
- [ ] Failure modes identified
- [ ] Performance targets set

### Implementation
- [ ] Code follows design intent
- [ ] Error handling comprehensive
- [ ] No hidden dependencies
- [ ] Configuration externalized
- [ ] Logging structured

### Operations
- [ ] Deployment process clear
- [ ] Health checks configured
- [ ] Monitoring/alerting defined
- [ ] Runbooks created
- [ ] Disaster recovery planned

### Team
- [ ] Architecture documented
- [ ] Knowledge shared
- [ ] Decision rationale recorded
- [ ] Onboarding materials ready

## Common Architecture Anti-Patterns

### ❌ The Distributed Monolith
```
Multiple databases with shared tables
Services calling directly into each other's databases
High coupling, all services must deploy together
```
**Fix:** Enforce database isolation and API boundaries

### ❌ Chatty Services
```
Service A → Service B → Service C → Service D
Four HTTP calls to complete one user request
Latency: 4x overhead + network delays
```
**Fix:** Batch operations, API aggregation, cache layer

### ❌ No Circuit Breaker
```
Service A fails
Service B waits for timeout (30s)
Service C waits calling B (60s)
Cascading failure
```
**Fix:** Implement circuit breakers, fail fast

### ❌ Shared Database
```
Post Service and Comment Service share `posts` table
Tight coupling
Data consistency issues
```
**Fix:** Separate databases, eventual consistency

## Go-Forward Architecture Plan Template

**For proposed change:**

1. **Current State**
   - Describe current architecture
   - Identify pain points

2. **Proposed State**
   - New architecture diagram
   - Changed components
   - Unchanged components

3. **Migration Path**
   - Step-by-step rollout plan
   - Backward compatibility strategy
   - Rollback procedure

4. **Trade-offs**
   - Benefits gained
   - Complexity added
   - Team learning curve
   - Migration effort

5. **Success Criteria**
   - Measurable improvement
   - Timeline for assessment
   - Rollback conditions

6. **Risk Assessment**
   - Technical risks
   - Operational risks
   - Mitigation strategies

## Decision Recording (ADR Format)

```markdown
# ADR: Use Event-Driven Architecture for Notifications

## Status: Accepted

## Context
User notifications must scale to millions of events/day.
Current request-response model causes service coupling.

## Decision
Implement event-driven architecture using RabbitMQ.
Post Service publishes `UserPostCreated` event.
Notification Service subscribes and sends notifications.

## Consequences
- ✅ Loose coupling
- ✅ Better scalability
- ✅ Async processing
- ⚠️ Eventual consistency
- ⚠️ Added complexity (message queue)
```

## Quality Criteria
- Architecture supports project goals
- Service boundaries clear
- Technology choices justified
- Scalability path identified
- Security concerns addressed
- Team alignment achieved

## Verification Questions
- [ ] Can you draw the architecture?
- [ ] Can you explain service responsibilities?
- [ ] Do you know failure modes?
- [ ] Can you deploy services independently?
- [ ] Is performance acceptable?
- [ ] Is security comprehensive?

## References
- Building Microservices by Sam Newman
- Microservices Patterns by Chris Richardson
- AWS Well-Architected Framework

## Changelog
- v1.0: Architecture review framework
