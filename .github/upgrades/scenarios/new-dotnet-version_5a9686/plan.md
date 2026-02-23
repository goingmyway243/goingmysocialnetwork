# .NET 10 Upgrade Plan

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Migration Strategy](#migration-strategy)
3. [Detailed Dependency Analysis](#detailed-dependency-analysis)
4. [Project-by-Project Plans](#project-by-project-plans)
5. [Package Update Reference](#package-update-reference)
6. [Breaking Changes Catalog](#breaking-changes-catalog)
7. [Risk Management](#risk-management)
8. [Testing & Validation Strategy](#testing--validation-strategy)
9. [Complexity & Effort Assessment](#complexity--effort-assessment)
10. [Source Control Strategy](#source-control-strategy)
11. [Success Criteria](#success-criteria)

---

## Executive Summary

### Overview
Upgrade **6 projects** from **.NET 9.0** to **.NET 10.0 (LTS)** within the SocialNetworkMicroservices solution. This is an Aspire-based microservices architecture with moderate complexity due to JWT authentication patterns and deprecated API usage.

### Key Statistics
- **Projects**: 6 (all SDK-style)
- **Total Issues**: 47 (19 mandatory, 23 potential, 5 optional)
- **Affected Files**: 9
- **Estimated Code Impact**: ~5.2% of codebase (24+ LOC out of 463 total)
- **Package Updates**: 10 packages require version updates
- **Deprecated Packages**: 5 package instances are deprecated

### Critical Findings
1. **JWT API Breaking Changes**: 13 binary incompatibilities in Identity service due to `System.IdentityModel.Tokens.Jwt` namespace changes in .NET 10
2. **Aspire Package Updates**: All Aspire 9.0.0 packages deprecated, must upgrade to 13.1.1
3. **Authentication Configuration**: JWT Bearer configuration APIs have source incompatibilities requiring code adjustments
4. **TimeSpan API Change**: `TimeSpan.FromSeconds(long)` overload removed in .NET 10

### Recommended Strategy
**All-at-Once Upgrade** (Single coordinated update)

**Rationale**:
- Small solution with simple dependency structure
- All projects are SDK-style and .NET 9 based
- No multi-targeting complexities
- Test coverage exists for validation
- Issues are well-defined and localized

### Timeline Estimate
- **Preparation**: 15 minutes (SDK validation, branch setup)
- **Execution**: 45-60 minutes (6 projects × 10 minutes average)
- **Testing & Validation**: 30 minutes
- **Total**: ~2 hours

### Risk Level
**Medium** - Breaking API changes in authentication code require careful migration, but clear upgrade paths exist.

---

## Migration Strategy

### Selected Strategy: All-at-Once Upgrade

#### Description
Update all 6 projects simultaneously in a single coordinated operation on the `upgrade-to-NET10` branch.

#### Advantages
✅ Fastest time to completion (single build-test cycle)  
✅ No intermediate incompatible states between projects  
✅ Simplified dependency management (no multi-targeting)  
✅ All Aspire components upgraded together (consistency)  
✅ Single PR for review and testing  

#### Disadvantages
⚠️ Larger changeset to review at once  
⚠️ If issues arise, affects entire solution  
⚠️ Requires full regression testing before merge  

#### Execution Phases

**Phase 1: Foundation Projects** (Level 0 - No dependencies)
1. `SocialNetworkMicroservices.Share` - Minimal changes (TFM only)
2. `SocialNetworkMicroservices.ServiceDefaults` - TFM + Aspire package updates
3. `SocialNetworkMicroservices.Identity` - TFM + packages + JWT API fixes (most complex)
4. `SocialNetworkMicroservices.Post` - TFM + packages + JWT config fixes

**Phase 2: Orchestration Layer** (Level 1)
5. `SocialNetworkMicroservices.AppHost` - TFM + Aspire hosting packages

**Phase 3: Testing Layer** (Level 2)
6. `SocialNetworkMicroservices.Tests` - TFM + packages + TimeSpan API fix

#### Rollback Strategy
If critical issues detected:
- `git checkout master` to revert all changes
- Branch `upgrade-to-NET10` preserved for investigation
- No production impact (work isolated to feature branch)

---

## Detailed Dependency Analysis

### Project Dependency Graph

```
Level 0 (Foundation):
├── SocialNetworkMicroservices.Identity (21 issues, 14 mandatory)
├── SocialNetworkMicroservices.Post (8 issues, 1 mandatory)
├── SocialNetworkMicroservices.ServiceDefaults (6 issues, 1 mandatory)
└── SocialNetworkMicroservices.Share (1 issue, 1 mandatory)

Level 1 (Orchestration):
└── SocialNetworkMicroservices.AppHost (7 issues, 1 mandatory)
    └── Depends on: Post

Level 2 (Testing):
└── SocialNetworkMicroservices.Tests (4 issues, 1 mandatory)
    └── Depends on: AppHost
```

### Dependency Rationale for Execution Order

**Why Level 0 First?**
- No dependencies = can be upgraded independently
- Identity and Post are runtime services, must be stable before orchestration
- Share library may be referenced by other projects
- ServiceDefaults provides common configurations

**Why AppHost in Level 1?**
- Depends on Post project reference
- Must upgrade only after Post is stable
- Orchestrates service discovery and hosting

**Why Tests Last?**
- Depends on AppHost (integration tests)
- Validates entire solution after all upgrades
- Can detect issues across all layers

### NuGet Package Dependencies

**Aspire Packages** (All linked, must upgrade together):
- Aspire.Hosting.AppHost
- Aspire.Hosting.PostgreSQL
- Aspire.Hosting.Redis
- Aspire.Hosting.Testing

**Microsoft Framework Packages** (Version-locked to .NET version):
- Microsoft.AspNetCore.Authentication.JwtBearer
- Microsoft.AspNetCore.OpenApi
- Microsoft.Extensions.Http.Resilience
- Microsoft.Extensions.ServiceDiscovery

**Third-Party Packages** (Compatible but upgrades available):
- OpenTelemetry.Instrumentation.AspNetCore
- OpenTelemetry.Instrumentation.Http

---

## Project-by-Project Plans

### 1. SocialNetworkMicroservices.Share
**Path**: `SocialNetworkMicroservices.Share\SocialNetworkMicroservices.Share.csproj`  
**Current TFM**: net9.0  
**Target TFM**: net10.0  
**Complexity**: ⭐ Low (1/5)  

#### Issues
- `Project.0002`: Target framework update required

#### Changes Required
1. Update `<TargetFramework>` from `net9.0` to `net10.0`

#### Validation
- Build succeeds
- No API compatibility issues (class library with no dependencies)

---

### 2. SocialNetworkMicroservices.ServiceDefaults
**Path**: `SocialNetworkMicroservices.ServiceDefaults\SocialNetworkMicroservices.ServiceDefaults.csproj`  
**Current TFM**: net9.0  
**Target TFM**: net10.0  
**Complexity**: ⭐⭐ Medium-Low (2/5)  

#### Issues
- `Project.0002`: Target framework update required
- `NuGet.0002`: Package upgrades recommended (4 packages)
- `NuGet.0005`: Deprecated packages (4 packages)

#### Changes Required

**1. Update Target Framework**
```xml
<TargetFramework>net10.0</TargetFramework>
```

**2. Update NuGet Packages**
```xml
<!-- Before -->
<PackageReference Include="Microsoft.Extensions.Http.Resilience" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.ServiceDiscovery" Version="9.0.0" />
<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.Runtime" Version="1.9.0" />

<!-- After -->
<PackageReference Include="Microsoft.Extensions.Http.Resilience" Version="10.3.0" />
<PackageReference Include="Microsoft.Extensions.ServiceDiscovery" Version="10.3.0" />
<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.15.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.15.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.Runtime" Version="1.9.0" />
```

#### Validation
- Build succeeds
- No breaking API changes in this project

---

### 3. SocialNetworkMicroservices.Identity
**Path**: `SocialNetworkMicroservices.Identity\SocialNetworkMicroservices.Identity.csproj`  
**Current TFM**: net9.0  
**Target TFM**: net10.0  
**Complexity**: ⭐⭐⭐⭐ High (4/5)  

#### Issues
- `Project.0002`: Target framework update required
- `NuGet.0002`: Package upgrades recommended (2 packages)
- `Api.0001`: 13 binary incompatibilities (JWT token APIs)
- `Api.0002`: 5 source incompatibilities (JWT Bearer configuration)

#### Changes Required

**1. Update Target Framework**
```xml
<TargetFramework>net10.0</TargetFramework>
```

**2. Update NuGet Packages**
```xml
<!-- Before -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.2" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.2" />

<!-- After -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.3" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.3" />
```

**3. Fix JWT API Binary Incompatibilities**

**File**: `Program.cs`

**Breaking Change**: `System.IdentityModel.Tokens.Jwt` namespace types moved/changed in .NET 10

**Lines 101-103, 136-138**: `JwtRegisteredClaimNames.Sub` and `JwtRegisteredClaimNames.Jti`
```csharp
// Before (.NET 9)
using System.IdentityModel.Tokens.Jwt;
var claims = new[]
{
    new Claim(JwtRegisteredClaimNames.Sub, "user_id_123"),
    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
};

// After (.NET 10) - Use System.Security.Claims.ClaimTypes or string literals
using System.Security.Claims;
var claims = new[]
{
    new Claim(ClaimTypes.NameIdentifier, "user_id_123"), // Sub → NameIdentifier
    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Jti still available
};

// Alternative: Use string literals directly
var claims = new[]
{
    new Claim("sub", "user_id_123"),
    new Claim("jti", Guid.NewGuid().ToString())
};
```

**Lines 108-114, 116**: `JwtSecurityTokenHandler` and `JwtSecurityToken`
```csharp
// Before (.NET 9)
using System.IdentityModel.Tokens.Jwt;
var token = new JwtSecurityToken(
    issuer: jwtSettings["Issuer"],
    audience: jwtSettings["Audience"],
    claims: claims,
    expires: expiresTime.DateTime,
    signingCredentials: creds
);
var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

// After (.NET 10) - Classes still exist but may require package reference
// Add NuGet: System.IdentityModel.Tokens.Jwt (if not already present)
using System.IdentityModel.Tokens.Jwt;
var token = new JwtSecurityToken(
    issuer: jwtSettings["Issuer"],
    audience: jwtSettings["Audience"],
    claims: claims,
    expires: expiresTime.DateTime,
    signingCredentials: creds
);
var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
```

**4. Fix JWT Bearer Configuration (Source Incompatibilities)**

**Lines 20-56**: `AddJwtBearer` and `TokenValidationParameters`
```csharp
// Configuration remains largely compatible, verify these properties still work:
// - TokenValidationParameters property assignment
// - AddJwtBearer extension method

// No code changes expected - source incompatibilities are warnings
// Verify after package upgrade that authentication still functions correctly
```

#### Validation
- Build succeeds with no errors
- JWT token generation endpoint returns valid tokens
- JWT authentication middleware validates tokens correctly
- Manual test: `/token` and `/cookie/token` endpoints

---

### 4. SocialNetworkMicroservices.Post
**Path**: `SocialNetworkMicroservices.Post\SocialNetworkMicroservices.Post.csproj`  
**Current TFM**: net9.0  
**Target TFM**: net10.0  
**Complexity**: ⭐⭐⭐ Medium (3/5)  

#### Issues
- `Project.0002`: Target framework update required
- `NuGet.0002`: Package upgrades recommended (2 packages)
- `Api.0002`: 5 source incompatibilities (JWT Bearer configuration)

#### Changes Required

**1. Update Target Framework**
```xml
<TargetFramework>net10.0</TargetFramework>
```

**2. Update NuGet Packages**
```xml
<!-- Before -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.2" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.2" />

<!-- After -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.3" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.3" />
```

**3. Fix JWT Bearer Configuration (Source Incompatibilities)**

**File**: `Program.cs`, **Lines 10-20**

**Issue**: `AddJwtBearer` and related properties have source incompatibilities
```csharp
// The existing code should continue to work after package upgrade:
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration.GetServiceUri("identity");
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true
        };
    });

// No immediate changes required - verify functionality after upgrade
```

#### Validation
- Build succeeds
- Authenticated endpoints require valid JWT tokens
- Integration test with Identity service succeeds

---

### 5. SocialNetworkMicroservices.AppHost
**Path**: `SocialNetworkMicroservices.AppHost\SocialNetworkMicroservices.AppHost.csproj`  
**Current TFM**: net9.0  
**Target TFM**: net10.0  
**Complexity**: ⭐⭐ Medium-Low (2/5)  

#### Issues
- `Project.0002`: Target framework update required
- `NuGet.0002`: 3 package upgrades recommended
- `NuGet.0005`: 3 packages deprecated (Aspire 9.0.0)

#### Changes Required

**1. Update Target Framework**
```xml
<TargetFramework>net10.0</TargetFramework>
```

**2. Update Aspire SDK**
```xml
<!-- Before -->
<Sdk Name="Aspire.AppHost.Sdk" Version="9.0.0" />

<!-- After -->
<Sdk Name="Aspire.AppHost.Sdk" Version="13.1.1" />
```

**3. Update NuGet Packages**
```xml
<!-- Before -->
<PackageReference Include="Aspire.Hosting.AppHost" Version="9.0.0" />
<PackageReference Include="Aspire.Hosting.PostgreSQL" Version="9.0.0" />
<PackageReference Include="Aspire.Hosting.Redis" Version="9.0.0" />

<!-- After -->
<PackageReference Include="Aspire.Hosting.AppHost" Version="13.1.1" />
<PackageReference Include="Aspire.Hosting.PostgreSQL" Version="13.1.1" />
<PackageReference Include="Aspire.Hosting.Redis" Version="13.1.1" />
```

#### Validation
- Build succeeds
- AppHost can start all services (Post, Identity, PostgreSQL, Redis)
- Service discovery works correctly
- Dashboard accessible

---

### 6. SocialNetworkMicroservices.Tests
**Path**: `SocialNetworkMicroservices.Tests\SocialNetworkMicroservices.Tests.csproj`  
**Current TFM**: net9.0  
**Target TFM**: net10.0  
**Complexity**: ⭐⭐ Medium-Low (2/5)  

#### Issues
- `Project.0002`: Target framework update required
- `NuGet.0002`: Package upgrade recommended (1 package)
- `NuGet.0005`: Package deprecated (Aspire.Hosting.Testing 9.0.0)
- `Api.0002`: 1 source incompatibility (TimeSpan API)

#### Changes Required

**1. Update Target Framework**
```xml
<TargetFramework>net10.0</TargetFramework>
```

**2. Update NuGet Packages**
```xml
<!-- Before -->
<PackageReference Include="Aspire.Hosting.Testing" Version="9.0.0" />

<!-- After -->
<PackageReference Include="Aspire.Hosting.Testing" Version="13.1.1" />
```

**3. Fix TimeSpan API Breaking Change**

**File**: `WebTests.cs`, **Line 21**

**Breaking Change**: `TimeSpan.FromSeconds(long)` overload removed in .NET 10

```csharp
// Before (.NET 9) - uses long overload
await resourceNotificationService.WaitForResourceAsync("webfrontend", KnownResourceStates.Running)
    .WaitAsync(TimeSpan.FromSeconds(30));

// After (.NET 10) - use double overload
await resourceNotificationService.WaitForResourceAsync("webfrontend", KnownResourceStates.Running)
    .WaitAsync(TimeSpan.FromSeconds(30.0)); // Add .0 to make it double literal
```

#### Validation
- Build succeeds
- All tests execute and pass
- No test timeouts or infrastructure issues

---

## Package Update Reference

### Critical Package Updates (Must Upgrade)

| Package | Current | Target | Reason |
|---------|---------|--------|--------|
| Aspire.Hosting.AppHost | 9.0.0 | 13.1.1 | Deprecated, out of support |
| Aspire.Hosting.PostgreSQL | 9.0.0 | 13.1.1 | Deprecated, out of support |
| Aspire.Hosting.Redis | 9.0.0 | 13.1.1 | Deprecated, out of support |
| Aspire.Hosting.Testing | 9.0.0 | 13.1.1 | Deprecated, out of support |
| Microsoft.AspNetCore.Authentication.JwtBearer | 9.0.2 | 10.0.3 | Version alignment with .NET 10 |
| Microsoft.AspNetCore.OpenApi | 9.0.2 | 10.0.3 | Version alignment with .NET 10 |
| Microsoft.Extensions.Http.Resilience | 9.0.0 | 10.3.0 | Version alignment with .NET 10 |
| Microsoft.Extensions.ServiceDiscovery | 9.0.0 | 10.3.0 | Version alignment with .NET 10 |
| OpenTelemetry.Instrumentation.AspNetCore | 1.9.0 | 1.15.0 | Enhanced .NET 10 support |
| OpenTelemetry.Instrumentation.Http | 1.9.0 | 1.15.0 | Enhanced .NET 10 support |

### Compatible Packages (No Update Required)

| Package | Version | Status |
|---------|---------|--------|
| coverlet.collector | 6.0.2 | ✅ Compatible with .NET 10 |
| Microsoft.NET.Test.Sdk | 17.10.0 | ✅ Compatible with .NET 10 |
| OpenTelemetry.Exporter.OpenTelemetryProtocol | 1.9.0 | ✅ Compatible with .NET 10 |
| OpenTelemetry.Extensions.Hosting | 1.9.0 | ✅ Compatible with .NET 10 |
| OpenTelemetry.Instrumentation.Runtime | 1.9.0 | ✅ Compatible with .NET 10 |
| xunit | 2.9.0 | ✅ Compatible with .NET 10 |
| xunit.runner.visualstudio | 2.8.2 | ✅ Compatible with .NET 10 |

### Special Considerations

**Aspire Version Jump (9.0.0 → 13.1.1)**
- Large version jump is expected for .NET 10 support
- Review Aspire release notes: https://learn.microsoft.com/dotnet/aspire/whats-new
- Potential new features/APIs available
- Configuration changes may be required

**JWT Bearer Package Update**
- API surface changes between 9.0.2 and 10.0.3
- Test authentication flows thoroughly
- Verify token validation behavior unchanged

---

## Breaking Changes Catalog

### 1. JWT Token APIs (System.IdentityModel.Tokens.Jwt)

**Impact**: Identity service, 13 binary incompatibilities  
**Severity**: High - Authentication functionality affected  
**Files**: `SocialNetworkMicroservices.Identity\Program.cs`

#### Breaking Change Details

**Type**: Assembly/Namespace relocation  
**Description**: JWT types moved or API signatures changed in .NET 10

**Affected APIs**:
- `JwtRegisteredClaimNames.Sub`
- `JwtRegisteredClaimNames.Jti`
- `JwtSecurityTokenHandler` constructor
- `JwtSecurityTokenHandler.WriteToken()`
- `JwtSecurityToken` constructor

**Resolution Strategy**:
Option 1: Use System.Security.Claims standard claim types
```csharp
// Replace JwtRegisteredClaimNames.Sub with ClaimTypes.NameIdentifier
new Claim(ClaimTypes.NameIdentifier, "user_id")
```

Option 2: Use string literals (recommended for standard JWT claims)
```csharp
// Direct claim type strings
new Claim("sub", "user_id")
new Claim("jti", Guid.NewGuid().ToString())
```

Option 3: Ensure System.IdentityModel.Tokens.Jwt NuGet package is referenced
```xml
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.3.1" />
```

**Recommendation**: Use Option 2 (string literals) for clearest intent and best compatibility.

---

### 2. JWT Bearer Configuration APIs

**Impact**: Identity & Post services, 10 source incompatibilities  
**Severity**: Medium - Configuration code affected  
**Files**: 
- `SocialNetworkMicroservices.Identity\Program.cs` (lines 20-56)
- `SocialNetworkMicroservices.Post\Program.cs` (lines 10-20)

#### Breaking Change Details

**Type**: Source incompatibility (compiler warnings/potential runtime changes)  
**Description**: JWT Bearer options APIs have subtle signature or behavior changes

**Affected APIs**:
- `JwtBearerOptions.TokenValidationParameters` (property setter)
- `JwtBearerOptions.Authority` (property setter)
- `JwtBearerOptions.RequireHttpsMetadata` (property setter)
- `JwtBearerExtensions.AddJwtBearer()` (extension method)
- `JwtBearerDefaults.AuthenticationScheme` (constant)

**Resolution Strategy**:
- Code likely compiles without changes after package update
- Run authentication integration tests to verify behavior
- Monitor for runtime warnings/deprecation messages
- No immediate code changes required unless build fails

**Validation Requirements**:
1. Token validation succeeds with valid tokens
2. Token validation fails with invalid tokens
3. Authority discovery works (Post → Identity)
4. HTTPS metadata validation functions correctly

---

### 3. TimeSpan API Change

**Impact**: Tests project, 1 source incompatibility  
**Severity**: Low - Build failure, easy fix  
**File**: `SocialNetworkMicroservices.Tests\WebTests.cs` (line 21)

#### Breaking Change Details

**Type**: API removal  
**Description**: `TimeSpan.FromSeconds(long)` overload removed in .NET 10 - only `TimeSpan.FromSeconds(double)` remains

**Affected Code**:
```csharp
// Before (.NET 9) - integer literal inferred as long
TimeSpan.FromSeconds(30)

// After (.NET 10) - must be double
TimeSpan.FromSeconds(30.0)  // Add .0 suffix
// OR
TimeSpan.FromSeconds(30d)   // Add 'd' suffix
```

**Reason for Breaking Change**: Type system simplification - single double overload preferred

**Resolution**: Simple literal change (30 → 30.0)

---

## Risk Management

### High-Risk Areas

#### 1. JWT Authentication in Identity Service
**Risk**: Token generation or validation breaks due to API changes  
**Likelihood**: Medium  
**Impact**: Critical (authentication system failure)

**Mitigation**:
- Implement manual testing of all authentication flows
- Test both JWT and Cookie authentication paths
- Verify token claims are correctly generated
- Test token validation in Post service
- Keep .NET 9 environment available for comparison testing

**Contingency**:
- Revert to .NET 9 if authentication fails
- Investigate System.IdentityModel.Tokens.Jwt package addition
- Consult .NET 10 breaking changes documentation

---

#### 2. Aspire Version Jump (9.0 → 13.1)
**Risk**: Hosting configuration incompatibilities or behavior changes  
**Likelihood**: Low-Medium  
**Impact**: High (service orchestration failure)

**Mitigation**:
- Review Aspire 13.1 release notes for breaking changes
- Test AppHost startup and service registration
- Verify PostgreSQL and Redis resource provisioning
- Validate service discovery between Post and Identity
- Check dashboard functionality

**Contingency**:
- Stage Aspire upgrade separately if issues detected
- Test each Aspire component individually
- Check for required configuration migrations

---

#### 3. Integration Between Services
**Risk**: Post service cannot authenticate requests to Identity service  
**Likelihood**: Low  
**Impact**: High (authentication flow broken)

**Mitigation**:
- Integration test: Post → Identity token validation
- Manual test: Authenticated POST request to /posts endpoint
- Verify Authority configuration points to correct Identity URL
- Test in both local and Aspire-hosted environments

**Contingency**:
- Review service discovery configuration
- Check JWT Bearer Authority property behavior in .NET 10
- Verify HTTPS metadata requirements haven't changed

---

### Medium-Risk Areas

#### 4. Test Infrastructure
**Risk**: Aspire.Hosting.Testing API changes break test setup  
**Likelihood**: Low  
**Impact**: Medium (tests cannot run)

**Mitigation**:
- Run all tests after upgrade
- Verify resource notification service still functions
- Check for Aspire testing API deprecations in 13.1

**Contingency**:
- Update test initialization code per Aspire 13.1 docs
- Add missing configuration if required by new version

---

### Low-Risk Areas

#### 5. Class Libraries (Share, ServiceDefaults)
**Risk**: Minimal - only framework and package updates  
**Likelihood**: Very Low  
**Impact**: Low (build errors, easily fixable)

**Mitigation**:
- Build after each update
- No API compatibility issues detected in assessment

---

## Testing & Validation Strategy

### Phase 1: Build Validation
**Objective**: Ensure all projects compile successfully

**Steps**:
1. Build entire solution: `dotnet build SocialNetworkMicroservices.sln`
2. Verify zero errors, zero warnings (or document acceptable warnings)
3. Check for any missing package references

**Success Criteria**: Clean build with all 6 projects

---

### Phase 2: Unit Testing
**Objective**: Verify isolated functionality

**Steps**:
1. Run all unit tests: `dotnet test SocialNetworkMicroservices.Tests.csproj`
2. Verify test pass rate remains 100%
3. Investigate any new failures

**Success Criteria**: All existing tests pass

---

### Phase 3: Integration Testing
**Objective**: Verify service-to-service communication

**Manual Test Scenarios**:

**Test 1: JWT Token Generation (Identity Service)**
```bash
# Start AppHost
dotnet run --project SocialNetworkMicroservices.AppHost

# Test token endpoint
curl -X POST http://localhost:<port>/token
# Expected: Valid JWT token returned
```

**Test 2: Cookie-Based Token Generation**
```bash
curl -X POST http://localhost:<port>/cookie/token
# Expected: JWT token in cookie and response
```

**Test 3: Authenticated Post Creation**
```bash
# Get token from Identity
TOKEN=$(curl -X POST http://localhost:<identity-port>/token | jq -r '.token')

# Create post with authentication
curl -X POST http://localhost:<post-port>/posts \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title":"Test","content":"Content"}'
# Expected: 200 OK with created post
```

**Test 4: Unauthenticated Request Rejection**
```bash
curl -X POST http://localhost:<post-port>/posts \
  -H "Content-Type: application/json" \
  -d '{"title":"Test","content":"Content"}'
# Expected: 401 Unauthorized
```

**Success Criteria**: All 4 scenarios pass

---

### Phase 4: Aspire Infrastructure Testing
**Objective**: Verify Aspire hosting and orchestration

**Steps**:
1. Launch AppHost and verify services start
2. Check Aspire Dashboard loads (http://localhost:15000 or similar)
3. Verify PostgreSQL resource provisioning
4. Verify Redis resource provisioning
5. Check service discovery resolves Identity service from Post service
6. Review telemetry data flows to dashboard

**Success Criteria**: All resources healthy, dashboard functional

---

### Phase 5: Regression Testing
**Objective**: Ensure no functionality degraded

**Steps**:
1. Execute full test suite
2. Manual exploratory testing of key workflows
3. Performance smoke test (response times reasonable)
4. Check for new runtime warnings in logs

**Success Criteria**: No regressions detected

---

## Complexity & Effort Assessment

### Overall Complexity: Medium (3/5)

#### Complexity Factors

**Low Complexity** ✅:
- All projects are SDK-style (.NET Core/5+)
- Small codebase (463 total LOC)
- Simple dependency structure (3 levels)
- All projects currently on .NET 9 (single version jump)
- Clear issue identification from assessment

**Medium Complexity** ⚠️:
- JWT API breaking changes require code modifications
- Aspire major version jump (9 → 13) may have surprises
- Authentication code is critical and sensitive
- Integration testing required (cannot rely on unit tests alone)

**Not Present** ✅:
- No .NET Framework → .NET Core migrations
- No multi-targeting scenarios
- No COM interop or Windows-specific APIs
- No Entity Framework migrations
- No third-party package incompatibilities

---

### Effort Estimation by Project

| Project | Complexity | Estimated Time | Key Work |
|---------|-----------|----------------|----------|
| Share | ⭐ Low | 5 min | TFM update only |
| ServiceDefaults | ⭐⭐ Med-Low | 10 min | TFM + 7 package updates |
| Identity | ⭐⭐⭐⭐ High | 20 min | TFM + packages + JWT code fixes |
| Post | ⭐⭐⭐ Medium | 10 min | TFM + packages + verification |
| AppHost | ⭐⭐ Med-Low | 10 min | TFM + Aspire SDK + 3 packages |
| Tests | ⭐⭐ Med-Low | 10 min | TFM + package + TimeSpan fix |

**Total Estimated Execution**: 65 minutes

**Additional Time**:
- Build/test cycles: 20 minutes
- Issue investigation buffer: 15 minutes
- Integration testing: 20 minutes

**Total Project Time**: ~2 hours

---

### Skill Level Required

**Minimum**: Intermediate .NET developer  
**Preferred**: Senior developer familiar with:
- .NET authentication patterns (JWT, cookies)
- Aspire orchestration
- Breaking change resolution
- Integration testing

---

## Source Control Strategy

### Branch Strategy

**Source Branch**: `master`  
**Target Branch**: `upgrade-to-NET10` (already created)  
**Merge Strategy**: Pull Request with review

### Commit Strategy

**Approach**: Logical commits per phase (not per file)

**Recommended Commits**:
1. `chore: upgrade Share and ServiceDefaults to .NET 10`
2. `chore: upgrade Identity service to .NET 10 and fix JWT APIs`
3. `chore: upgrade Post service to .NET 10`
4. `chore: upgrade AppHost to .NET 10 and Aspire 13.1`
5. `chore: upgrade Tests to .NET 10 and fix TimeSpan API`
6. `chore: update global.json for .NET 10 SDK` (if applicable)

**Rationale**:
- Logical grouping for easier review
- Each commit represents a testable unit
- Easy to bisect if issues found later
- Clear history for future reference

---

### Pull Request Guidelines

**PR Title**: `chore: Upgrade solution to .NET 10`

**PR Description Template**:
```markdown
## Overview
Upgrades all 6 projects from .NET 9 to .NET 10 (LTS).

## Changes Summary
- ✅ Updated target frameworks: net9.0 → net10.0
- ✅ Upgraded Aspire packages: 9.0.0 → 13.1.1
- ✅ Upgraded Microsoft packages: 9.x → 10.x
- ✅ Fixed JWT API breaking changes (Identity service)
- ✅ Fixed TimeSpan API breaking change (Tests)

## Testing Performed
- [x] Clean build (zero errors)
- [x] All unit tests pass
- [x] JWT authentication works (manual testing)
- [x] Post service authenticates with Identity
- [x] Aspire orchestration starts all services
- [x] Dashboard accessible and functional

## Breaking Changes Addressed
1. JWT token generation migrated to string literals
2. TimeSpan.FromSeconds updated to double overload

## Rollback Plan
Revert PR and merge master into upgrade-to-NET10 if issues detected in staging.

## Related Documentation
- Assessment: `.github/upgrades/scenarios/new-dotnet-version_5a9686/assessment.md`
- Plan: `.github/upgrades/scenarios/new-dotnet-version_5a9686/plan.md`
```

---

### Pre-Merge Checklist

Before merging `upgrade-to-NET10` → `master`:

- [ ] All projects build successfully
- [ ] All tests pass (100% pass rate maintained)
- [ ] Manual authentication testing completed
- [ ] Aspire dashboard shows all resources healthy
- [ ] No new runtime warnings in logs
- [ ] Code review completed
- [ ] Staging environment deployment successful (if applicable)
- [ ] Rollback plan documented and tested

---

## Success Criteria

### Technical Success Criteria

#### Must Have (Blocking) ✅
1. **Build Success**: All 6 projects compile with zero errors
2. **Target Framework**: All projects target `net10.0`
3. **Package Versions**: All packages at recommended .NET 10 versions
4. **Test Pass Rate**: 100% of existing tests pass
5. **No Regressions**: All existing functionality works unchanged

#### Should Have (Important) ⚠️
1. **JWT Authentication**: Token generation and validation work correctly
2. **Service Communication**: Post service authenticates with Identity service
3. **Aspire Orchestration**: AppHost starts all services successfully
4. **Resource Provisioning**: PostgreSQL and Redis containers start
5. **Dashboard Access**: Aspire dashboard accessible and shows healthy state

#### Nice to Have (Optional) 💡
1. **Performance**: No degradation in response times
2. **New Features**: Explore new .NET 10 or Aspire 13.1 features
3. **Code Quality**: Refactor deprecated patterns found during upgrade
4. **Documentation**: Update README with .NET 10 requirements

---

### Business Success Criteria

1. **Zero Downtime**: Upgrade deployed without service interruption (if production exists)
2. **Timeline**: Completed within allocated 2-hour window
3. **Risk Mitigation**: Rollback plan tested and ready
4. **Knowledge Transfer**: Team understands changes made and why

---

### Validation Checklist

After execution, verify:

- [ ] **Build**: `dotnet build` succeeds for solution
- [ ] **Tests**: `dotnet test` shows all tests passing
- [ ] **JWT Token Endpoint**: `/token` returns valid JWT
- [ ] **Cookie Token Endpoint**: `/cookie/token` returns JWT in cookie
- [ ] **Authenticated POST**: Creating post with Bearer token succeeds
- [ ] **Unauthorized POST**: Creating post without token returns 401
- [ ] **Aspire Dashboard**: Accessible and shows all services running
- [ ] **PostgreSQL**: Resource shows healthy in dashboard
- [ ] **Redis**: Resource shows healthy in dashboard
- [ ] **Service Discovery**: Post service resolves Identity service URL
- [ ] **No Warnings**: Build produces no new warnings
- [ ] **Git Status**: All changes committed to upgrade-to-NET10 branch

---

## Appendix: Quick Reference Commands

### Build & Test
```bash
# Build entire solution
dotnet build SocialNetworkMicroservices.sln

# Restore packages
dotnet restore SocialNetworkMicroservices.sln

# Run tests
dotnet test SocialNetworkMicroservices.Tests\SocialNetworkMicroservices.Tests.csproj

# Run AppHost
dotnet run --project SocialNetworkMicroservices.AppHost\SocialNetworkMicroservices.AppHost.csproj
```

### Package Management
```bash
# List outdated packages
dotnet list package --outdated

# Update specific package (example)
dotnet add SocialNetworkMicroservices.AppHost package Aspire.Hosting.AppHost --version 13.1.1
```

### Git Operations
```bash
# Check current branch
git branch

# View changes
git status
git diff

# Commit changes
git add .
git commit -m "chore: upgrade to .NET 10"

# Push branch
git push origin upgrade-to-NET10

# Rollback if needed
git checkout master
```

---

## Plan Metadata

- **Plan Version**: 1.0
- **Created**: Based on assessment `new-dotnet-version_5a9686`
- **Solution**: SocialNetworkMicroservices
- **Source Framework**: .NET 9.0
- **Target Framework**: .NET 10.0 (LTS)
- **Strategy**: All-at-Once Upgrade
- **Estimated Duration**: 2 hours
- **Risk Level**: Medium
