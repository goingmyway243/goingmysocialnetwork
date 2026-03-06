# PKCE Login Flow Documentation

## Overview

The PKCE (Proof Key for Code Exchange) login flow has been implemented to provide a more secure authentication method for public clients like mobile apps and SPAs (Single Page Applications). This implementation follows the OAuth 2.0 Authorization Code Flow with PKCE extension (RFC 7636).

## Endpoints

### 1. Generate PKCE Parameters (Helper)
**GET** `/api/login/pkce/generate`

Generates the code_verifier and code_challenge needed for PKCE flow. This is a helper endpoint for testing.

**Response:**
```json
{
  "codeVerifier": "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk",
  "codeChallenge": "E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM",
  "codeChallengeMethod": "S256"
}
```

### 2. PKCE Login
**POST** `/api/login/pkce`

Authenticates the user and returns an authorization code that can be exchanged for tokens.

**Request Body:**
```json
{
  "username": "admin",
  "password": "Admin123!",
  "codeChallenge": "E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM",
  "codeChallengeMethod": "S256",
  "clientId": "mobile-client",
  "redirectUri": "com.socialnetwork.app://callback",
  "scope": "profile email roles"
}
```

**Response (Success):**
```json
{
  "success": true,
  "authorizationCode": "eyJhbGciOiJSUzI1NiIsImtpZCI6...",
  "redirectUri": "com.socialnetwork.app://callback",
  "user": {
    "id": "1",
    "username": "admin",
    "email": "admin@example.com",
    "firstName": "Admin",
    "lastName": "User",
    "roles": ["Admin", "User"]
  }
}
```

### 3. Exchange Authorization Code for Tokens
**POST** `/api/login/pkce/token`

Exchanges the authorization code received from the PKCE login for access and refresh tokens.

**Request Body:**
```json
{
  "code": "eyJhbGciOiJSUzI1NiIsImtpZCI6...",
  "codeVerifier": "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk",
  "clientId": "mobile-client",
  "redirectUri": "com.socialnetwork.app://callback"
}
```

**Response (Success):**
```json
{
  "success": true,
  "accessToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "refreshToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6..."
}
```

## Complete PKCE Flow Example

### Step 1: Generate PKCE Parameters

```bash
curl -X GET "https://localhost:7000/api/login/pkce/generate"
```

Save the `codeVerifier` securely on the client (you'll need it in Step 3). Use the `codeChallenge` in Step 2.

### Step 2: Authenticate and Get Authorization Code

```bash
curl -X POST "https://localhost:7000/api/login/pkce" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "Admin123!",
    "codeChallenge": "E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM",
    "codeChallengeMethod": "S256",
    "clientId": "mobile-client",
    "redirectUri": "com.socialnetwork.app://callback",
    "scope": "profile email roles"
  }'
```

Save the `authorizationCode` from the response.

### Step 3: Exchange Code for Tokens

```bash
curl -X POST "https://localhost:7000/api/login/pkce/token" \
  -H "Content-Type: application/json" \
  -d '{
    "code": "eyJhbGciOiJSUzI1NiIsImtpZCI6...",
    "codeVerifier": "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk",
    "clientId": "mobile-client",
    "redirectUri": "com.socialnetwork.app://callback"
  }'
```

Use the returned `accessToken` for authenticated API requests.

## Security Considerations

1. **Code Verifier**: Must be securely stored on the client between steps 1 and 3. Never expose it in URLs or logs.

2. **Code Challenge**: Derived from the code verifier using SHA256 hash. Sent to the server during authentication.

3. **Authorization Code**: Short-lived and can only be used once. Should be exchanged for tokens immediately.

4. **HTTPS Required**: Always use HTTPS in production to protect against man-in-the-middle attacks.

5. **Client Types**: 
   - `mobile-client`: For mobile applications (no client secret required)
   - `web-client`: For web applications with server-side component
   - `swagger-client`: For testing via Swagger UI

## Supported Clients

The following clients are pre-configured for PKCE:

- **mobile-client**: No client secret required (public client)
- **web-client**: Requires client secret (confidential client)

## Error Responses

All error responses follow this format:

```json
{
  "success": false,
  "error": "error_code",
  "errorDescription": "Human-readable error description"
}
```

Common error codes:
- `invalid_request`: Missing or invalid request parameters
- `invalid_credentials`: Invalid username or password
- `authorization_request_failed`: Failed to generate authorization code
- `token_exchange_failed`: Failed to exchange code for tokens
- `internal_error`: Server error

## Implementation Details

### Code Verifier Generation
- Random 32-byte value
- Base64-URL encoded
- Length: 43-128 characters

### Code Challenge Generation
- SHA256 hash of code verifier
- Base64-URL encoded
- Challenge method: S256 (recommended)

### Flow Security
- Protects against authorization code interception attacks
- No client secret required for public clients
- Suitable for mobile apps and SPAs

## Testing with Postman

1. **Import Collection**: Create a new collection with the three endpoints above

2. **Environment Variables**:
   - `base_url`: https://localhost:7000
   - `code_verifier`: Save from Step 1
   - `code_challenge`: Save from Step 1
   - `authorization_code`: Save from Step 2
   - `access_token`: Save from Step 3

3. **Test Sequence**:
   ```
   GET {{base_url}}/api/login/pkce/generate
   → Save codeVerifier and codeChallenge
   
   POST {{base_url}}/api/login/pkce
   → Save authorizationCode
   
   POST {{base_url}}/api/login/pkce/token
   → Save accessToken
   ```

## Comparison with Password Flow

| Feature | Password Flow | PKCE Flow |
|---------|--------------|-----------|
| Security | Less secure | More secure |
| Client Type | Trusted | Public/Untrusted |
| Client Secret | Required | Not required |
| Use Case | Testing, trusted apps | Mobile, SPAs |
| Standard | OAuth 2.0 | OAuth 2.0 + PKCE (RFC 7636) |

## Related Documentation

- [OAuth 2.0 RFC 6749](https://tools.ietf.org/html/rfc6749)
- [PKCE RFC 7636](https://tools.ietf.org/html/rfc7636)
- [OpenIddict Documentation](https://documentation.openiddict.com/)
