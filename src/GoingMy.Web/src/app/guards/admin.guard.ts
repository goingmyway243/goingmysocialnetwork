import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Guard for admin-only routes.
 * Redirects unauthenticated users to OIDC login, and non-admin users to the main dashboard.
 */
export const adminGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isLoggedIn()) {
    authService.login(state.url);
    return false;
  }

  if (!authService.hasRole('Admin')) {
    return router.createUrlTree(['/dashboard']);
  }

  return true;
};
