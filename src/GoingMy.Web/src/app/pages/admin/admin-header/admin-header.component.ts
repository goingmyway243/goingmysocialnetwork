import { Component, inject } from '@angular/core';
import { AuthService } from '../../../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-admin-header',
  imports: [],
  templateUrl: './admin-header.component.html',
  styleUrl: './admin-header.component.css'
})
export class AdminHeaderComponent {
  private readonly _authService = inject(AuthService);
  private readonly _router = inject(Router);

  get username(): string {
    return this._authService.getCurrentUsername();
  }

  navigateToHome(): void {
    this._router.navigate(['/admin']);
  }
}
