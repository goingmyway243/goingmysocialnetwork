import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'auth-callback',
  standalone: true,
  templateUrl: './auth-callback.component.html',
  styleUrls: ['./auth-callback.component.css']
})
export class AuthCallbackComponent implements OnInit {
  constructor(
    private authService: AuthService,
    private router: Router
  ) { }

  async ngOnInit() {
    try {
      const success = await this.authService.handleAuthCallback();
      if (success) {
        const returnUrl = sessionStorage.getItem('returnUrl') || '/dashboard';
        sessionStorage.removeItem('returnUrl');
        this.router.navigateByUrl(returnUrl);
      } else {
        // No token found — redirect back to Blazor login
        window.location.href = environment.blazorLoginUrl;
      }
    } catch (error) {
      console.error('Error during callback processing:', error);
      window.location.href = environment.blazorLoginUrl;
    }
  }
}