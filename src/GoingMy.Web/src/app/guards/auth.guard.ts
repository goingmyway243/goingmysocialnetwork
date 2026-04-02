import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Auth guard to protect routes that require authentication.
 * Initiates PKCE OAuth flow for unauthenticated users.
 */
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);

  if (authService.isLoggedIn()) {
    return true;
  }

  // Initiate PKCE OAuth flow, passing the target path
  authService.login(state.url);
  return false;
};
