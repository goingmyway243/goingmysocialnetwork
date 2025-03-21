import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, tap } from 'rxjs';
import { ILoginRequest, ILoginResponse, IRegisterRequest, IRegisterResponse } from '../dtos/identity-api.dto';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import { BaseApiService } from './base-api.service';

@Injectable({
    providedIn: 'root',
})
export class IdentityApiService extends BaseApiService {
    protected override apiUrl: string = `${environment.baseUrl}/api/identity`;
  
    constructor(http: HttpClient, private authService: AuthService) { 
      super(http);
    }

    fetchUserInfoIfNeeded(): Observable<any> {
      const isAuthenticated = this.authService.isAuthenticated();
      const currentUser = this.authService.getCurrentUser();
      return isAuthenticated && !currentUser ? this.getUserInfoAsync() : of();
    }

    getUserInfoAsync(): Observable<ILoginResponse> {
        return this.get<ILoginResponse>('me').pipe(
            tap((result) => this.authService.updateAuthInfo(result))
        );
    }

    registerAsync(request: IRegisterRequest): Observable<IRegisterResponse> {
        return this.post<IRegisterResponse>('register', request);
    }

    loginAsync(request: ILoginRequest): Observable<ILoginResponse> {
        return this.post<ILoginResponse>('login', request).pipe(
            tap((result) => this.authService.updateAuthInfo(result))
        );
    }

    checkUserInRoleAsync(userId: string, role: string): Observable<boolean> {
        return this.get<boolean>(`check-role?userId=${userId}&role=${role}`);
    }

    logoutAsync(): Observable<void> {
        return this.get<void>('logout');
    }
}
