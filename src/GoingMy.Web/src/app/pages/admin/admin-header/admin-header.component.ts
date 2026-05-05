import { Component, inject } from '@angular/core';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-admin-header',
  imports: [],
  templateUrl: './admin-header.component.html',
  styleUrl: './admin-header.component.css'
})
export class AdminHeaderComponent {
  private readonly _authService = inject(AuthService);

  get username(): string {
    return this._authService.getCurrentUsername();
  }
}
