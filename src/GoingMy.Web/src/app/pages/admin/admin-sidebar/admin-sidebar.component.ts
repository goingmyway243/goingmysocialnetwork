import { Component, signal, inject } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

interface AdminNavItem {
  label: string;
  icon: string;
  id: string;
  route: string;
}

@Component({
  selector: 'app-admin-sidebar',
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-sidebar.component.html',
  styleUrl: './admin-sidebar.component.css'
})
export class AdminSidebarComponent {
  private readonly _router = inject(Router);

  readonly activeItem = signal('dashboard');

  readonly navItems: AdminNavItem[] = [
    { label: 'Dashboard', icon: 'pi pi-chart-bar', id: 'dashboard', route: '/admin/dashboard' },
    { label: 'Users',     icon: 'pi pi-users',     id: 'users',     route: '/admin/users'     }
  ];

  navigate(item: AdminNavItem): void {
    this.activeItem.set(item.id);
    this._router.navigate([item.route]);
  }

  backToDashboard(): void {
    this._router.navigate(['/dashboard']);
  }
}
