import { Component, inject, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { MenuItem } from 'primeng/api';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-dashboard-sidebar',
  imports: [CommonModule, RouterModule, ButtonModule],
  templateUrl: './dashboard-sidebar.component.html',
  styleUrl: './dashboard-sidebar.component.css'
})
export class DashboardSidebarComponent {
  private readonly _authService = inject(AuthService);

  readonly activeMenu = signal('home');

  get isAdmin(): boolean { return this._authService.hasRole('Admin'); }

  menuItems: MenuItem[] = [
    {
      label: 'Home',
      icon: 'pi pi-home',
      id: 'home',
      command: () => this.setActiveMenu('home', '/dashboard')
    },
    {
      label: 'Discover',
      icon: 'pi pi-compass',
      id: 'explore',
      command: () => this.setActiveMenu('discover', '/dashboard/discover')
    },
    {
      label: 'Messages',
      icon: 'pi pi-envelope',
      id: 'messages',
      command: () => this.setActiveMenu('messages', '/dashboard/messages')
    },
    {
      label: 'Notifications',
      icon: 'pi pi-bell',
      id: 'notifications',
      command: () => this.setActiveMenu('notifications', '/dashboard/notifications')
    },
    // {
    //   label: 'Bookmarks',
    //   icon: 'pi pi-bookmark',
    //   id: 'bookmarks',
    //   command: () => this.setActiveMenu('bookmarks', '/dashboard/bookmarks')
    // }
  ];

  private readonly _router = inject(Router);

  setActiveMenu(menuId: string, route?: string): void {
    this.activeMenu.set(menuId);
    if (route) {
      this._router.navigate([route]);
    }
  }

  goToAdmin(): void {
    this._router.navigate(['/admin']);
  }

  isActive(menuId: string): boolean {
    return this.activeMenu() === menuId;
  }
}
