import { Component, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { MenuItem } from 'primeng/api';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard-sidebar',
  imports: [CommonModule, RouterModule, ButtonModule],
  templateUrl: './dashboard-sidebar.component.html',
  styleUrl: './dashboard-sidebar.component.css'
})
export class DashboardSidebarComponent {
  readonly activeMenu = signal('home');

  menuItems: MenuItem[] = [
    {
      label: 'Home',
      icon: 'pi pi-home',
      id: 'home',
      command: () => this.setActiveMenu('home', '/dashboard')
    },
    {
      label: 'Explore',
      icon: 'pi pi-compass',
      id: 'explore',
      command: () => this.setActiveMenu('explore', '/dashboard/explore')
    },
    {
      label: 'Discover',
      icon: 'pi pi-users',
      id: 'discover',
      command: () => this.setActiveMenu('discover', '/discover')
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
    {
      label: 'Bookmarks',
      icon: 'pi pi-bookmark',
      id: 'bookmarks',
      command: () => this.setActiveMenu('bookmarks', '/dashboard/bookmarks')
    }
  ];

  constructor(private router: Router) {}

  setActiveMenu(menuId: string, route?: string): void {
    this.activeMenu.set(menuId);
    if (route) {
      this.router.navigate([route]);
    }
  }

  isActive(menuId: string): boolean {
    return this.activeMenu() === menuId;
  }
}
