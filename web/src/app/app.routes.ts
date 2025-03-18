import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/dashboard-page/dashboard-page.component').then(p => p.DashboardPageComponent),
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
    loadComponent: () => import('./pages/login-page/login-page.component').then(p => p.LoginPageComponent)
  },
  {
    path: 'signup',
    loadComponent: () => import('./pages/signup-page/signup-page.component').then(p => p.SignupPageComponent)
  },
  { path: '**', redirectTo: '', pathMatch: 'full' }
];
