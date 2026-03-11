import { Injectable, signal, computed } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, BehaviorSubject, throwError } from 'rxjs';
import { tap, catchError, map } from 'rxjs/operators';
import { LoginRequest, LoginResponse, AuthState, UserInfo } from '../models/auth.models';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly API_URL = environment.apiUrl;
  private readonly TOKEN_KEY = 'access_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';
  private readonly USER_KEY = 'user_info';

  // Using signals for reactive state management
  private authStateSignal = signal<AuthState>({
    isAuthenticated: false,
    user: null,
    accessToken: null,
    refreshToken: null
  });

  // Public computed signals
  public readonly isAuthenticated = computed(() => this.authStateSignal().isAuthenticated);
  public readonly currentUser = computed(() => this.authStateSignal().user);
  public readonly accessToken = computed(() => this.authStateSignal().accessToken);

  constructor(private http: HttpClient) {
    this.loadAuthStateFromStorage();
  }

  /**
   * Login with username and password
   */
  login(username: string, password: string): Observable<LoginResponse> {
    const request: LoginRequest = { username, password };

    return this.http.post<LoginResponse>(`${this.API_URL}/login`, request).pipe(
      tap(response => {
        if (response.success && response.accessToken) {
          this.handleSuccessfulLogin(response);
        }
      }),
      catchError(this.handleError)
    );
  }

  /**
   * Logout the current user
   */
  logout(): void {
    this.clearAuthState();
    this.authStateSignal.set({
      isAuthenticated: false,
      user: null,
      accessToken: null,
      refreshToken: null
    });
  }

  /**
   * Get the current access token
   */
  getAccessToken(): string | null {
    return this.authStateSignal().accessToken;
  }

  /**
   * Get the current refresh token
   */
  getRefreshToken(): string | null {
    return this.authStateSignal().refreshToken;
  }

  /**
   * Check if user is authenticated
   */
  isLoggedIn(): boolean {
    return this.authStateSignal().isAuthenticated;
  }

  /**
   * Get current user info
   */
  getCurrentUser(): UserInfo | null {
    return this.authStateSignal().user;
  }

  /**
   * Handle successful login response
   */
  private handleSuccessfulLogin(response: LoginResponse): void {
    if (response.accessToken) {
      this.saveToStorage(this.TOKEN_KEY, response.accessToken);
    }
    if (response.refreshToken) {
      this.saveToStorage(this.REFRESH_TOKEN_KEY, response.refreshToken);
    }
    if (response.user) {
      this.saveToStorage(this.USER_KEY, JSON.stringify(response.user));
    }

    this.authStateSignal.set({
      isAuthenticated: true,
      user: response.user || null,
      accessToken: response.accessToken || null,
      refreshToken: response.refreshToken || null
    });
  }

  /**
   * Load authentication state from local storage
   */
  private loadAuthStateFromStorage(): void {
    const token = this.getFromStorage(this.TOKEN_KEY);
    const refreshToken = this.getFromStorage(this.REFRESH_TOKEN_KEY);
    const userJson = this.getFromStorage(this.USER_KEY);

    if (token) {
      let user: UserInfo | null = null;
      if (userJson) {
        try {
          user = JSON.parse(userJson);
        } catch (e) {
          console.error('Failed to parse user info from storage', e);
        }
      }

      this.authStateSignal.set({
        isAuthenticated: true,
        user,
        accessToken: token,
        refreshToken
      });
    }
  }

  /**
   * Clear authentication state from storage
   */
  private clearAuthState(): void {
    this.removeFromStorage(this.TOKEN_KEY);
    this.removeFromStorage(this.REFRESH_TOKEN_KEY);
    this.removeFromStorage(this.USER_KEY);
  }

  /**
   * Save data to local storage
   */
  private saveToStorage(key: string, value: string): void {
    if (typeof window !== 'undefined' && window.localStorage) {
      localStorage.setItem(key, value);
    }
  }

  /**
   * Get data from local storage
   */
  private getFromStorage(key: string): string | null {
    if (typeof window !== 'undefined' && window.localStorage) {
      return localStorage.getItem(key);
    }
    return null;
  }

  /**
   * Remove data from local storage
   */
  private removeFromStorage(key: string): void {
    if (typeof window !== 'undefined' && window.localStorage) {
      localStorage.removeItem(key);
    }
  }

  /**
   * Handle HTTP errors
   */
  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unknown error occurred';

    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Server-side error
      if (error.error?.errorDescription) {
        errorMessage = error.error.errorDescription;
      } else if (error.error?.error) {
        errorMessage = error.error.error;
      } else if (error.message) {
        errorMessage = error.message;
      } else {
        errorMessage = `Server returned code ${error.status}`;
      }
    }

    console.error('Authentication error:', errorMessage);
    return throwError(() => new Error(errorMessage));
  }
}
