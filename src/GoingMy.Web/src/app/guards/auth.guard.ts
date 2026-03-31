import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Auth guard to protect routes that require authentication.
 * Redirects unauthenticated users to the Blazor login page.
 */
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);

  if (authService.isLoggedIn()) {
    return true;
  }

  // Redirect to Blazor login page (external), passing the target path as returnUrl
  window.location.href = authService.getBlazorLoginUrl(state.url);
  return false;
};

/**
 * Guest guard to prevent authenticated users from accessing login/register pages.
 */
export const guestGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isLoggedIn()) {
    return true;
  }

  // Already authenticated — send to dashboard
  return router.createUrlTree(['/dashboard']);
};
