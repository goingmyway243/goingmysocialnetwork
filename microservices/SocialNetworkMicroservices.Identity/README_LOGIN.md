# Identity Service - Login Endpoints

## Overview
The Identity microservice provides authentication and authorization using OpenIddict (OAuth 2.0 / OpenID Connect). It includes simplified login endpoints for easy testing and development.

## Test Users

The following test users are available for development and testing:

| Username | Password | Email | Roles | Description |
|----------|----------|-------|-------|-------------|
| `admin` | `admin123` | admin@socialnetwork.com | admin, user | Administrator with full privileges |
| `john.doe` | `password123` | john.doe@example.com | user | Regular user |
| `jane.smith` | `password123` | jane.smith@example.com | user | Regular user |
| `test` | `password` | test@example.com | user | Legacy test user |
| `moderator` | `mod123` | moderator@socialnetwork.com | moderator, user | Moderator with elevated privileges |

## API Endpoints

### 1. Simple Login (Recommended for Testing)

**POST** `/api/login`

Simplified login endpoint that validates credentials and returns an OAuth token with user information.

**Request Body:**
```json
{
  "username": "admin",
  "password": "admin123"
}
```

**Response (Success):**
```json
{
  "success": true,
  "accessToken": "eyJhbGciOiJSUzI1Ni...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "refreshToken": "...",
  "user": {
    "id": "1",
    "username": "admin",
    "email": "admin@socialnetwork.com",
    "firstName": "Admin",
    "lastName": "User",
    "roles": ["admin", "user"]
  }
}
```

**Response (Failure):**
```json
{
  "success": false,
  "error": "invalid_credentials",
  "errorDescription": "The username or password is incorrect."
}
```

### 2. Get Test Users

**GET** `/api/login/test-users`

Returns list of available test users (with passwords masked).

**Response:**
```json
[
  {
    "id": "1",
    "username": "admin",
    "password": "********",
    "email": "admin@socialnetwork.com",
    "firstName": "Admin",
    "lastName": "User",
    "roles": ["admin", "user"]
  },
  ...
]
```

### 3. Health Check

**GET** `/api/login/health`

Check if the login service is operational.

**Response:**
```json
{
  "status": "healthy",
  "timestamp": "2026-03-03T10:30:00Z"
}
```

### 4. OAuth Token Endpoint (Standard)

**POST** `/connect/token`

Standard OAuth 2.0 token endpoint supporting multiple grant types.

**Supported Grant Types:**
- `password` - Resource Owner Password Credentials
- `client_credentials` - Client Credentials
- `refresh_token` - Refresh Token
- `authorization_code` - Authorization Code

**Example (Password Grant):**
```
POST /connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=password
&username=admin
&password=admin123
&scope=openid profile email
```

### 5. User Info Endpoint

**GET/POST** `/connect/userinfo`

Get authenticated user information.

**Request:**
```
GET /connect/userinfo
Authorization: Bearer {access_token}
```

**Response:**
```json
{
  "sub": "1",
  "name": "admin",
  "email": "admin@socialnetwork.com"
}
```

## Quick Start

### Using the Simple Login Endpoint

1. **Get list of test users:**
   ```bash
   curl -X GET https://localhost:7001/api/login/test-users
   ```

2. **Login:**
   ```bash
   curl -X POST https://localhost:7001/api/login \
     -H "Content-Type: application/json" \
     -d '{"username": "admin", "password": "admin123"}'
   ```

3. **Use the access token:**
   ```bash
   curl -X GET https://localhost:7001/connect/userinfo \
     -H "Authorization: Bearer {access_token}"
   ```

### Using OAuth Flow Directly

1. **Get token:**
   ```bash
   curl -X POST https://localhost:7001/connect/token \
     -H "Content-Type: application/x-www-form-urlencoded" \
     -d "grant_type=password&username=admin&password=admin123&scope=openid profile email"
   ```

2. **Use the token in API requests:**
   ```bash
   curl -X GET https://localhost:7001/api/protected-endpoint \
     -H "Authorization: Bearer {access_token}"
   ```

## Testing with HTTP Files

Use the provided `TestLogin.http` file with VS Code REST Client extension or Visual Studio:

1. Open `TestLogin.http`
2. Click "Send Request" above any request
3. View response in the response panel

## Development Notes

- **Test users are stored in memory** - User service uses a singleton with hardcoded test users
- **Tokens are self-contained JWTs** - No need for token validation against a database
- **Development certificates** - Using development signing/encryption certificates (not for production)
- **HTTPS required** - Identity service runs on HTTPS by default

## Security Considerations

⚠️ **WARNING:** These test users and simplified endpoints are for **development and testing only**. 

For production:
- Replace test users with a real user database
- Implement proper password hashing (e.g., BCrypt, PBKDF2)
- Use secure certificate management
- Implement rate limiting
- Add account lockout policies
- Implement proper logging and monitoring
- Use secure client credentials
- Validate redirect URIs

## Integration with Other Services

Other microservices can validate tokens issued by this service using OpenIddict validation:

```csharp
builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        options.SetIssuer("https://identity-service-url");
        options.AddAudiences("your_api_audience");
        options.UseSystemNetHttp();
        options.UseAspNetCore();
    });
```

## Troubleshooting

### Token validation fails
- Ensure the issuer URL matches exactly
- Check that audience is configured correctly
- Verify clock synchronization between services

### Login endpoint returns 500
- Check logs for detailed error messages
- Verify HttpClient is registered in DI container
- Ensure token endpoint is accessible

### Invalid credentials error
- Verify username and password match test users
- Check for typos (credentials are case-sensitive)
- Review TestUserService for available users

## Additional Resources

- [OpenIddict Documentation](https://documentation.openiddict.com/)
- [OAuth 2.0 Specification](https://oauth.net/2/)
- [OpenID Connect Specification](https://openid.net/connect/)
