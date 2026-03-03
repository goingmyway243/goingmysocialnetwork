# Integration Guide - Identity & Post Microservices

This guide demonstrates how the Identity and Post microservices work together using OpenIddict for authentication and authorization.

## Architecture Overview

```
┌─────────────┐      1. Request Token       ┌──────────────────┐
│   Client    │ ─────────────────────────> │    Identity      │
│             │                             │   Microservice   │
│             │ <───────────────────────── │  (OpenIddict)    │
└─────────────┘      2. Access Token       └──────────────────┘
       │
       │ 3. API Request with Bearer Token
       │
       ▼
┌─────────────┐                             ┌──────────────────┐
│   Client    │ ─────────────────────────> │      Post        │
│             │                             │   Microservice   │
│             │ <───────────────────────── │  (Protected API) │
└─────────────┘      4. Response           └──────────────────┘
```

## Complete Workflow

### Step 1: Start Both Services

**Terminal 1 - Identity Service:**
```bash
cd SocialNetworkMicroservices.Identity
dotnet run
# Listening on https://localhost:5001
```

**Terminal 2 - Post Service:**
```bash
cd SocialNetworkMicroservices.Post
dotnet run
# Listening on https://localhost:5002
```

### Step 2: Authenticate and Get Access Token

**Request:**
```bash
curl -X POST https://localhost:5001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "username=test" \
  -d "password=password" \
  -d "scope=openid email profile"
```

**Response:**
```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsImtpZCI6IjRBMUI5...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "refresh_token": "CfDJ8...",
  "id_token": "eyJhbGciOiJSUzI1NiIsImtpZCI6IjRBMUI5..."
}
```

**Extract the access_token for use in subsequent requests.**

### Step 3: Call Protected Post Endpoints

#### Get All Posts
```bash
curl -X GET https://localhost:5002/api/posts \
  -H "Authorization: Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6IjRBMUI5..."
```

**Response:**
```json
{
  "userId": "test",
  "username": "test",
  "posts": [
    {
      "id": 1,
      "title": "First post by test",
      "content": "This is the content of the first post",
      "userId": "test",
      "createdAt": "2024-01-15T10:00:00Z"
    }
  ]
}
```

#### Create a New Post
```bash
curl -X POST https://localhost:5002/api/posts \
  -H "Authorization: Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6IjRBMUI5..." \
  -H "Content-Type: application/json" \
  -d '{
    "title": "My First Social Network Post",
    "content": "Hello, this is my first post on the social network!"
  }'
```

**Response:**
```json
{
  "message": "Post created successfully",
  "post": {
    "id": 5432,
    "title": "My First Social Network Post",
    "content": "Hello, this is my first post on the social network!",
    "userId": "test",
    "createdAt": "2024-01-15T12:30:00Z"
  }
}
```

### Step 4: Test Token Expiration and Refresh

#### Use Refresh Token to Get New Access Token
```bash
curl -X POST https://localhost:5001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=refresh_token" \
  -d "refresh_token=CfDJ8..."
```

**Response:**
```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsImtpZCI6IjVCMkM3...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "refresh_token": "CfDJ8..."
}
```

## Testing Scenarios

### Scenario 1: Successful Authentication Flow
```bash
# 1. Get token
TOKEN=$(curl -s -X POST https://localhost:5001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password&username=test&password=password&scope=openid email profile" \
  | jq -r '.access_token')

# 2. Use token to create post
curl -X POST https://localhost:5002/api/posts \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title":"Test Post","content":"Test Content"}'

# 3. Get all posts
curl -X GET https://localhost:5002/api/posts \
  -H "Authorization: Bearer $TOKEN"
```

### Scenario 2: Unauthorized Access (No Token)
```bash
curl -X GET https://localhost:5002/api/posts
# Response: 401 Unauthorized
```

### Scenario 3: Invalid Token
```bash
curl -X GET https://localhost:5002/api/posts \
  -H "Authorization: Bearer invalid_token_here"
# Response: 401 Unauthorized
```

### Scenario 4: Expired Token
```bash
# Wait for token to expire (default 3600 seconds)
curl -X GET https://localhost:5002/api/posts \
  -H "Authorization: Bearer <expired_token>"
# Response: 401 Unauthorized

# Use refresh token to get new access token
curl -X POST https://localhost:5001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=refresh_token&refresh_token=<refresh_token>"
```

## Service Configuration

### Identity Service (Port 5001)
**Endpoints:**
- `POST /connect/token` - Issue access tokens
- `GET /connect/userinfo` - Get user information
- `GET /cookie/login` - Cookie-based login
- `GET /cookie/logout` - Cookie-based logout
- `GET /cookie/protected` - Protected endpoint with cookie auth

**Grant Types Supported:**
- Password Flow
- Client Credentials Flow
- Refresh Token Flow
- Authorization Code Flow

### Post Service (Port 5002)
**Endpoints:**
- `GET /api/posts` - Get all posts
- `GET /api/posts/{id}` - Get post by ID
- `POST /api/posts` - Create new post
- `PUT /api/posts/{id}` - Update post
- `DELETE /api/posts/{id}` - Delete post

**Authentication:**
- Requires Bearer token from Identity service
- Validates tokens using OpenIddict validation

## Service Discovery Configuration

Both services use service discovery for communication. Configure in `appsettings.json`:

```json
{
  "services": {
    "identity": {
      "https": ["https://localhost:5001"]
    },
    "post": {
      "https": ["https://localhost:5002"]
    }
  }
}
```

## Troubleshooting

### Issue: 401 Unauthorized on Post Service

**Possible Causes:**
1. Token not included in request
2. Token expired
3. Token signature invalid
4. Identity service URL misconfigured

**Solution:**
- Verify token is included: `Authorization: Bearer <token>`
- Check token expiration in decoded JWT
- Ensure Identity service is running
- Verify service discovery configuration

### Issue: Cannot Connect to Identity Service

**Possible Causes:**
1. Identity service not running
2. Service discovery configuration incorrect
3. HTTPS certificate issues

**Solution:**
- Start Identity service: `dotnet run --project SocialNetworkMicroservices.Identity`
- Check `appsettings.json` service URLs
- Trust development certificate: `dotnet dev-certs https --trust`

### Issue: Token Validation Fails

**Possible Causes:**
1. Issuer mismatch between services
2. Development certificates not properly configured

**Solution:**
- Verify issuer URL matches in both services
- Regenerate development certificates
- Check logs for detailed error messages

## PowerShell Test Script

Here's a complete PowerShell script to test the integration:

```powershell
# test-integration.ps1

# 1. Get access token
$tokenResponse = Invoke-RestMethod -Uri "https://localhost:5001/connect/token" `
    -Method Post `
    -ContentType "application/x-www-form-urlencoded" `
    -Body "grant_type=password&username=test&password=password&scope=openid email profile"

$accessToken = $tokenResponse.access_token
Write-Host "Access Token obtained: $($accessToken.Substring(0, 20))..." -ForegroundColor Green

# 2. Get all posts
$headers = @{ Authorization = "Bearer $accessToken" }
$posts = Invoke-RestMethod -Uri "https://localhost:5002/api/posts" `
    -Method Get `
    -Headers $headers

Write-Host "Posts retrieved: $($posts.posts.Count) posts" -ForegroundColor Green
$posts.posts | Format-Table

# 3. Create a new post
$newPost = @{
    title = "Test Post from PowerShell"
    content = "This post was created by the integration test script"
} | ConvertTo-Json

$createResponse = Invoke-RestMethod -Uri "https://localhost:5002/api/posts" `
    -Method Post `
    -Headers $headers `
    -ContentType "application/json" `
    -Body $newPost

Write-Host "Post created successfully: ID = $($createResponse.post.id)" -ForegroundColor Green
```

## Security Best Practices

1. **Always use HTTPS** in production
2. **Store tokens securely** (never in localStorage for sensitive apps)
3. **Implement token refresh** before expiration
4. **Use appropriate token lifetimes** (shorter for higher security)
5. **Validate all inputs** on the API side
6. **Implement rate limiting** to prevent abuse
7. **Log authentication attempts** for security monitoring
8. **Use CORS properly** to restrict origins
9. **Rotate signing certificates** regularly in production
10. **Never expose sensitive claims** in access tokens

## Next Steps

1. Add database persistence for posts
2. Implement user-specific post filtering
3. Add post likes/comments features
4. Implement real-time updates with SignalR
5. Add image upload capabilities
6. Implement pagination for large datasets
7. Add full-text search
8. Implement authorization policies (post ownership)
9. Add API versioning
10. Deploy to production environment

## References

- [Identity Service Documentation](../SocialNetworkMicroservices.Identity/README_OPENIDDICT.md)
- [Post Service Documentation](../SocialNetworkMicroservices.Post/README.md)
- [OpenIddict Documentation](https://documentation.openiddict.com/)
- [OAuth 2.0 Specification](https://tools.ietf.org/html/rfc6749)
