import { Component, OnInit, inject } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  // ── 1. Dependencies ─────────────────────────────────────────
  private readonly _authService = inject(AuthService);
  private readonly _router = inject(Router);

  // ── 4. Lifecycle ─────────────────────────────────────────────
  ngOnInit(): void {
    // Check if this is the OAuth callback route
    const isCallback = window.location.pathname.includes('signin-oidc');
    
    // If not a callback and user is not logged in, start PKCE flow
    if (!isCallback && !this._authService.isLoggedIn()) {
      console.log('User not authenticated - initiating PKCE flow');
      this._authService.login('/dashboard');
    }
  }
}
