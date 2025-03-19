import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ILoginRequest, IRegisterRequest, IRegisterResponse } from '../models/auth.model';
import { User } from '../models/user.model';
import { environment } from '../../../environments/environment';


@Injectable({
    providedIn: 'root',
})
export class IdentityService {
    private readonly apiUrl = `${environment.baseUrl}/api/identity`;

    constructor(private http: HttpClient) {}

    register(request: IRegisterRequest): Observable<IRegisterResponse> {
        return this.http.post<IRegisterResponse>(`${this.apiUrl}/register`, request);
    }

    login(request: ILoginRequest): Observable<User> {
        return this.http.post<User>(`${this.apiUrl}/login`, request);
    }

    checkUserInRole(userId: string, role: string): Observable<boolean> {
        return this.http.get<boolean>(`${this.apiUrl}/check-role`, {
            params: { userId, role },
        });
    }

    logout(): Observable<void> {
        return this.http.get<void>(`${this.apiUrl}/logout`);
    }
}