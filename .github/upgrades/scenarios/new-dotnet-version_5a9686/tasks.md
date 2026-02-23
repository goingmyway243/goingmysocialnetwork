# SocialNetworkMicroservices .NET 10 Upgrade Tasks

## Overview

This document tracks the execution of upgrading 6 projects from .NET 9.0 to .NET 10.0. All projects will be upgraded simultaneously in a single atomic operation, followed by test validation.

**Progress**: 1/2 tasks complete (50%) ![0%](https://progress-bar.xyz/50)

---

## Tasks

### [✓] TASK-001: Atomic framework and dependency upgrade *(Completed: 2026-02-23 09:30)*
**References**: Plan §Migration Strategy, §Project-by-Project Plans, §Package Update Reference, §Breaking Changes Catalog

- [✓] (1) Update TargetFramework to net10.0 in all 6 projects per Plan §Project-by-Project Plans (Share, ServiceDefaults, Identity, Post, AppHost, Tests)
- [✓] (2) Update Aspire.AppHost.Sdk to 13.1.1 in AppHost project per Plan §Project 5 (AppHost)
- [✓] (3) Update all package references per Plan §Package Update Reference (10 packages: Aspire packages 9.0.0→13.1.1, Microsoft packages 9.x→10.x, OpenTelemetry packages 1.9.0→1.15.0)
- [✓] (4) All project files and package references updated (**Verify**)
- [✓] (5) Restore all dependencies
- [✓] (6) All dependencies restored successfully (**Verify**)
- [✓] (7) Fix JWT API breaking changes in Identity\Program.cs per Plan §Breaking Changes Catalog #1 (lines 101-103, 108-114, 116, 136-138: replace JwtRegisteredClaimNames with string literals, verify JwtSecurityToken classes)
- [✓] (8) Fix TimeSpan API in Tests\WebTests.cs line 21 per Plan §Breaking Changes Catalog #3 (change TimeSpan.FromSeconds(30) to TimeSpan.FromSeconds(30.0))
- [✓] (9) Build solution and fix any remaining compilation errors per Plan §Breaking Changes Catalog
- [✓] (10) Solution builds with 0 errors (**Verify**)
- [✓] (11) Commit changes with message: "TASK-001: Upgrade all projects to .NET 10"

---

### [▶] TASK-002: Run test suite and validate upgrade
**References**: Plan §Testing & Validation Strategy

- [✓] (1) Run all tests in SocialNetworkMicroservices.Tests project
- [✓] (2) Fix any test failures (reference Plan §Breaking Changes Catalog for common issues)
- [✓] (3) Re-run tests after fixes
- [✓] (4) All tests pass with 0 failures (**Verify**)
- [▶] (5) Commit test fixes with message: "TASK-002: Complete testing and validation"

---









