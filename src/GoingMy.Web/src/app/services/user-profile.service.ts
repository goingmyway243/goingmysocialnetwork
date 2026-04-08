import { Injectable, inject, signal, computed } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { UserApiService } from './user-api.service';
import { UserProfile, UpdateProfileRequest, UpdateAvatarRequest, UpdateCoverRequest } from '../models/user.models';

@Injectable({
  providedIn: 'root'
})
export class UserProfileService {
  private readonly _api = inject(UserApiService);

  // ── State signals ────────────────────────────────────────────
  private readonly _profile = signal<UserProfile | null>(null);
  private readonly _followers = signal<UserProfile[]>([]);
  private readonly _following = signal<UserProfile[]>([]);

  readonly profile = computed(() => this._profile());
  readonly followers = computed(() => this._followers());
  readonly following = computed(() => this._following());

  // ── Profile ───────────────────────────────────────────────────

  /** Fetches a profile by ID and stores it in the signal. */
  loadProfile(id: string): Observable<UserProfile> {
    return this._api.getUserProfile(id).pipe(
      tap(p => this._profile.set(p))
    );
  }

  /** Updates the authenticated user's profile. */
  updateProfile(id: string, request: UpdateProfileRequest): Observable<UserProfile> {
    return this._api.updateUserProfile(id, request).pipe(
      tap(p => this._profile.set(p))
    );
  }

  // ── Media ─────────────────────────────────────────────────────

  updateAvatar(id: string, avatarUrl: string): Observable<UserProfile> {
    return this._api.updateAvatar(id, { avatarUrl } as UpdateAvatarRequest).pipe(
      tap(p => this._profile.set(p))
    );
  }

  updateCover(id: string, coverUrl: string): Observable<UserProfile> {
    return this._api.updateCover(id, { coverUrl } as UpdateCoverRequest).pipe(
      tap(p => this._profile.set(p))
    );
  }

  // ── Follow Graph ──────────────────────────────────────────────

  followUser(id: string): Observable<void> {
    return this._api.followUser(id).pipe(
      tap(() => {
        // Optimistically increment the followee's counter if it's the loaded profile
        const current = this._profile();
        if (current?.id === id) {
          this._profile.set({ ...current, followersCount: current.followersCount + 1 });
        }
      })
    );
  }

  unfollowUser(id: string): Observable<void> {
    return this._api.unfollowUser(id).pipe(
      tap(() => {
        const current = this._profile();
        if (current?.id === id) {
          this._profile.set({ ...current, followersCount: Math.max(0, current.followersCount - 1) });
        }
      })
    );
  }

  loadFollowers(id: string, page = 1, pageSize = 20): Observable<UserProfile[]> {
    return this._api.getFollowers(id, page, pageSize).pipe(
      tap(list => this._followers.set(list))
    );
  }

  loadFollowing(id: string, page = 1, pageSize = 20): Observable<UserProfile[]> {
    return this._api.getFollowing(id, page, pageSize).pipe(
      tap(list => this._following.set(list))
    );
  }

  clearProfile(): void {
    this._profile.set(null);
    this._followers.set([]);
    this._following.set([]);
  }
}
