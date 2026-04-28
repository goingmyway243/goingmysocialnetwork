import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  UserProfile,
  UpdateProfileRequest,
  UpdateAvatarRequest,
  UpdateCoverRequest
} from '../models/user.models';

/** Auth-scoped signup request — targets AuthService, not UserService. */
export interface SignUpRequest {
  username: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

/** Auth-scoped user identity response (no profile fields). */
export interface AuthUserResponse {
  id: string;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
  lastLoginAt?: string;
}

/** Change password request — auth operation, stays in AuthService. */
export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserApiService {
  private readonly _http = inject(HttpClient);
  private readonly _authBaseUrl = `${environment.apiGatewayUrl}/api/user`;
  private readonly _userBaseUrl = `${environment.apiGatewayUrl}/api/userprofiles`;

  // ── Auth Service ─────────────────────────────────────────────

  /** POST /api/user/signup (AuthService) */
  signUp(request: SignUpRequest): Observable<AuthUserResponse> {
    return this._http.post<AuthUserResponse>(`${this._authBaseUrl}/signup`, request);
  }

  /** GET /api/user/{id} (AuthService) */
  getAuthUserById(id: string): Observable<AuthUserResponse> {
    return this._http.get<AuthUserResponse>(`${this._authBaseUrl}/${id}`);
  }

  /** POST /api/user/{id}/change-password (AuthService) */
  changePassword(id: string, request: ChangePasswordRequest): Observable<{ message: string }> {
    return this._http.post<{ message: string }>(`${this._authBaseUrl}/${id}/change-password`, request);
  }

  // ── User Service — Profile ───────────────────────────────────

  /** GET /api/userprofiles/{id} (UserService) */
  getUserProfile(id: string): Observable<UserProfile> {
    return this._http.get<UserProfile>(`${this._userBaseUrl}/${id}`);
  }

  /** PUT /api/userprofiles/{id} (UserService) */
  updateUserProfile(id: string, request: UpdateProfileRequest): Observable<UserProfile> {
    return this._http.put<UserProfile>(`${this._userBaseUrl}/${id}`, request);
  }

  // ── User Service — Media ─────────────────────────────────────

  /** POST /api/userprofiles/{id}/avatar (UserService) */
  updateAvatar(id: string, request: UpdateAvatarRequest): Observable<UserProfile> {
    return this._http.post<UserProfile>(`${this._userBaseUrl}/${id}/avatar`, request);
  }

  /** POST /api/userprofiles/{id}/cover (UserService) */
  updateCover(id: string, request: UpdateCoverRequest): Observable<UserProfile> {
    return this._http.post<UserProfile>(`${this._userBaseUrl}/${id}/cover`, request);
  }

  // ── User Service — Follow Graph ──────────────────────────────

  /** POST /api/userprofiles/{id}/follow (UserService) */
  followUser(id: string): Observable<void> {
    return this._http.post<void>(`${this._userBaseUrl}/${id}/follow`, {});
  }

  /** DELETE /api/userprofiles/{id}/follow (UserService) */
  unfollowUser(id: string): Observable<void> {
    return this._http.delete<void>(`${this._userBaseUrl}/${id}/follow`);
  }

  /** GET /api/userprofiles/{id}/is-following (UserService) */
  checkIsFollowing(id: string): Observable<boolean> {
    return this._http.get<boolean>(`${this._userBaseUrl}/${id}/is-following`);
  }

  /** GET /api/userprofiles/{id}/followers (UserService) */
  getFollowers(id: string, page = 1, pageSize = 20): Observable<UserProfile[]> {
    return this._http.get<UserProfile[]>(
      `${this._userBaseUrl}/${id}/followers?page=${page}&pageSize=${pageSize}`);
  }

/** GET /api/userprofiles/{id}/following (UserService) */
  getFollowing(id: string, page = 1, pageSize = 20): Observable<UserProfile[]> {
    return this._http.get<UserProfile[]>(
      `${this._userBaseUrl}/${id}/following?page=${page}&pageSize=${pageSize}`);
  }

  // ── User Service — Search ────────────────────────────────────

  /** GET /api/userprofiles/search (UserService) */
  searchUsers(searchTerm?: string, location?: string, isVerified?: boolean, page = 1, pageSize = 20): Observable<UserProfile[]> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) params = params.set('searchTerm', searchTerm);
    if (location) params = params.set('location', location);
    if (isVerified !== undefined) params = params.set('isVerified', isVerified.toString());

    return this._http.get<UserProfile[]>(`${this._userBaseUrl}/search`, { params });
  }
}
