import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

// ── Models ────────────────────────────────────────────────────

export interface AdminUser {
  id: string;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: number[];
  isActive: boolean;
  createdAt: string;
  lastLoginAt?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface DailyRegistration {
  date: string;
  count: number;
}

export interface UserStats {
  totalUsers: number;
  activeUsers: number;
  adminUsers: number;
  registrationsLast30Days: DailyRegistration[];
}

export interface PostStats {
  totalPosts: number;
  totalLikes: number;
  totalComments: number;
  postsLast7Days: number;
  postsLast30Days: number;
}

export interface GetUsersParams {
  page?: number;
  pageSize?: number;
  search?: string;
  isActive?: boolean;
}

// ── Service ───────────────────────────────────────────────────

@Injectable({ providedIn: 'root' })
export class AdminApiService {
  private readonly _http = inject(HttpClient);
  private readonly _authAdminBase = `${environment.apiGatewayUrl}/api/admin`;
  private readonly _postAdminBase = `${environment.apiGatewayUrl}/api/posts/admin`;

  // ── Users ────────────────────────────────────────────────────

  getUsers(params: GetUsersParams = {}): Observable<PagedResult<AdminUser>> {
    let httpParams = new HttpParams()
      .set('page', params.page ?? 1)
      .set('pageSize', params.pageSize ?? 20);

    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.isActive !== undefined) httpParams = httpParams.set('isActive', params.isActive);

    return this._http.get<PagedResult<AdminUser>>(`${this._authAdminBase}/users`, { params: httpParams });
  }

  setUserStatus(userId: string, isActive: boolean): Observable<AdminUser> {
    return this._http.patch<AdminUser>(`${this._authAdminBase}/users/${userId}/status`, { isActive });
  }

  revokeUserTokens(userId: string): Observable<void> {
    return this._http.post<void>(`${this._authAdminBase}/users/${userId}/revoke-tokens`, {});
  }

  // ── Stats ────────────────────────────────────────────────────

  getUserStats(): Observable<UserStats> {
    return this._http.get<UserStats>(`${this._authAdminBase}/stats/users`);
  }

  getPostStats(): Observable<PostStats> {
    return this._http.get<PostStats>(`${this._postAdminBase}/stats`);
  }
}
