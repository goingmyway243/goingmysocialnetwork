import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-callback',
  standalone: true,
  template: `
    <div class="flex items-center justify-center min-h-screen">
      <div class="text-center">
        <h2 class="text-2xl font-semibold mb-4">Processing login...</h2>
        <p class="text-gray-600">Please wait while we complete your authentication.</p>
      </div>
    </div>
  `
})
export class CallbackComponent implements OnInit {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  async ngOnInit() {
    try {
      const success = await this.authService.handleCallback();
      
      if (success) {
        // Redirect to home page or requested page
        const returnUrl = sessionStorage.getItem('returnUrl') || '/';
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
