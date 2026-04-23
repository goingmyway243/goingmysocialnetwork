import { Component, OnInit, OnDestroy, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { ToggleButtonModule } from 'primeng/togglebutton';
import { SkeletonModule } from 'primeng/skeleton';
import { ToastModule } from 'primeng/toast';
import { TagModule } from 'primeng/tag';
import { AvatarModule } from 'primeng/avatar';
import { MessageService } from 'primeng/api';
import { DashboardHeaderComponent } from '../../components/dashboard-header/dashboard-header.component';
import { EmptyStateComponent } from '../../components/empty-state/empty-state.component';
import { UserProfileService } from '../../services/user-profile.service';
import { UserApiService } from '../../services/user-api.service';
import { AuthService } from '../../services/auth.service';
import { UserProfile } from '../../models/user.models';

@Component({
  selector: 'app-discover',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    InputTextModule,
    ButtonModule,
    ToggleButtonModule,
    SkeletonModule,
    ToastModule,
    TagModule,
    AvatarModule,
    DashboardHeaderComponent,
    EmptyStateComponent
  ],
  providers: [MessageService],
  templateUrl: './discover.component.html',
  styleUrl: './discover.component.css'
})
export class DiscoverComponent implements OnInit, OnDestroy {

  // ── 1. Dependencies ─────────────────────────────────────────
  private readonly _profileService = inject(UserProfileService);
  private readonly _userApi = inject(UserApiService);
  private readonly _authService = inject(AuthService);
  private readonly _router = inject(Router);
  private readonly _activatedRoute = inject(ActivatedRoute);
  private readonly _messageService = inject(MessageService);

  // ── 2. State ────────────────────────────────────────────────
  readonly searchTerm = signal('');
  readonly locationFilter = signal('');
  readonly verifiedOnly = signal(false);
  readonly hasSearched = signal(false);

  /** Track follow state per user by ID */
  private readonly _followingIds = signal<Set<string>>(new Set());
  private readonly _followLoading = signal<Set<string>>(new Set());

  // ── 3. Derived State ─────────────────────────────────────────
  readonly searchResults = computed(() => this._profileService.searchResults());
  readonly searchLoading = computed(() => this._profileService.searchLoading());
  readonly searchError = computed(() => this._profileService.searchError());
  readonly currentUserId = computed(() => this._authService.getCurrentUserId());
  readonly hasResults = computed(() => this.searchResults().length > 0);
  readonly isEmpty = computed(() => this.hasSearched() && !this.searchLoading() && !this.hasResults());

  // ── 4. Lifecycle ─────────────────────────────────────────────
  ngOnInit(): void {
    this._profileService.clearSearchResults();
    
    // Read query params and auto-execute search if search param exists
    this._activatedRoute.queryParams.subscribe(params => {
      const searchParam = params['search'];
      if (searchParam) {
        this.searchTerm.set(searchParam);
        this.hasSearched.set(true);
        this.onSearch();
      }
    });
  }

  ngOnDestroy(): void {
    this._profileService.clearSearchResults();
  }

  // ── 5. Actions ───────────────────────────────────────────────
  onSearch(): void {
    this.hasSearched.set(true);
    this._profileService.searchUsers(
      this.searchTerm() || undefined,
      this.locationFilter() || undefined,
      this.verifiedOnly() || undefined
    ).subscribe({
      error: () => this._messageService.add({
        severity: 'error', summary: 'Error', detail: 'Failed to search users'
      })
    });
  }

  onClearSearch(): void {
    this.searchTerm.set('');
    this.locationFilter.set('');
    this.verifiedOnly.set(false);
    this.hasSearched.set(false);
    this._profileService.clearSearchResults();
  }

  onSearchKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter') this.onSearch();
  }

  isFollowing(userId: string): boolean {
    return this._followingIds().has(userId);
  }

  isFollowLoading(userId: string): boolean {
    return this._followLoading().has(userId);
  }

  toggleFollow(user: UserProfile): void {
    if (this.isFollowLoading(user.id)) return;

    this._followLoading.update(s => new Set([...s, user.id]));
    const alreadyFollowing = this.isFollowing(user.id);

    const obs = alreadyFollowing
      ? this._userApi.unfollowUser(user.id)
      : this._userApi.followUser(user.id);

    obs.subscribe({
      next: () => {
        this._followingIds.update(s => {
          const next = new Set(s);
          alreadyFollowing ? next.delete(user.id) : next.add(user.id);
          return next;
        });
        this._followLoading.update(s => { const next = new Set(s); next.delete(user.id); return next; });
      },
      error: () => {
        this._followLoading.update(s => { const next = new Set(s); next.delete(user.id); return next; });
        this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Action failed. Please try again.' });
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
