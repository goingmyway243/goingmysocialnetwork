import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root',
})
export class BaseApiService {
    protected apiUrl = ''; // Replace with correct API URL

    constructor(private http: HttpClient) {}

    get<T>(url: string): Observable<T> {
        return this.http.get<T>(url ? `${this.apiUrl}/${url}` : this.apiUrl);
    }

    post<T>(url: string, body: any): Observable<T> {
        return this.http.post<T>(url ? `${this.apiUrl}/${url}` : this.apiUrl, body);
    }

    put<T>(url: string, body: any): Observable<T> {
        return this.http.put<T>(url ? `${this.apiUrl}/${url}` : this.apiUrl, body);
    }

    delete<T>(url: string): Observable<T> {
        return this.http.delete<T>(url ? `${this.apiUrl}/${url}` : this.apiUrl);
    }
}