import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LoginRequest, RegisterRequest, RegisterResponse } from '../models/auth.model';
import { User } from '../models/user.model';


@Injectable({
    providedIn: 'root',
})
export class IdentityService {
    private readonly baseUrl = 'api/identity';

    constructor(private http: HttpClient) {}

    register(request: RegisterRequest): Observable<RegisterResponse> {
        return this.http.post<RegisterResponse>(`${this.baseUrl}/register`, request);
    }

    login(request: LoginRequest): Observable<User> {
        return this.http.post<User>(`${this.baseUrl}/login`, request);
    }

    checkUserInRole(userId: string, role: string): Observable<boolean> {
        return this.http.get<boolean>(`${this.baseUrl}/check-role`, {
            params: { userId, role },
        });
    }

    logout(): Observable<void> {
        return this.http.get<void>(`${this.baseUrl}/logout`);
    }
}