import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root',
})
export class BaseAuthService {
    protected apiUrl = ''; // Replace with correct API URL

    constructor(private http: HttpClient) {}

    get<T>(url: string): Observable<T> {
        return this.http.get<T>(`${this.apiUrl}/${url}`, { withCredentials: true });
    }

    post<T>(url: string, body: any): Observable<T> {
        return this.http.post<T>(`${this.apiUrl}/${url}`, body, { withCredentials: true });
    }

    put<T>(url: string, body: any): Observable<T> {
        return this.http.put<T>(`${this.apiUrl}/${url}`, body, { withCredentials: true });
    }

    delete<T>(url: string): Observable<T> {
        return this.http.delete<T>(`${this.apiUrl}/${url}`, { withCredentials: true });
    }
}