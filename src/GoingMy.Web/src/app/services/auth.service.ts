import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { OAuthService, AuthConfig, OAuthEvent } from 'angular-oauth2-oidc';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

/** Key names used by angular-oauth2-oidc for token storage */
const TOKEN_KEY = 'access_token';
const TOKEN_STORED_AT_KEY = 'access_token_stored_at';
const TOKEN_EXPIRATION_KEY = 'access_token_expiration';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  constructor(
    private oauthService: OAuthService,
    private http: HttpClient,
    private router: Router
  ) {}

  get oauthEvents(): Observable<OAuthEvent> {
    return this.oauthService.events;
  }

  initAuth(): void {
    const authConfig: AuthConfig = environment.authConfig;
    this.oauthService.configure(authConfig);
    this.oauthService.loadDiscoveryDocumentAndTryLogin();
  }

  getAccessToken(): string {
    // Check oauthService first, fall back to sessionStorage (tokens stored by Blazor callback)
    return this.oauthService.getAccessToken() || sessionStorage.getItem(TOKEN_KEY) || '';
  }

  isLoggedIn(): boolean {
    if (this.oauthService.hasValidAccessToken()) {
      return true;
    }
    // Also check token stored directly from Blazor login redirect
    const token = sessionStorage.getItem(TOKEN_KEY);
    const expiration = sessionStorage.getItem(TOKEN_EXPIRATION_KEY);
    if (token && expiration) {
      return Date.now() < parseInt(expiration, 10);
    }
    return false;
  }

  /**
   * Stores a JWT token received from the Blazor login callback in sessionStorage
   * using the same keys that angular-oauth2-oidc reads.
   */
  storeExternalToken(accessToken: string, expiresIn: number): void {
    const now = Date.now();
    sessionStorage.setItem(TOKEN_KEY, accessToken);
    sessionStorage.setItem(TOKEN_STORED_AT_KEY, now.toString());
    sessionStorage.setItem(TOKEN_EXPIRATION_KEY, (now + expiresIn * 1000).toString());
  }

  /**
   * Reads access_token and expires_in from the current URL query params
   * (placed there by the Blazor login page after password flow).
   * Returns true if a token was found and stored.
   */
  handleExternalTokenCallback(): boolean {
    const params = new URLSearchParams(window.location.search);
    const accessToken = params.get('access_token');
    const expiresIn = parseInt(params.get('expires_in') ?? '900', 10);

    if (accessToken) {
      this.storeExternalToken(accessToken, expiresIn);
      // Clean up token from URL without reloading
      const cleanUrl = window.location.pathname;
      window.history.replaceState({}, '', cleanUrl);
      return true;
    }
    return false;
  }

  /**
   * @deprecated Login is now handled by the Blazor login page.
   * Redirects to the Blazor login URL.
   */
  login(_username?: string, _password?: string): void {
    window.location.href = this.getBlazorLoginUrl();
  }

  /**
   * Builds the Blazor login URL with the Angular signin-oidc callback as returnUrl.
   */
  getBlazorLoginUrl(returnPath?: string): string {
    const callbackUrl = `${window.location.origin}/signin-oidc`;
    const params = new URLSearchParams({ returnUrl: callbackUrl });
    if (returnPath) {
      sessionStorage.setItem('returnUrl', returnPath);
    }
    return `${environment.blazorLoginUrl}?${params.toString()}`;
  }

  async handleAuthCallback(): Promise<boolean> {
    // First, try the Blazor external token callback (token in URL query params)
    if (this.handleExternalTokenCallback()) {
      return true;
    }

    // Fall back to OIDC code flow exchange
    try {
      await this.oauthService.tryLoginCodeFlow();
    } catch (error) {
      console.log('Error during OIDC login callback processing:', error);
      return false;
    }

    return this.oauthService.hasValidAccessToken();
  }

  logout(): void {
    this.oauthService.logOut();
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(TOKEN_STORED_AT_KEY);
    sessionStorage.removeItem(TOKEN_EXPIRATION_KEY);
    // Redirect to Blazor logout / login
    window.location.href = environment.blazorLoginUrl;
  }
}
