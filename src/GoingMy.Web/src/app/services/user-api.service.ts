import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

// Enums
export enum Gender {
  Male = 0,
  Female = 1,
  Other = 2
}

// Request DTOs
export interface SignUpRequest {
  username: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface UpdateUserRequest {
  firstName?: string;
  lastName?: string;
  bio?: string;
  dateOfBirth?: string; // ISO date string
  gender?: Gender;
  location?: string;
  websiteUrl?: string;
  isPrivate?: boolean;
}

export interface ChangeAvatarRequest {
  avatarUrl: string;
}

export interface ChangeCoverRequest {
  coverUrl: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

// Response DTOs
export interface UserResponse {
  id: string;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  bio?: string;
  avatarUrl?: string;
  coverUrl?: string;
  dateOfBirth?: string;
  gender: Gender;
  location?: string;
  websiteUrl?: string;
  followersCount: number;
  followingCount: number;
  postsCount: number;
  isVerified: boolean;
  isPrivate: boolean;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
  lastLoginAt?: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserApiService {
  private readonly baseUrl = 'https://localhost:7001/api/user';

  constructor(private http: HttpClient) {}

  /**
   * Register a new user account
   * POST /api/user/signup
   */
  signUp(request: SignUpRequest): Observable<UserResponse> {
    return this.http.post<UserResponse>(`${this.baseUrl}/signup`, request);
  }

  /**
   * Get user by ID
   * GET /api/user/{id}
   */
  getUserById(id: string): Observable<UserResponse> {
    return this.http.get<UserResponse>(`${this.baseUrl}/${id}`);
  }

  /**
   * Get user by username
   * GET /api/user/username/{username}
   */
  getUserByUsername(username: string): Observable<UserResponse> {
    return this.http.get<UserResponse>(`${this.baseUrl}/username/${username}`);
  }

  /**
   * Update user profile
   * PUT /api/user/{id}
   * Requires authorization
   */
  updateUser(id: string, request: UpdateUserRequest): Observable<UserResponse> {
    return this.http.put<UserResponse>(`${this.baseUrl}/${id}`, request);
  }

  /**
   * Deactivate user account (soft delete)
   * DELETE /api/user/{id}
   * Requires authorization
   */
  deleteUser(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  /**
   * Change user avatar
   * POST /api/user/{id}/avatar
   * Requires authorization
   */
  changeAvatar(id: string, request: ChangeAvatarRequest): Observable<UserResponse> {
    return this.http.post<UserResponse>(`${this.baseUrl}/${id}/avatar`, request);
  }

  /**
   * Change user cover photo
   * POST /api/user/{id}/cover
   * Requires authorization
   */
  changeCover(id: string, request: ChangeCoverRequest): Observable<UserResponse> {
    return this.http.post<UserResponse>(`${this.baseUrl}/${id}/cover`, request);
  }

  /**
   * Change user password
   * POST /api/user/{id}/change-password
   * Requires authorization
   */
  changePassword(id: string, request: ChangePasswordRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.baseUrl}/${id}/change-password`, request);
  }
}
