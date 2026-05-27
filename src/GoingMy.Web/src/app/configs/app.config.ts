import { ApplicationConfig, provideZoneChangeDetection, provideAppInitializer, inject } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';

import { routes } from '../app.routes';
import { providePrimeNG } from 'primeng/config';
import { MessageService } from 'primeng/api';
import { DarkGlassPreset } from './app.theme';
import { authInterceptor } from '../interceptors/auth.interceptor';
import { refreshTokenInterceptor } from '../interceptors/refresh-token.interceptor';
import { provideOAuthClient } from 'angular-oauth2-oidc';
import { AuthService } from '../services/auth.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideAnimationsAsync(),
    provideHttpClient(
      withFetch(),
      withInterceptors([authInterceptor, refreshTokenInterceptor])
    ),
    provideOAuthClient(),
    provideAppInitializer(() => {
      const authService = inject(AuthService);
      authService.initAuth();
    }),
    providePrimeNG({
      theme: {
        preset: DarkGlassPreset
      }
    }),
    MessageService
  ]
};
