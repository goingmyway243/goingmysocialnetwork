import { Injectable, inject, signal, computed } from '@angular/core';
import { Observable, tap, forkJoin, map, catchError, of } from 'rxjs';
import { UserApiService } from './user-api.service';
import { AuthService } from './auth.service';
import { PostApiService } from './post-api.service';
import { UserProfile, UpdateProfileRequest, UpdateAvatarRequest, UpdateCoverRequest } from '../models/user.models';
import { Post } from '../models/post.model';

@Injectable({
  providedIn: 'root'
})
export class UserProfileService {
  private readonly _api = inject(UserApiService);
  private readonly _postApi = inject(PostApiService);
  private readonly _authService = inject(AuthService);

  // ── State signals ────────────────────────────────────────────
  private readonly _profile = signal<UserProfile | null>(null);
  private readonly _followers = signal<UserProfile[]>([]);
  private readonly _following = signal<UserProfile[]>([]);
  private readonly _profilePosts = signal<Post[]>([]);
  private readonly _profileLikes = signal<Post[]>([]);
  private readonly _isFollowing = signal(false);
  private readonly _postsLoading = signal(false);
  private readonly _likesLoading = signal(false);
  private readonly _followersLoading = signal(false);
  private readonly _followingLoading = signal(false);

  // ── Public readonly accessors ────────────────────────────────
  readonly profile = computed(() => this._profile());
  readonly followers = computed(() => this._followers());
  readonly following = computed(() => this._following());
  readonly profilePosts = computed(() => this._profilePosts());
  readonly profileLikes = computed(() => this._profileLikes());
  readonly isFollowing = computed(() => this._isFollowing());
  readonly postsLoading = computed(() => this._postsLoading());
  readonly likesLoading = computed(() => this._likesLoading());
  readonly followersLoading = computed(() => this._followersLoading());
  readonly followingLoading = computed(() => this._followingLoading());

  /** True when the loaded profile belongs to the currently logged-in user. */
  readonly isOwnProfile = computed(() => {
    const currentId = this._authService.getCurrentUserId();
    const profileId = this._profile()?.id;
    return !!currentId && !!profileId && currentId === profileId;
  });

  // ── Profile ───────────────────────────────────────────────────

  /** Fetches a profile by ID and stores it in the signal. Also checks if current user is following. */
  loadProfile(id: string): Observable<UserProfile> {
    const currentUserId = this._authService.getCurrentUserId();
    const profileObs = this._api.getUserProfile(id);

    // If user is logged in, also check follow status; otherwise just get profile
    if (currentUserId) {
      return forkJoin({
        profile: profileObs,
        isFollowing: this._api.checkIsFollowing(id).pipe(
          catchError(() => of(false)) // Default to false if check fails (e.g., 401)
        )
      }).pipe(
        tap(({ profile, isFollowing }) => {
          this._profile.set(profile);
          this._isFollowing.set(isFollowing);
        }),
        map(({ profile }) => profile)
      );
    } else {
      // Not logged in: just get profile and set isFollowing to false
      return profileObs.pipe(
        tap(profile => {
          this._profile.set(profile);
          this._isFollowing.set(false);
        })
      );
    }
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
        this._isFollowing.set(true);
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
        this._isFollowing.set(false);
        const current = this._profile();
        if (current?.id === id) {
          this._profile.set({ ...current, followersCount: Math.max(0, current.followersCount - 1) });
        }
      })
    );
  }

  setIsFollowing(value: boolean): void {
    this._isFollowing.set(value);
  }

  loadFollowers(id: string, page = 1, pageSize = 20): Observable<UserProfile[]> {
    this._followersLoading.set(true);
    return this._api.getFollowers(id, page, pageSize).pipe(
      tap(list => {
        this._followers.set(list);
        this._followersLoading.set(false);
      })
    );
  }

  loadFollowing(id: string, page = 1, pageSize = 20): Observable<UserProfile[]> {
    this._followingLoading.set(true);
    return this._api.getFollowing(id, page, pageSize).pipe(
      tap(list => {
        this._following.set(list);
        this._followingLoading.set(false);
      })
    );
  }

  // ── Profile Tab Content ───────────────────────────────────────

  loadProfilePosts(userId: string, page = 1, pageSize = 20): Observable<Post[]> {
    this._postsLoading.set(true);
    return this._postApi.getUserPosts(userId, page, pageSize).pipe(
      tap(posts => {
        const normalized = posts.map(p => ({ ...p, likes: p.likes ?? 0, comments: p.comments ?? 0 }));
        this._profilePosts.set(normalized);
        this._postsLoading.set(false);
      })
    );
  }

  loadProfileLikes(userId: string, page = 1, pageSize = 20): Observable<Post[]> {
    this._likesLoading.set(true);
    return this._postApi.getUserLikedPosts(userId, page, pageSize).pipe(
      tap(posts => {
        const normalized = posts.map(p => ({ ...p, likes: p.likes ?? 0, comments: p.comments ?? 0 }));
        this._profileLikes.set(normalized);
        this._likesLoading.set(false);
      })
    );
  }

  /** Update like count on a post in both profilePosts and profileLikes lists. */
  updatePostLike(postId: string, delta: number): void {
    const applyDelta = (posts: Post[]) =>
      posts.map(p => p.id === postId ? { ...p, likes: Math.max(0, (p.likes ?? 0) + delta) } : p);
    this._profilePosts.update(applyDelta);
    this._profileLikes.update(applyDelta);
  }

  /** Prepend a newly created post to the top of profilePosts. */
  prependPost(post: Post): void {
    const normalized = { ...post, likes: post.likes ?? 0, comments: post.comments ?? 0 };
    this._profilePosts.update(current => [normalized, ...current]);
    const profile = this._profile();
    if (profile) {
      this._profile.set({ ...profile, postsCount: profile.postsCount + 1 });
    }
  }

  clearProfile(): void {
    this._profile.set(null);
    this._followers.set([]);
    this._following.set([]);
    this._profilePosts.set([]);
    this._profileLikes.set([]);
    this._isFollowing.set(false);
  }

  // ── Search ────────────────────────────────────────────────────

  private readonly _searchResults = signal<UserProfile[]>([]);
  private readonly _searchLoading = signal(false);
  private readonly _searchError = signal<string | null>(null);

  readonly searchResults = computed(() => this._searchResults());
  readonly searchLoading = computed(() => this._searchLoading());
  readonly searchError = computed(() => this._searchError());

  searchUsers(searchTerm?: string, location?: string, isVerified?: boolean, page = 1, pageSize = 20): Observable<UserProfile[]> {
    this._searchLoading.set(true);
    this._searchError.set(null);
    return this._api.searchUsers(searchTerm, location, isVerified, page, pageSize).pipe(
      tap({
        next: results => {
          this._searchResults.set(results);
          this._searchLoading.set(false);
        },
        error: () => {
          this._searchError.set('Failed to search users. Please try again.');
          this._searchLoading.set(false);
        }
      })
    );
  }

  clearSearchResults(): void {
    this._searchResults.set([]);
    this._searchError.set(null);
  }
}
