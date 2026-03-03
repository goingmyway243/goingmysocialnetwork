# Post Microservice - Protected Endpoints

This Post microservice provides protected RESTful API endpoints for managing posts, secured with OpenIddict token validation.

## Authentication

The service uses **OpenIddict Validation** to validate Bearer tokens issued by the Identity microservice. All endpoints require authentication.

## Configuration

The service automatically discovers the Identity service URL through service discovery:
```csharp
options.SetIssuer(builder.Configuration.GetServiceUri("identity")!);
```

## API Endpoints

### Base URL
All endpoints are prefixed with `/api/posts`

### 1. Get All Posts
**GET** `/api/posts`

Returns all posts for the authenticated user.

**Headers:**
```
Authorization: Bearer <access_token>
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
      "createdAt": "2024-01-15T10:30:00Z"
    },
    {
      "id": 2,
      "title": "Second post by test",
      "content": "This is the content of the second post",
      "userId": "test",
      "createdAt": "2024-01-15T11:30:00Z"
    }
  ]
}
```

### 2. Get Post by ID
**GET** `/api/posts/{id}`

Returns a specific post by ID.

**Parameters:**
- `id` (path): Post ID (integer)

**Headers:**
```
Authorization: Bearer <access_token>
```

**Response:**
```json
{
  "id": 1,
  "title": "Post #1 by test",
  "content": "This is the content of post #1",
  "userId": "test",
  "createdAt": "2024-01-15T12:00:00Z"
}
```

### 3. Create Post
**POST** `/api/posts`

Creates a new post.

**Headers:**
```
Authorization: Bearer <access_token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "title": "My New Post",
  "content": "This is the content of my new post"
}
```

**Response:**
```json
{
  "message": "Post created successfully",
  "post": {
    "id": 1234,
    "title": "My New Post",
    "content": "This is the content of my new post",
    "userId": "test",
    "createdAt": "2024-01-15T12:00:00Z"
  }
}
```

### 4. Update Post
**PUT** `/api/posts/{id}`

Updates an existing post.

**Parameters:**
- `id` (path): Post ID (integer)

**Headers:**
```
Authorization: Bearer <access_token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "title": "Updated Post Title",
  "content": "Updated post content"
}
```

**Response:**
```json
{
  "message": "Post updated successfully",
  "post": {
    "id": 1,
    "title": "Updated Post Title",
    "content": "Updated post content",
    "userId": "test",
    "createdAt": "2024-01-15T12:00:00Z"
  }
}
```

### 5. Delete Post
**DELETE** `/api/posts/{id}`

Deletes a post.

**Parameters:**
- `id` (path): Post ID (integer)

**Headers:**
```
Authorization: Bearer <access_token>
```

**Response:**
```json
{
  "message": "Post 1 deleted successfully by user test"
}
```

## Testing the API

### 1. Get an Access Token from Identity Service

```bash
curl -X POST https://localhost:5001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password&username=test&password=password&scope=openid email profile"
```

**Response:**
```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsImtpZCI6...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "refresh_token": "...",
  "id_token": "..."
}
```

### 2. Use the Access Token to Call Protected Endpoints

#### Get All Posts
```bash
curl -X GET https://localhost:5002/api/posts \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

#### Get Specific Post
```bash
curl -X GET https://localhost:5002/api/posts/1 \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

#### Create Post
```bash
curl -X POST https://localhost:5002/api/posts \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "My Test Post",
    "content": "This is a test post content"
  }'
```

#### Update Post
```bash
curl -X PUT https://localhost:5002/api/posts/1 \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Updated Title",
    "content": "Updated content"
  }'
```

#### Delete Post
```bash
curl -X DELETE https://localhost:5002/api/posts/1 \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

## User Claims Available

Each authenticated request provides access to the following user claims:

- **sub** (Subject): User ID from the token
- **name**: Username
- **email**: User email (if email scope was granted)

Access claims in your endpoints:
```csharp
var userId = user.FindFirst("sub")?.Value;
var username = user.FindFirst("name")?.Value;
var email = user.FindFirst("email")?.Value;
```

## Error Responses

### 401 Unauthorized
Returned when no valid Bearer token is provided or the token is expired.

```json
{
  "error": "invalid_token",
  "error_description": "The access token is not valid."
}
```

### 403 Forbidden
Returned when the token is valid but the user doesn't have permission to access the resource.

## Architecture

### Dependencies
- **OpenIddict.Validation.AspNetCore** (v5.8.0): Token validation for ASP.NET Core
- **OpenIddict.Validation.SystemNetHttp** (v5.8.0): HTTP client for introspection

### Authentication Flow
1. Client requests token from Identity service (`/connect/token`)
2. Identity service validates credentials and issues access token
3. Client includes access token in `Authorization: Bearer <token>` header
4. Post service validates token with OpenIddict validation middleware
5. If valid, request proceeds to endpoint; if invalid, returns 401 Unauthorized

## Next Steps

To enhance this microservice in production:

1. **Add Database**: Implement actual data persistence with Entity Framework Core
2. **Add Pagination**: Implement pagination for the GET all posts endpoint
3. **Add Filtering**: Add query parameters for filtering posts (by date, title, etc.)
4. **Add Authorization Policies**: Implement role-based or policy-based authorization
5. **Add Validation**: Use FluentValidation or Data Annotations for request validation
6. **Add Logging**: Implement structured logging with Serilog
7. **Add Health Checks**: Implement health check endpoints
8. **Add Rate Limiting**: Protect endpoints from abuse
9. **Add Caching**: Cache frequently accessed data

## Related Documentation

- [Identity Service OpenIddict Integration](../SocialNetworkMicroservices.Identity/README_OPENIDDICT.md)
- [OpenIddict Validation Documentation](https://documentation.openiddict.com/guides/getting-started/integrating-with-a-resource-server.html)
