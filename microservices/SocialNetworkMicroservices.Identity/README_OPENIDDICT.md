# OpenIddict Integration - Identity Microservice

This Identity microservice has been successfully integrated with OpenIddict, a flexible OAuth 2.0/OpenID Connect framework for ASP.NET Core.

## What's Changed

### 1. **Packages Added**
- `OpenIddict.AspNetCore` (v5.8.0)
- `OpenIddict.EntityFrameworkCore` (v5.8.0)
- `Microsoft.EntityFrameworkCore.InMemory` (v10.0.3)

### 2. **OpenIddict Configuration**
The service is configured to support:
- Password Flow (Resource Owner Password Credentials)
- Client Credentials Flow
- Refresh Token Flow
- Authorization Code Flow

### 3. **Endpoints Available**

#### Token Endpoint
**POST** `/connect/token`

Request token using different grant types:

**Password Grant Type:**
```bash
curl -X POST https://localhost:5001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password&username=test&password=password&scope=openid email profile"
```

**Client Credentials Grant Type:**
```bash
curl -X POST https://localhost:5001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials&client_id=your_client_id&client_secret=your_client_secret&scope=api"
```

**Refresh Token Grant Type:**
```bash
curl -X POST https://localhost:5001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=refresh_token&refresh_token=YOUR_REFRESH_TOKEN"
```

#### UserInfo Endpoint
**GET/POST** `/connect/userinfo`

Get user information using the access token:
```bash
curl -X GET https://localhost:5001/connect/userinfo \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

### 4. **Authentication Scheme**
The service uses a policy scheme that supports both:
- **OpenIddict Bearer Token Authentication** (for API access)
- **Cookie Authentication** (for browser-based authentication)

The authentication scheme is automatically selected based on the request:
- If `Authorization: Bearer <token>` header is present → OpenIddict validation
- Otherwise → Cookie authentication

### 5. **Test Credentials**
For development/testing purposes:
- **Username:** `test`
- **Password:** `password`

### 6. **Scopes Registered**
- `openid` - OpenID Connect scope
- `email` - Email scope
- `profile` - Profile scope
- `roles` - Roles scope

## How to Test

1. **Start the application:**
```bash
dotnet run --project SocialNetworkMicroservices.Identity
```

2. **Request a token:**
```bash
curl -X POST https://localhost:5001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password&username=test&password=password&scope=openid email profile"
```

Response:
```json
{
  "access_token": "eyJhbGc...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "refresh_token": "...",
  "id_token": "eyJhbGc..."
}
```

3. **Access protected endpoint:**
```bash
curl -X GET https://localhost:5001/weatherforecast \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

4. **Get user information:**
```bash
curl -X GET https://localhost:5001/connect/userinfo \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

## Architecture

### Files Structure
```
SocialNetworkMicroservices.Identity/
├── Program.cs                      # Main application configuration
├── ApplicationDbContext.cs         # EF Core DbContext for OpenIddict
├── AuthorizationController.cs      # OAuth 2.0/OIDC endpoints
└── SocialNetworkMicroservices.Identity.csproj
```

### Key Components

1. **ApplicationDbContext:** Entity Framework Core context that stores OpenIddict entities (applications, authorizations, tokens, scopes)

2. **AuthorizationController:** Handles OAuth 2.0 and OpenID Connect flows
   - Token exchange endpoint
   - UserInfo endpoint
   - Claim destination mapping

3. **Authentication Configuration:** 
   - OpenIddict server with development certificates
   - Cookie authentication for browser flows
   - Policy scheme for automatic authentication selection

## Next Steps

To use this in production, you should:

1. **Replace In-Memory Database:** Use a real database (SQL Server, PostgreSQL, etc.)
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    options.UseOpenIddict();
});
```

2. **Add Client Registration:** Register OAuth clients in the database
3. **Replace Development Certificates:** Use real certificates for signing tokens
4. **Implement User Management:** Add proper user authentication against a user database
5. **Add Authorization Policies:** Implement role-based or policy-based authorization
6. **Configure CORS:** If accessing from different origins
7. **Add Logging and Monitoring:** Track authentication attempts and token issuance

## References
- [OpenIddict Documentation](https://documentation.openiddict.com/)
- [OAuth 2.0 RFC](https://tools.ietf.org/html/rfc6749)
- [OpenID Connect Specification](https://openid.net/specs/openid-connect-core-1_0.html)
