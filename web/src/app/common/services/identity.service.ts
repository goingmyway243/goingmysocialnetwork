import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { ILoginRequest, ILoginResponse, IRegisterRequest, IRegisterResponse } from '../models/auth.model';
import { User } from '../models/user.model';
import { environment } from '../../../environments/environment';

@Injectable({
    providedIn: 'root',
})
export class IdentityService {
    private readonly apiUrl = `${environment.baseUrl}/api/identity`;
    private _currentUserSubject = new BehaviorSubject<User | null>(null);

    currentUser$: Observable<User | null> = this._currentUserSubject.asObservable();

    constructor(private http: HttpClient) { }

    getCurrentUserId(): string {
        return this.getCurrentUser()?.id ?? ''
    }

    getCurrentUser(): User | null {
        return this._currentUserSubject.value;
    }

    getAccessToken(): string | null {
        return localStorage.getItem('access_token');
    }

    isAuthenticated(): boolean {
        var accessToken = this.getAccessToken();
        if (accessToken) {
            const payload = JSON.parse(atob(accessToken.split('.')[1]));
            const expiry = payload.exp * 1000;

            if (Date.now() < expiry) {
                return true;
            }
        }

        return false;
    }

    getUserInfoAsync(): Observable<ILoginResponse> {
        return this.http.get<ILoginResponse>(`${this.apiUrl}/me`).pipe(
            tap((result) => this.updateAuthInfo(result))
        );
    }

    registerAsync(request: IRegisterRequest): Observable<IRegisterResponse> {
        return this.http.post<IRegisterResponse>(`${this.apiUrl}/register`, request);
    }

    loginAsync(request: ILoginRequest): Observable<ILoginResponse> {
        return this.http.post<ILoginResponse>(`${this.apiUrl}/login`, request).pipe(
            tap((result) => this.updateAuthInfo(result))
        );
    }

    checkUserInRoleAsync(userId: string, role: string): Observable<boolean> {
        return this.http.get<boolean>(`${this.apiUrl}/check-role`, {
            params: { userId, role },
        });
    }

    logoutAsync(): Observable<void> {
        return this.http.get<void>(`${this.apiUrl}/logout`);
    }

    private updateAuthInfo(result: ILoginResponse) {
        this._currentUserSubject.next(result.user); // Set the current user on successful login
        localStorage.setItem('access_token', result.token);
    }
}
