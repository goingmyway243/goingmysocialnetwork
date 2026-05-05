import { Injectable, inject } from '@angular/core';
import { OAuthService, AuthConfig, OAuthEvent } from 'angular-oauth2-oidc';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  // ── 1. Dependencies ─────────────────────────────────────────
  private readonly _oauthService = inject(OAuthService);

  // ── 2. Private State ─────────────────────────────────────────
  private _refreshSubscription: any = null;

  get oauthEvents(): Observable<OAuthEvent> {
    return this._oauthService.events;
  }

  // ── 3. Initialization ────────────────────────────────────────
  initAuth(): void {
    const authConfig: AuthConfig = environment.authConfig;
    this._oauthService.configure(authConfig);
    this._oauthService.loadDiscoveryDocumentAndTryLogin();

    // Setup automatic silent token refresh
    // The library will proactively refresh the token before expiry
    this._setupAutoRefresh();
  }

  private _setupAutoRefresh(): void {
    // Remove any existing subscription
    if (this._refreshSubscription) {
      this._refreshSubscription.unsubscribe();
    }

    // Configure automatic silent refresh
    // The library will proactively refresh the token before expiry (default: 30s margin)
    this._refreshSubscription = this._oauthService.setupAutomaticSilentRefresh();
  }

  // ── 4. Token Management ──────────────────────────────────────
  getAccessToken(): string {
    return this._oauthService.getAccessToken() || '';
  }

  isLoggedIn(): boolean {
    return this._oauthService.hasValidAccessToken();
  }

  getCurrentUserId(): string {
    const claims = this._oauthService.getIdentityClaims() as Record<string, unknown> | null;
    return (claims?.['sub'] as string) ?? '';
  }

  getCurrentUsername(): string {
    const claims = this._oauthService.getIdentityClaims() as Record<string, unknown> | null;
    return (claims?.['name'] as string) ?? '';
  }

  /**
   * Returns true if the current user has the given role claim.
   * Handles both single-value (string) and multi-value (string[]) role claims.
   */
  hasRole(role: string): boolean {
    const claims = this._oauthService.getIdentityClaims() as Record<string, unknown> | null;
    if (!claims) return false;
    const roles = claims['role'];
    if (Array.isArray(roles)) return (roles as string[]).includes(role);
    return roles === role;
  }

  /**
   * Manually refresh the access token (fallback mechanism).
   * Normally the library handles this automatically via setupAutomaticSilentRefresh().
   */
  refreshAccessToken(): Observable<any> {
    return new Observable((observer) => {
      this._oauthService.refreshToken()
        .then(() => {
          observer.next({ access_token: this.getAccessToken() });
          observer.complete();
        })
        .catch((error) => {
          observer.error(error);
        });
    });
  }

  // ── 5. Authentication Flow ───────────────────────────────────
  /**
   * Initiates the PKCE authorization code flow.
   * Redirects user to the authorization server login page.
   */
  login(targetUrl?: string): void {
    if (targetUrl) {
      sessionStorage.setItem('returnUrl', targetUrl);
    }
    
    // Check if we need to force re-authentication (after logout)
    const forceLogin = sessionStorage.getItem('forceLogin');
    if (forceLogin === 'true') {
      sessionStorage.removeItem('forceLogin');
      // Force re-authentication even if session exists
      this._oauthService.initCodeFlow('', { prompt: 'login' });
    } else {
      this._oauthService.initCodeFlow();
    }
  }

  /**
   * Handles the OAuth callback after authorization.
   * Exchanges authorization code for tokens (PKCE).
   */
  async handleAuthCallback(): Promise<boolean> {
    try {
      await this._oauthService.loadDiscoveryDocumentAndTryLogin();
      return this._oauthService.hasValidAccessToken();
    } catch (error) {
      console.error('Error during OIDC callback processing:', error);
      return false;
    }
  }

  /**
   * Logs out the user, clears tokens and server-side cookies, then restarts PKCE flow.
   */
  logout(): void {
    // Stop automatic silent refresh
    if (this._refreshSubscription) {
      this._refreshSubscription.unsubscribe();
      this._refreshSubscription = null;
    }

    // Clear local OAuth tokens first
    this._oauthService.logOut();
    sessionStorage.removeItem('returnUrl');
    
    // Set flag to force re-authentication on next login
    sessionStorage.setItem('forceLogin', 'true');
    
    // Redirect to server logout endpoint to clear the cookie
    // The server will redirect back to postLogoutRedirectUri (/)
    const logoutUrl = `${environment.authConfig.issuer}connect/logout`;
    window.location.href = logoutUrl;
  }
}
