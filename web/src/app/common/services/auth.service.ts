import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { ILoginResponse } from '../dtos/identity-api.dto';
import { User } from '../models/user.model';

@Injectable({
    providedIn: 'root',
})
export class AuthService {
    private _currentUserSubject = new BehaviorSubject<User | null>(null);

    currentUser$: Observable<User | null> = this._currentUserSubject.asObservable();

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

    updateAuthInfo(result: ILoginResponse) {
        this._currentUserSubject.next(result.user); // Set the current user on successful login
        localStorage.setItem('access_token', result.token);
    }
}
