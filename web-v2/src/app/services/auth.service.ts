import { Injectable, signal } from '@angular/core';
import { OAuthService, AuthConfig } from 'angular-oauth2-oidc';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  constructor(private oauthService: OAuthService) {}

  initAuth(): void {
    const authConfig: AuthConfig = environment.authConfig;
    this.oauthService.configure(authConfig);
    this.oauthService.loadDiscoveryDocument();
    this.oauthService.tryLoginImplicitFlow();
  }

  getAccessToken(): string {
    return this.oauthService.getAccessToken();
  }

  isLoggedIn(): boolean {
    return this.oauthService.hasValidAccessToken();
  }

  login(username: string, password: string): void {
    const params = {
      username,
      password
    };

    this.oauthService.initCodeFlow('', params);
  }

  async handleAuthCallback(): Promise<boolean> {
    await this.oauthService.tryLoginCodeFlow();

    if (this.oauthService.hasValidAccessToken()) {
      return true;
    }

    return false;
  }
}
