import { ApplicationConfig, provideZoneChangeDetection, provideAppInitializer, inject } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';

import { routes } from '../app.routes';
import { providePrimeNG } from 'primeng/config';
import MyPreset from './app.theme';
import { authInterceptor } from '../interceptors/auth.interceptor';
import { provideOAuthClient } from 'angular-oauth2-oidc';
import { AuthService } from '../services/auth.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(
      withFetch(),
      withInterceptors([authInterceptor])
    ),
    provideOAuthClient({
      resourceServer: {
        allowedUrls: ['http://localhost:5133/api'],
        sendAccessToken: true
      }
    }),
    provideAppInitializer(() => {
      const authService = inject(AuthService);
      authService.initAuth();
    }),
    providePrimeNG({
      theme: {
        preset: MyPreset
      }
    })
  ]
};
