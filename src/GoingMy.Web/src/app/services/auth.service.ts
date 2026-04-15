import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { OAuthService, AuthConfig, OAuthEvent } from 'angular-oauth2-oidc';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  // ── 1. Dependencies ─────────────────────────────────────────
  private readonly _oauthService = inject(OAuthService);
  private readonly _http = inject(HttpClient);
  private readonly _router = inject(Router);

  get oauthEvents(): Observable<OAuthEvent> {
    return this._oauthService.events;
  }

  // ── 2. Initialization ────────────────────────────────────────
  initAuth(): void {
    const authConfig: AuthConfig = environment.authConfig;
    this._oauthService.configure(authConfig);
    this._oauthService.loadDiscoveryDocumentAndTryLogin();
  }

  // ── 3. Token Management ──────────────────────────────────────
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

  // ── 4. Authentication Flow ───────────────────────────────────
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
