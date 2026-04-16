import {
  HttpInterceptorFn,
  HttpErrorResponse,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

/**
 * HTTP Interceptor to handle 401 errors as a safety net fallback.
 * 
 * The main token refresh is now handled by OAuthService.setupAutomaticSilentRefresh(),
 * which proactively refreshes tokens before expiry.
 * 
 * This interceptor catches any 401 responses that slip through (edge cases)
 * and initiates logout to force re-authentication.
 */
export const refreshTokenInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  // Skip refresh for auth-related endpoints
  if (_shouldSkipRefresh(req)) {
    return next(req);
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        console.warn('Received 401 Unauthorized - initiating logout');
        authService.logout();
      }
      return throwError(() => error);
    })
  );
};

function _shouldSkipRefresh(req: any): boolean {
  // Skip refresh for auth-related endpoints
  const skipUrls = ['/connect/authorize', '/connect/token', '/connect/logout', '/signin-oidc'];
  return skipUrls.some((url) => req.url.includes(url));
}
