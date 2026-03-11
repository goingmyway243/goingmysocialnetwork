# Authentication Service

This directory contains the authentication service and related components for the Social Network application.

## Overview

The authentication system integrates with the Identity microservice to provide OAuth 2.0/OpenID Connect authentication.

## Files Created

### Models
- **`models/auth.models.ts`**: TypeScript interfaces for authentication
  - `LoginRequest`: Login credentials
  - `LoginResponse`: Response from login API
  - `UserInfo`: User information
  - `AuthState`: Application authentication state

### Services
- **`services/auth.service.ts`**: Main authentication service
  - Login functionality
  - Token management
  - Local storage persistence
  - Reactive state management using Angular signals

### Interceptors
- **`interceptors/auth.interceptor.ts`**: HTTP interceptor
  - Automatically adds Bearer token to API requests
  - Skips authentication for login/token endpoints

### Environments
- **`environments/environment.ts`**: Development configuration
- **`environments/environment.prod.ts`**: Production configuration

## Usage

### Login Component Integration

The login component has been updated to use the auth service:

```typescript
this.authService.login(email, password).subscribe({
  next: (response) => {
    if (response.success) {
      // Handle successful login
      this.router.navigate(['/']);
    } else {
      // Handle login failure
      this.errorMessage.set(response.errorDescription);
    }
  },
  error: (error) => {
    // Handle error
    this.errorMessage.set(error.message);
  }
});
```

### Using Auth Service in Components

```typescript
import { AuthService } from './services/auth.service';

export class YourComponent {
  constructor(private authService: AuthService) {}

  // Check if user is authenticated
  get isAuthenticated() {
    return this.authService.isAuthenticated();
  }

  // Get current user
  get currentUser() {
    return this.authService.currentUser();
  }

  // Logout
  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
```

### Protected Routes (Guard)

To protect routes, create an auth guard:

```typescript
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './services/auth.service';

export const authGuard = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isLoggedIn()) {
    return true;
  }

  return router.createUrlTree(['/login']);
};
```

## API Configuration

The API URL is configured in the environment files:

```typescript
// environment.ts (Development)
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api',
  identityUrl: 'http://localhost:5000'
};
```

## Identity Service Endpoint

The auth service connects to the following Identity microservice endpoint:

- **Endpoint**: `POST /api/login`
- **Request Body**:
  ```json
  {
    "username": "user@example.com",
    "password": "password123"
  }
  ```
- **Response**:
  ```json
  {
    "success": true,
    "accessToken": "eyJhbGc...",
    "tokenType": "Bearer",
    "expiresIn": 3600,
    "refreshToken": "refresh_token_here",
    "user": {
      "id": "user-id",
      "username": "user@example.com",
      "email": "user@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "roles": ["user"]
    }
  }
  ```

## Features

- ✅ Login with email/password
- ✅ JWT token management
- ✅ Persistent authentication (localStorage)
- ✅ Automatic token injection via HTTP interceptor
- ✅ Reactive state management with Angular signals
- ✅ Error handling
- ✅ SSR-safe (checks for window/localStorage availability)

## Testing

Use these test credentials (if using TestUserService):
- **Email**: `admin@test.com`
- **Password**: `admin123`

or

- **Email**: `user@test.com`
- **Password**: `user123`

## Next Steps

1. Create an auth guard for protected routes
2. Implement token refresh functionality
3. Add logout endpoint integration
4. Create user profile management
5. Add password reset functionality
