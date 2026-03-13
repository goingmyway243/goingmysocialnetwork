import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

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
        // Redirect to home page or requested page
        const returnUrl = sessionStorage.getItem('returnUrl') || '/dashboard';
        sessionStorage.removeItem('returnUrl');
        this.router.navigateByUrl(returnUrl);
      } else {
        console.error('Authentication failed');
        this.router.navigate(['/login'], {
          queryParams: { error: 'Authentication failed' }
        });
      }
    } catch (error) {
      console.error('Error during callback processing:', error);
      this.router.navigate(['/login'], {
        queryParams: { error: 'An error occurred during authentication' }
      });
    }
  }
}
