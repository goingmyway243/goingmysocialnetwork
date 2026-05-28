import { Component, OnInit, ViewChild, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MenubarModule } from 'primeng/menubar';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { MenuModule } from 'primeng/menu';
import { Menu } from 'primeng/menu';
import { MenuItem } from 'primeng/api';
import { AuthService } from '../../services/auth.service';
import { ThemeService } from '../../services/theme.service';
import { UserApiService } from '../../services/user-api.service';
import { NotificationBellComponent } from '../notification-bell/notification-bell.component';

@Component({
  selector: 'app-dashboard-header',
  imports: [FormsModule, MenubarModule, ButtonModule, InputTextModule, MenuModule, NotificationBellComponent],
  templateUrl: './dashboard-header.component.html',
  styleUrl: './dashboard-header.component.css'
})
export class DashboardHeaderComponent {
  @ViewChild('userMenu') userMenu!: Menu;
  
  private readonly _authService = inject(AuthService);
  private readonly _userApi = inject(UserApiService);
  readonly themeService = inject(ThemeService);
  
  searchValue = signal('');
  readonly currentUserAvatarUrl = signal<string | null>(null);
  userMenuItems: MenuItem[] = [];

  constructor(
    private router: Router,
    private authService: AuthService
  ) {
    this.initializeUserMenu();
  }

  ngOnInit(): void {
    this._loadCurrentUserAvatar();
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
      this.router.navigate(['/dashboard/profile', userId]);
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
      this.router.navigate(['/dashboard/discover'], { queryParams: { search: this.searchValue().trim() } });
      this.searchValue.set('');
    }
  }

  private _loadCurrentUserAvatar(): void {
    const currentUserId = this._authService.getCurrentUserId();
    if (!currentUserId) {
      this.currentUserAvatarUrl.set(null);
      return;
    }

    this._userApi.getUserProfile(currentUserId).subscribe({
      next: profile => this.currentUserAvatarUrl.set(profile.avatarUrl ?? null),
      error: () => this.currentUserAvatarUrl.set(null)
    });
  }
}
