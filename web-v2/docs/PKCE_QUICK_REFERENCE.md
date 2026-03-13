# PKCE Flow - Quick Reference

## Backend Changes

### 1. Update Database Schema
The Identity service needs to recreate the database with the new PKCE configuration:

```powershell
# Option 1: Delete existing database (Development only!)
cd microservices/SocialNetworkMicroservices.Identity
# Delete the database through your PostgreSQL client or:
# DROP DATABASE IF EXISTS "goingmysocial-identity";

# Option 2: Clear OpenIddict tables
# DELETE FROM OpenIddictApplications;
# DELETE FROM OpenIddictAuthorizations;
# DELETE FROM OpenIddictScopes;
# DELETE FROM OpenIddictTokens;

# Then restart the service
dotnet run
```

The seeder will automatically create the new `web-client` configuration with PKCE.

### 2. Verify Configuration
Check that `web-client` has PKCE enabled:
- No `ClientSecret` defined (public client)
- `Requirements.Features.ProofKeyForCodeExchange` is set
- Redirect URIs include `http://localhost:4200/signin-oidc`

## Frontend Changes

### 1. Install Dependencies (if needed)
```powershell
cd web-v2
npm install
```

The `angular-oauth2-oidc` library should already be in `package.json`.

### 2. Run the App
```powershell
npm start
# App runs on http://localhost:4200
```

## Testing Checklist

- [ ] Identity Service running on `http://localhost:5133`
- [ ] Database seeded with new client configuration
- [ ] Angular app running on `http://localhost:4200`
- [ ] Can navigate to `/login`
- [ ] Can enter credentials and submit
- [ ] Gets redirected to `/connect/authorize`
- [ ] Gets redirected back to `/signin-oidc`
- [ ] Tokens are obtained (check browser console)
- [ ] User is authenticated (check `isAuthenticated` signal)

## Files Modified

### Identity Service
- ✅ `Data/OpenIddictSeeder.cs` - Added PKCE requirement, openid scope

### Angular App
- ✅ `src/environments/environment.ts` - OAuth config
- ✅ `src/environments/environment.prod.ts` - OAuth config
- ✅ `src/app/services/auth.service.ts` - Complete rewrite with angular-oauth2-oidc
- ✅ `src/app/configs/app.config.ts` - Added APP_INITIALIZER
- ✅ `src/app/pages/login/login.component.ts` - Updated to use new auth service
- ✅ `src/app/pages/callback/callback.component.ts` - New OAuth callback handler
- ✅ `src/app/app.routes.ts` - Added `/signin-oidc` route
- ✅ `src/app/guards/auth.guard.ts` - Updated for return URL handling
- ✅ `src/app/interceptors/auth.interceptor.ts` - Updated to skip OAuth endpoints

## Common Commands

### Start Identity Service
```powershell
cd microservices/SocialNetworkMicroservices.Identity
dotnet run
```

### Start Angular App
```powershell
cd web-v2
npm start
```

### View Logs
- **Backend**: Console output from `dotnet run`
- **Frontend**: Browser DevTools Console (F12)

## Test Credentials

| Username | Password | Roles |
|----------|----------|-------|
| admin | admin123 | Admin, User |
| user | user123 | User |

## Key URLs

| Service | URL |
|---------|-----|
| Angular App | http://localhost:4200 |
| Identity Server | http://localhost:5133 |
| Authorization Endpoint | http://localhost:5133/connect/authorize |
| Token Endpoint | http://localhost:5133/connect/token |
| UserInfo Endpoint | http://localhost:5133/connect/userinfo |

## OAuth2 Flow Summary

```
Login Form (Angular)
  ↓ credentials
Generate PKCE Challenge
  ↓ redirect with challenge + credentials
/connect/authorize (Identity Server)
  ↓ validates & generates auth code
Redirect to /signin-oidc (Angular)
  ↓ auth code
Callback Component
  ↓ exchange code + verifier
/connect/token (Identity Server)
  ↓ validates PKCE
Access Token + ID Token
  ↓
Authenticated User
```

## Debugging Tips

### Enable OAuth Debug Logs
Already enabled in `environment.ts`:
```typescript
showDebugInformation: true
```

Check console for:
- `[OAuth] token_received`
- `[OAuth] discovery_document_loaded`
- `[OAuth] code_flow_completed`

### Check Token
```javascript
// In browser console
localStorage.getItem('access_token_stored_at')
localStorage.getItem('id_token_stored_at')
sessionStorage.getItem('pkce_code_verifier')
```

### Common Issues

1. **"discovery_document_load_error"**
   - Identity Server not running
   - Wrong issuer URL

2. **"invalid_grant" error**
   - PKCE verification failed
   - Code verifier mismatch
   - Authorization code expired

3. **"invalid_redirect_uri"**
   - Redirect URI mismatch
   - URI not registered in OpenIddict

4. **CORS errors**
   - Identity Server needs CORS configured
   - Already configured with `AllowAnyOrigin()`

## Next Steps

1. Test the login flow end-to-end
2. Add a home page/dashboard
3. Add logout button
4. Implement token refresh
5. Add protected routes with `authGuard`

## Documentation

- Full Guide: `PKCE_SETUP_GUIDE.md`
- Backend PKCE Docs: `microservices/SocialNetworkMicroservices.Identity/README_PKCE.md`
- Frontend Auth Docs: `src/app/AUTH_README.md`
