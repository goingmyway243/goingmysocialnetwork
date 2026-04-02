import { Component, OnDestroy, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'auth-callback',
  standalone: true,
  templateUrl: './auth-callback.component.html',
  styleUrls: ['./auth-callback.component.css']
})
export class AuthCallbackComponent implements OnDestroy {
  // ── 1. Dependencies ─────────────────────────────────────────
  private readonly _authService = inject(AuthService);
  private readonly _router = inject(Router);

  // ── 2. State ────────────────────────────────────────────────
  readonly isProcessing = signal(true);
  readonly error = signal<string | null>(null);

  // ── 4. Lifecycle ─────────────────────────────────────────────
  constructor() {
    this.handleCallback();
  }

  ngOnDestroy(): void {
    // Cleanup if needed
  }

  // ── 5. Actions ───────────────────────────────────────────────
  private async handleCallback(): Promise<void> {
    try {
      const success = await this._authService.handleAuthCallback();
      console.log('PKCE callback processed, success:', success);
      
      if (success) {
        const returnUrl = sessionStorage.getItem('returnUrl') || '/dashboard';
        sessionStorage.removeItem('returnUrl');
        await this._router.navigateByUrl(returnUrl);
      } else {
        this.error.set('Authentication failed - please try again');
        await this._router.navigateByUrl('/');
      }
    } catch (error) {
      console.error('Error during PKCE callback processing:', error);
      this.error.set('Error processing authentication callback');
      await this._router.navigateByUrl('/');
    } finally {
      this.isProcessing.set(false);
    }
  }
}