import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    // OAuth2/OIDC callback route for PKCE flow
    path: 'signin-oidc',
    loadComponent: () => import('./pages/auth-callback/auth-callback.component').then(m => m.AuthCallbackComponent)
  },
  {
    path: 'posts/:id',
    loadComponent: () => import('./pages/post-detail/post-detail.component').then(m => m.PostDetailComponent),
    canActivate: [authGuard]
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent),
    canActivate: [authGuard],
    children: [
      {
        path: '',
        redirectTo: 'home',
        pathMatch: 'full'
      },
      {
        path: 'home',
        loadComponent: () => import('./pages/dashboard/dashboard-home/dashboard-home.component').then(m => m.DashboardHomeComponent)
      },
      {
        path: 'explore',
        loadComponent: () => import('./pages/dashboard/dashboard-home/dashboard-home.component').then(m => m.DashboardHomeComponent)
      },
      {
        path: 'messages',
        loadComponent: () => import('./pages/dashboard/dashboard-home/dashboard-home.component').then(m => m.DashboardHomeComponent)
      },
      {
        path: 'notifications',
        loadComponent: () => import('./pages/dashboard/dashboard-home/dashboard-home.component').then(m => m.DashboardHomeComponent)
      },
      {
        path: 'bookmarks',
        loadComponent: () => import('./pages/dashboard/dashboard-home/dashboard-home.component').then(m => m.DashboardHomeComponent)
      },
      {
        path: 'theme',
        loadComponent: () => import('./pages/dashboard/dashboard-home/dashboard-home.component').then(m => m.DashboardHomeComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./pages/dashboard/dashboard-home/dashboard-home.component').then(m => m.DashboardHomeComponent)
      }
    ]
  },
  {
    path: 'profile/:userId',
    loadComponent: () => import('./pages/profile/profile.component').then(m => m.ProfileComponent),
    canActivate: [authGuard]
  },
  {
    // Default: go to dashboard (authGuard will redirect to Blazor login if not authenticated)
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full'
  }
];
