import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { NotificationDto, NotificationPagedResult } from '../models/notification.models';

@Injectable({ providedIn: 'root' })
export class NotificationApiService {

  // ── 1. Dependencies ─────────────────────────────────────────
  private readonly _http = inject(HttpClient);
  private readonly _baseUrl = `${environment.apiGatewayUrl}/api/notifications`;

  // ── 2. Read ──────────────────────────────────────────────────
  getNotifications(pageNumber = 0, pageSize = 20): Observable<NotificationPagedResult> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    return this._http.get<NotificationPagedResult>(this._baseUrl, { params });
  }

  getUnreadCount(): Observable<{ count: number }> {
    return this._http.get<{ count: number }>(`${this._baseUrl}/unread-count`);
  }

  // ── 3. Write ─────────────────────────────────────────────────
  markAsRead(id: string): Observable<void> {
    return this._http.put<void>(`${this._baseUrl}/${id}/read`, {});
  }

  markAllAsRead(): Observable<void> {
    return this._http.put<void>(`${this._baseUrl}/read-all`, {});
  }

  delete(id: string): Observable<void> {
    return this._http.delete<void>(`${this._baseUrl}/${id}`);
  }
}
