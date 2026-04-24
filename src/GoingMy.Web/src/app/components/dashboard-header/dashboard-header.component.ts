import { Component, ViewChild, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MenubarModule } from 'primeng/menubar';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { MenuModule } from 'primeng/menu';
import { Menu } from 'primeng/menu';
import { MenuItem } from 'primeng/api';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-dashboard-header',
  imports: [FormsModule, MenubarModule, ButtonModule, InputTextModule, MenuModule],
  templateUrl: './dashboard-header.component.html',
  styleUrl: './dashboard-header.component.css'
})
export class DashboardHeaderComponent {
  @ViewChild('userMenu') userMenu!: Menu;
  
  private readonly _authService = inject(AuthService);
  
  searchValue = signal('');
  userMenuItems: MenuItem[] = [];

  constructor(
    private router: Router,
    private authService: AuthService
  ) {
    this.initializeUserMenu();
  }

  private initializeUserMenu(): void {
    this.userMenuItems = [
      {
        label: 'Profile',
        icon: 'pi pi-user',
        command: () => this.navigateToProfile()
      },
      {
        separator: true
      },
      {
        label: 'Logout',
        icon: 'pi pi-sign-out',
        command: () => this.logout()
      }
    ];
  }

  toggleUserMenu(event: Event): void {
    if (this.userMenu) {
      this.userMenu.toggle(event);
    }
  }

  navigateToHome(): void {
    this.router.navigate(['/dashboard']);
  }

  navigateToProfile(): void {
    const userId = this._authService.getCurrentUserId();
    if (userId) {
      this.router.navigate(['/profile', userId]);
    }
  }

  logout(): void {
    this.authService.logout();
  }

  navigateToMessages(): void {
    this.router.navigate(['/dashboard/messages']);
  }

  onSearch(): void {
    if (this.searchValue().trim()) {
      this.router.navigate(['/discover'], { queryParams: { search: this.searchValue().trim() } });
      this.searchValue.set('');
    }
  }
}
