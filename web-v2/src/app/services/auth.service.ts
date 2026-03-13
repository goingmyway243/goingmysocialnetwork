import { Injectable, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { OAuthService, AuthConfig } from 'angular-oauth2-oidc';
import { filter } from 'rxjs/operators';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private isAuthenticatedSignal = signal<boolean>(false);
  private userProfileSignal = signal<any>(null);

  // Public computed signals
  public readonly isAuthenticated = computed(() => this.isAuthenticatedSignal());
  public readonly currentUser = computed(() => this.userProfileSignal());
  public readonly accessToken = computed(() => this.oauthService.getAccessToken());

  constructor(
    private oauthService: OAuthService,
    private router: Router
  ) {
    this.configureOAuth();
  }

  /**
   * Configure OAuth2/OIDC with PKCE
   */
  private configureOAuth(): void {
    const authConfig: AuthConfig = {
      issuer: environment.auth.issuer,
      redirectUri: environment.auth.redirectUri,
      clientId: environment.auth.clientId,
      responseType: environment.auth.responseType,
      scope: environment.auth.scope,
      showDebugInformation: environment.auth.showDebugInformation,
      requireHttps: environment.auth.requireHttps,
      
      // PKCE configuration
      useSilentRefresh: environment.auth.useSilentRefresh,
      silentRefreshRedirectUri: environment.auth.silentRefreshRedirectUri,
      
      // Disable PKCE for now since we're using a custom login page
      // We'll use the authorization code flow with the login page
      disablePKCE: false,
      
      // OpenIddict endpoints
      loginUrl: `${environment.auth.issuer}/connect/authorize`,
      tokenEndpoint: `${environment.auth.issuer}/connect/token`,
      userinfoEndpoint: `${environment.auth.issuer}/connect/userinfo`,
      logoutUrl: `${environment.auth.issuer}/connect/logout`,
      
      // Custom query parameters for login endpoint
      customQueryParams: {},
      
      // Skip issuer check for development
      strictDiscoveryDocumentValidation: false,
      skipIssuerCheck: !environment.production,
    };

    this.oauthService.configure(authConfig);
    
    // Automatically load user profile after successful login
    this.oauthService.events
      .pipe(filter((e: any) => e.type === 'token_received'))
      .subscribe(() => {
        this.loadUserProfile();
      });

    // Update authentication state on token events
    this.oauthService.events.subscribe((event: any) => {
      if (event.type === 'token_received' || event.type === 'token_refreshed') {
        this.isAuthenticatedSignal.set(true);
      } else if (event.type === 'logout' || event.type === 'session_terminated') {
        this.isAuthenticatedSignal.set(false);
        this.userProfileSignal.set(null);
      }
    });
  }

  /**
   * Initialize authentication - must be called on app startup
   */
  async initAuth(): Promise<boolean> {
    try {
      // Try to load discovery document
      await this.oauthService.loadDiscoveryDocument();
      
      // Try to silently refresh the token if we have one
      await this.oauthService.tryLoginImplicitFlow();
      
      // Check if we have a valid token
      if (this.oauthService.hasValidAccessToken()) {
        await this.loadUserProfile();
        this.isAuthenticatedSignal.set(true);
        return true;
      }
      
      return false;
    } catch (error) {
      console.error('Error during authentication initialization:', error);
      return false;
    }
  }

  /**
   * Login with username and password using custom login page
   * This initiates the OAuth2 Authorization Code Flow with PKCE
   */
  loginWithCredentials(username: string, password: string): void {
    // Store credentials to pass as custom parameters
    const customParams = {
      username: username,
      password: password
    };
    
    // Use the library's initCodeFlow which handles state and PKCE automatically
    // The second parameter can be used for additional state
    // Custom query parameters will be appended to the authorization URL
    this.oauthService.initCodeFlow('', customParams);
  }

  /**
   * Handle OAuth callback
   */
  async handleCallback(): Promise<boolean> {
    try {
      // Try to complete the code flow (this validates state and exchanges code for tokens)
      await this.oauthService.tryLoginCodeFlow();
      
      if (this.oauthService.hasValidAccessToken()) {
        await this.loadUserProfile();
        this.isAuthenticatedSignal.set(true);
        return true;
      }
      
      return false;
    } catch (error) {
      console.error('Error handling OAuth callback:', error);
      return false;
    }
  }

  /**
   * Logout the current user
   */
  logout(): void {
    this.oauthService.revokeTokenAndLogout();
    this.isAuthenticatedSignal.set(false);
    this.userProfileSignal.set(null);
    this.router.navigate(['/login']);
  }

  /**
   * Load user profile from userinfo endpoint
   */
  private async loadUserProfile(): Promise<void> {
    try {
      const claims = this.oauthService.getIdentityClaims();
      this.userProfileSignal.set(claims);
    } catch (error) {
      console.error('Error loading user profile:', error);
    }
  }

  /**
   * Get the current access token
   */
  getAccessToken(): string | null {
    return this.oauthService.getAccessToken();
  }

  /**
   * Check if user is authenticated
   */
  isLoggedIn(): boolean {
    return this.oauthService.hasValidAccessToken();
  }

  /**
   * Get current user info
   */
  getCurrentUser(): any {
    return this.userProfileSignal();
  }
}
