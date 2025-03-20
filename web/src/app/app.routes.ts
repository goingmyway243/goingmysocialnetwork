import { Routes } from '@angular/router';
import { AuthGuard } from './common/guards/auth.guard';
import { AuthRedirectGuard } from './common/guards/auth-redirect.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/dashboard-page/dashboard-page.component').then(p => p.DashboardPageComponent),
    canActivate: [AuthGuard],
    children: [
      {
        path: '',
        loadComponent: () => import('./pages/home-page/home-page.component').then(p => p.HomePageComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./pages/profile-page/profile-page.component').then(p => p.ProfilePageComponent)
      },
    ]
  },
  {
    path: 'login',
    canActivate: [AuthRedirectGuard],
    loadComponent: () => import('./pages/login-page/login-page.component').then(p => p.LoginPageComponent)
  },
  {
    path: 'signup',
    canActivate: [AuthRedirectGuard],
    loadComponent: () => import('./pages/signup-page/signup-page.component').then(p => p.SignupPageComponent)
  },
  { path: '**', redirectTo: '', pathMatch: 'full' }
];
