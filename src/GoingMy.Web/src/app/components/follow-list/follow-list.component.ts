import {
  Component, Input, effect, inject, signal, computed
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { SkeletonModule } from 'primeng/skeleton';
import { UserProfileService } from '../../services/user-profile.service';
import { UserApiService } from '../../services/user-api.service';
import { AuthService } from '../../services/auth.service';
import { EmptyStateComponent } from '../empty-state/empty-state.component';
import { UserProfile } from '../../models/user.models';

type ListMode = 'followers' | 'following';

@Component({
  selector: 'app-follow-list',
  standalone: true,
  imports: [CommonModule, SkeletonModule, EmptyStateComponent],
  templateUrl: './follow-list.component.html',
  styleUrl: './follow-list.component.css'
})
export class FollowListComponent {
  @Input({ required: true })
  set userId(value: string) {
    this._userId.set(value);
  }

  @Input()
  set mode(value: ListMode) {
    this._mode.set(value);
  }

  // ── 1. Dependencies ─────────────────────────────────────────
  private readonly _profileService = inject(UserProfileService);
  private readonly _userApi = inject(UserApiService);
  private readonly _authService = inject(AuthService);
  private readonly _router = inject(Router);

  // ── 2. State ────────────────────────────────────────────────
  private readonly _userId = signal<string>('');
  private readonly _mode = signal<ListMode>('followers');
  private readonly _list = signal<UserProfile[]>([]);
  private readonly _loading = signal(false);
  private readonly _followLoading = signal<Set<string>>(new Set());
  private readonly _unfollowedIds = signal<Set<string>>(new Set());

  // ── 3. Derived State ─────────────────────────────────────────
  readonly list = computed(() => this._list());
  readonly loading = computed(() => this._loading());
  readonly currentUserId = computed(() => this._authService.getCurrentUserId());
  readonly isEmpty = computed(() => !this._loading() && this._list().length === 0);

  // ── 4. Constructor ──────────────────────────────────────────
  constructor() {
    // Watch for userId or mode changes and reload list
    effect(() => {
      const userId = this._userId();
      if (userId) {
        this.loadList();
      }
    });
  }

  // ── 5. Actions ───────────────────────────────────────────────
  private loadList(): void {
    this._loading.set(true);
    const userId = this._userId();

    const obs = this._mode() === 'followers'
      ? this._profileService.loadFollowers(userId)
      : this._profileService.loadFollowing(userId);

    obs.subscribe({
      next: list => { this._list.set(list); this._loading.set(false); },
      error: () => this._loading.set(false)
    });
  }

  isFollowLoading(userId: string): boolean {
    return this._followLoading().has(userId);
  }

  isUnfollowed(userId: string): boolean {
    return this._unfollowedIds().has(userId);
  }

  unfollow(user: UserProfile): void {
    if (this.isFollowLoading(user.id)) return;
    this._followLoading.update(s => new Set([...s, user.id]));

    this._userApi.unfollowUser(user.id).subscribe({
      next: () => {
        this._followLoading.update(s => { const n = new Set(s); n.delete(user.id); return n; });
        this._unfollowedIds.update(s => new Set([...s, user.id]));
      },
      error: () => {
        this._followLoading.update(s => { const n = new Set(s); n.delete(user.id); return n; });
      }
    });
  }

  follow(user: UserProfile): void {
    if (this.isFollowLoading(user.id)) return;
    this._followLoading.update(s => new Set([...s, user.id]));

    this._userApi.followUser(user.id).subscribe({
      next: () => {
        this._followLoading.update(s => { const n = new Set(s); n.delete(user.id); return n; });
        this._unfollowedIds.update(s => { const n = new Set(s); n.delete(user.id); return n; });
      },
      error: () => {
        this._followLoading.update(s => { const n = new Set(s); n.delete(user.id); return n; });
      }
    });
  }

  // ── 6. Navigation ────────────────────────────────────────────
  navigateToProfile(userId: string): void {
    this._router.navigate(['/profile', userId]);
  }

  getUserInitials(user: UserProfile): string {
    return `${user.firstName.charAt(0)}${user.lastName.charAt(0)}`.toUpperCase();
  }
}
