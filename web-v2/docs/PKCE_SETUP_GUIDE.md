# PKCE Login Flow Configuration Guide

## Overview

This guide explains the PKCE (Proof Key for Code Exchange) OAuth2/OIDC login flow configuration between your **Angular web-v2** application and the **OpenIddict Identity Service**.

## What Changed

### Identity Service (Backend)

#### 1. OpenIddict Client Configuration
**File**: `microservices/SocialNetworkMicroservices.Identity/Data/OpenIddictSeeder.cs`

- Removed `ClientSecret` from `web-client` (making it a public client)
- Added PKCE requirement: `Requirements.Features.ProofKeyForCodeExchange`
- Added `openid` scope to permissions
- Added additional redirect URIs for flexibility

#### 2. OpenID Scope
Added the standard `openid` scope required for OIDC flows.

### Angular Application (Frontend)

#### 1. Environment Configuration
**Files**: 
- `src/environments/environment.ts` (development)
- `src/environments/environment.prod.ts` (production)

Configured OAuth2/OIDC settings:
```typescript
auth: {
  issuer: 'http://localhost:5133',
  clientId: 'web-client',
  redirectUri: window.location.origin + '/signin-oidc',
  postLogoutRedirectUri: window.location.origin + '/',
  responseType: 'code',
  scope: 'openid profile email roles',
  showDebugInformation: true,
  requireHttps: false,
  useSilentRefresh: false,
}
```

#### 2. Auth Service
**File**: `src/app/services/auth.service.ts`

Completely rewritten to use `angular-oauth2-oidc` library with:
- Automatic PKCE support
- OAuth2 Authorization Code Flow
- Token management via the library
- Discovery document loading
- User profile management

Key methods:
- `initAuth()`: Initialize authentication on app startup
- `loginWithCredentials(username, password)`: Start OAuth flow with credentials
- `handleCallback()`: Process OAuth redirect
- `logout()`: Revoke tokens and logout
- `isLoggedIn()`: Check authentication status

#### 3. Application Configuration
**File**: `src/app/configs/app.config.ts`

- Added `APP_INITIALIZER` to initialize auth on app startup
- Configured `provideOAuthClient` with resource server settings

#### 4. OAuth Callback Component
**File**: `src/app/pages/callback/callback.component.ts`

New component to handle OAuth redirects after authentication:
- Processes authorization code
- Exchanges code for tokens
- Redirects to intended page or home

#### 5. Routes
**File**: `src/app/app.routes.ts`

Added callback route:
```typescript
{
  path: 'signin-oidc',
  loadComponent: () => import('./pages/callback/callback.component').then(m => m.CallbackComponent)
}
```

#### 6. Auth Guard Updates
**File**: `src/app/guards/auth.guard.ts`

- Saves return URL in sessionStorage for post-login redirect
- Works seamlessly with OAuth flow

#### 7. Auth Interceptor Updates
**File**: `src/app/interceptors/auth.interceptor.ts`

- Updated to skip OAuth endpoints (`/connect/`)
- Automatically adds Bearer token to API requests

## How It Works

### Login Flow with PKCE

```
1. User enters credentials in login form
   ↓
2. App generates code_verifier (random string)
   ↓
3. App generates code_challenge (SHA256 hash of verifier)
   ↓
4. App redirects to /connect/authorize with:
   - client_id: web-client
   - redirect_uri: http://localhost:4200/signin-oidc
   - response_type: code
   - scope: openid profile email roles
   - code_challenge: [generated hash]
   - code_challenge_method: S256
   - username & password (in query params)
   ↓
5. Identity server validates credentials
   ↓
6. Identity server generates authorization code
   ↓
7. Identity server redirects to callback URL with code
   ↓
8. Callback component receives authorization code
   ↓
9. angular-oauth2-oidc library exchanges code + verifier for tokens
   ↓
10. App stores tokens and user is authenticated
```

## Testing the Flow

### Prerequisites

1. **Identity Service Running**
   ```powershell
   cd microservices/SocialNetworkMicroservices.Identity
   dotnet run
   ```
   Service should be running on `http://localhost:5133`

2. **Database Initialized**
   The OpenIddict seeder will automatically create the database and seed clients on startup.

### Test Credentials

Using the `TestUserService`, you can test with:
- **Username**: `admin` or `admin@test.com`
- **Password**: `admin123`

Or:
- **Username**: `user` or `user@test.com`
- **Password**: `user123`

### Running the Angular App

```powershell
cd web-v2
npm install  # if not already done
npm start
```

Navigate to: `http://localhost:4200`

### Testing Steps

1. Open `http://localhost:4200` - should redirect to `/login`
2. Enter credentials (e.g., `admin` / `admin123`)
3. Click "Sign In"
4. App will redirect to Identity Server's authorize endpoint
5. Identity Server validates credentials and redirects back to `/signin-oidc`
6. Callback component processes the authorization code
7. Tokens are obtained and stored
8. User is redirected to home page

### Debugging

Enable debug information in development environment:
```typescript
// environment.ts
showDebugInformation: true
```

This will log OAuth events to the browser console:
- Token received
- Token refreshed
- Discovery document loaded
- Errors

Check browser console for:
```
[OAuth] token_received
[OAuth] discovery_document_loaded
```

## Security Features

### PKCE Protection

- **Code Verifier**: Random 32-byte value (Base64-URL encoded)
- **Code Challenge**: SHA-256 hash of verifier (Base64-URL encoded)
- **Challenge Method**: S256 (most secure)

### Benefits

1. **Prevents authorization code interception attacks**
2. **No client secret needed** (safe for SPAs)
3. **Industry standard** for public clients
4. **OpenID Connect compliant**

## Production Considerations

### 1. HTTPS Required

In production, update `environment.prod.ts`:
```typescript
requireHttps: true,
```

### 2. Proper Redirect URIs

Update in `OpenIddictSeeder.cs`:
```csharp
RedirectUris =
{
    new Uri("https://yourdomain.com/signin-oidc")
}
```

And in `environment.prod.ts`:
```typescript
issuer: 'https://identity.yourdomain.com',
redirectUri: 'https://yourdomain.com/signin-oidc',
```

### 3. Silent Token Refresh

Enable in production:
```typescript
useSilentRefresh: true,
silentRefreshRedirectUri: window.location.origin + '/silent-refresh.html',
```

Create `public/silent-refresh.html`:
```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <title>Silent Refresh</title>
</head>
<body>
    <script>
        parent.postMessage(location.hash, location.origin);
    </script>
</body>
</html>
```

### 4. Token Storage

Tokens are stored by `angular-oauth2-oidc` in sessionStorage by default. For better security, consider:
- Using secure cookies (requires backend support)
- Implementing token encryption
- Setting appropriate token lifetimes

## Troubleshooting

### Issue: "Invalid redirect_uri"

**Solution**: Ensure redirect URI in `OpenIddictSeeder.cs` exactly matches the one in `environment.ts`.

### Issue: "PKCE required"

**Solution**: Verify `Requirements.Features.ProofKeyForCodeExchange` is set in `OpenIddictSeeder.cs`.

### Issue: "Discovery document failed to load"

**Solution**: 
1. Check Identity Server is running
2. Verify issuer URL is correct
3. Check CORS settings in Identity Server

### Issue: "Token request failed"

**Solution**:
1. Check browser console for errors
2. Verify code_verifier is being stored/retrieved correctly
3. Check Identity Server logs

## API Integration

After successful authentication, the access token is automatically added to API requests by the auth interceptor.

### Making Authenticated API Calls

```typescript
import { HttpClient } from '@angular/common/http';
import { environment } from '../environments/environment';

@Injectable()
export class DataService {
  constructor(private http: HttpClient) {}

  getData() {
    // Token is automatically added by authInterceptor
    return this.http.get(`${environment.apiUrl}/data`);
  }
}
```

### Checking Authentication Status

```typescript
import { AuthService } from './services/auth.service';

@Component({...})
export class MyComponent {
  constructor(public authService: AuthService) {}

  get isAuthenticated() {
    return this.authService.isAuthenticated();
  }

  get currentUser() {
    return this.authService.currentUser();
  }
}
```

## Next Steps

1. **Add logout functionality** to UI
2. **Create protected pages** with `authGuard`
3. **Implement token refresh** handling
4. **Add user profile page**
5. **Configure silent token refresh** for better UX

## References

- [OAuth 2.0 PKCE RFC 7636](https://tools.ietf.org/html/rfc7636)
- [OpenID Connect Core 1.0](https://openid.net/specs/openid-connect-core-1_0.html)
- [angular-oauth2-oidc Documentation](https://github.com/manfredsteyer/angular-oauth2-oidc)
- [OpenIddict Documentation](https://documentation.openiddict.com/)
