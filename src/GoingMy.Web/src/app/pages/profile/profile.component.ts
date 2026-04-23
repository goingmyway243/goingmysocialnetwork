import { Component, OnInit, OnDestroy, inject, signal, computed } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Observable, Subscription } from 'rxjs';
import { TabsModule } from 'primeng/tabs';
import { SkeletonModule } from 'primeng/skeleton';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { UserProfileService } from '../../services/user-profile.service';
import { PostApiService } from '../../services/post-api.service';
import { AuthService } from '../../services/auth.service';
import { DashboardHeaderComponent } from '../../components/dashboard-header/dashboard-header.component';
import { ProfileHeaderComponent } from '../../components/profile-header/profile-header.component';
import { ProfileAboutComponent } from '../../components/profile-about/profile-about.component';
import { UserListModalComponent } from '../../components/followers-modal/followers-modal.component';
import { EditProfileModalComponent } from '../../components/edit-profile-modal/edit-profile-modal.component';
import { PostCardComponent } from '../../components/post-card/post-card.component';
import { ComposePostComponent } from '../../components/compose-post/compose-post.component';
import { EmptyStateComponent } from '../../components/empty-state/empty-state.component';
import { FollowListComponent } from '../../components/follow-list/follow-list.component';
import { Post, PostCommentsState } from '../../models/post.model';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    TabsModule,
    SkeletonModule,
    ToastModule,
    DashboardHeaderComponent,
    ProfileHeaderComponent,
    ProfileAboutComponent,
    UserListModalComponent,
    EditProfileModalComponent,
    PostCardComponent,
    ComposePostComponent,
    EmptyStateComponent,
    FollowListComponent
  ],
  providers: [MessageService],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit, OnDestroy {

  // ── 1. Dependencies ─────────────────────────────────────────
  private readonly _profileService = inject(UserProfileService);
  private readonly _postApi = inject(PostApiService);
  private readonly _authService = inject(AuthService);
  private readonly _route = inject(ActivatedRoute);
  private readonly _router = inject(Router);
  private readonly _messageService = inject(MessageService);

  // ── 2. State ─────────────────────────────────────────────────
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly activeTab = signal('0');  // Use string values for PrimeNG v20

  readonly showFollowersModal = signal(false);
  readonly showFollowingModal = signal(false);
  readonly showEditModal = signal(false);

  private readonly _likedPostIds = signal<Set<string>>(new Set());
  private readonly _commentStates = signal<Map<string, PostCommentsState>>(new Map());

  private _userId = '';
  private _sub?: Subscription;

  // ── 3. Derived State ─────────────────────────────────────────
  readonly profile = computed(() => this._profileService.profile());
  readonly profilePosts = computed(() => this._profileService.profilePosts());
  readonly profileLikes = computed(() => this._profileService.profileLikes());
  readonly followers = computed(() => this._profileService.followers());
  readonly following = computed(() => this._profileService.following());
  readonly isOwnProfile = computed(() => this._profileService.isOwnProfile());
  readonly isFollowing = computed(() => this._profileService.isFollowing());
  readonly postsLoading = computed(() => this._profileService.postsLoading());
  readonly likesLoading = computed(() => this._profileService.likesLoading());
  readonly followersLoading = computed(() => this._profileService.followersLoading());
  readonly followingLoading = computed(() => this._profileService.followingLoading());
  readonly currentUserId = computed(() => this._authService.getCurrentUserId());

  // ── 4. Lifecycle ─────────────────────────────────────────────
  ngOnInit(): void {
    this._sub = this._route.paramMap.subscribe(params => {
      const userId = params.get('userId');
      if (userId) {
        this._userId = userId;
        this.initProfile(userId);
      }
    });
  }

  ngOnDestroy(): void {
    this._sub?.unsubscribe();
    this._profileService.clearProfile();
  }

  private initProfile(userId: string): void {
    this.loading.set(true);
    this.error.set(null);
    this._profileService.clearProfile();
    this.activeTab.set('0');

    this._profileService.loadProfile(userId).subscribe({
      next: (p) => {
        this.loading.set(false);
        this.loadPostsTab();

        // Check follow status for non-own profiles
        const currentId = this._authService.getCurrentUserId();
        if (currentId && currentId !== p.id) {
          // Check if current user is following — by loading followers and checking presence
          // Simplified: rely on backend for follow status check during first load
          // Full implementation could add a dedicated isFollowing endpoint
        }
      },
      error: () => {
        this.error.set('Could not load this profile. Please try again later.');
        this.loading.set(false);
      }
    });
  }

  // ── 5. Tab Management ────────────────────────────────────────
  onActiveTabChange(event: any): void {
    const value = event?.value || event;
    this.activeTab.set(value);
    // Lazy load posts/likes only when tabs are activated
    if (value === '0' && this.profilePosts().length === 0) {
      this.loadPostsTab();
    } else if (value === '1' && this.profileLikes().length === 0) {
      this.loadLikesTab();
    }
  }

  private loadPostsTab(): void {
    if (this._userId) {
      this._profileService.loadProfilePosts(this._userId).subscribe({
        next: (posts) => this.initCommentStates(posts)
      });
    }
  }

  private loadLikesTab(): void {
    if (this._userId) {
      this._profileService.loadProfileLikes(this._userId).subscribe({
        next: (posts) => this.initCommentStates(posts)
      });
    }
  }

  private initCommentStates(posts: Post[]): void {
    this._commentStates.update(map => {
      const next = new Map(map);
      posts.forEach(p => {
        if (!next.has(p.id)) {
          next.set(p.id, { expanded: false, loading: false, comments: [], newComment: '', submitting: false });
        }
      });
      return next;
    });
  }

  // ── 6. Follow / Unfollow ─────────────────────────────────────
  onFollowToggle(): void {
    if (this.isFollowing()) {
      this._profileService.unfollowUser(this._userId).subscribe();
    } else {
      this._profileService.followUser(this._userId).subscribe();
    }
  }

  // ── 7. Followers / Following Modals ──────────────────────────
  onFollowersClick(): void {
    this._profileService.loadFollowers(this._userId).subscribe();
    this.showFollowersModal.set(true);
  }

  onFollowingClick(): void {
    this._profileService.loadFollowing(this._userId).subscribe();
    this.showFollowingModal.set(true);
  }

  onUserSelected(userId: string): void {
    this._router.navigate(['/profile', userId]);
  }

  // ── 8. Edit Profile Modal ────────────────────────────────────
  onEditClick(): void {
    this.showEditModal.set(true);
  }

  onProfileUpdated(): void {
    this._messageService.add({ severity: 'success', summary: 'Saved!', detail: 'Your profile has been updated.' });
  }

  // ── 9. Post Interactions ─────────────────────────────────────
  getCommentState(postId: string): PostCommentsState {
    return this._commentStates().get(postId) ?? { expanded: false, loading: false, comments: [], newComment: '', submitting: false };
  }

  isLiked(postId: string): boolean {
    return this._likedPostIds().has(postId);
  }

  onLikeToggle(post: Post): void {
    const wasLiked = this.isLiked(post.id);

    this._likedPostIds.update(set => {
      const next = new Set(set);
      wasLiked ? next.delete(post.id) : next.add(post.id);
      return next;
    });

    const delta = wasLiked ? -1 : 1;
    this._profileService.updatePostLike(post.id, delta);

    const action$ = (wasLiked ? this._postApi.unlikePost(post.id) : this._postApi.likePost(post.id)) as Observable<unknown>;
    action$.subscribe({
      error: () => {
        // Revert on error
        this._likedPostIds.update(set => { const next = new Set(set); wasLiked ? next.add(post.id) : next.delete(post.id); return next; });
        this._profileService.updatePostLike(post.id, wasLiked ? 1 : -1);
      }
    });
  }

  onCommentsToggle(post: Post): void {
    this._commentStates.update(map => {
      const next = new Map(map);
      const state = next.get(post.id) ?? { expanded: false, loading: false, comments: [], newComment: '', submitting: false };
      if (!state.expanded && state.comments.length === 0) {
        next.set(post.id, { ...state, expanded: true, loading: true });
        this._postApi.getComments(post.id).subscribe({
          next: (comments) => this._commentStates.update(m => {
            const n = new Map(m);
            const s = n.get(post.id)!;
            n.set(post.id, { ...s, comments, loading: false });
            return n;
          })
        });
      } else {
        next.set(post.id, { ...state, expanded: !state.expanded });
      }
      return next;
    });
  }

  onCommentInput(event: { postId: string; value: string }): void {
    this._commentStates.update(map => {
      const next = new Map(map);
      const state = next.get(event.postId);
      if (state) next.set(event.postId, { ...state, newComment: event.value });
      return next;
    });
  }

  onCommentSubmit(post: Post): void {
    const state = this.getCommentState(post.id);
    if (!state.newComment.trim() || state.submitting) return;

    this._commentStates.update(map => {
      const next = new Map(map);
      next.set(post.id, { ...state, submitting: true });
      return next;
    });

    this._postApi.addComment(post.id, state.newComment.trim()).subscribe({
      next: (comment) => {
        this._commentStates.update(map => {
          const next = new Map(map);
          const s = next.get(post.id)!;
          next.set(post.id, { ...s, comments: [...s.comments, comment], newComment: '', submitting: false });
          return next;
        });
      },
      error: () => {
        this._commentStates.update(map => {
          const next = new Map(map);
          const s = next.get(post.id)!;
          next.set(post.id, { ...s, submitting: false });
          return next;
        });
      }
    });
  }

  onDetailView(postId: string): void {
    this._router.navigate(['/posts', postId]);
  }

  onPostCreated(post: Post): void {
    this._profileService.prependPost(post);
    this._messageService.add({ severity: 'success', summary: 'Posted!', detail: 'Your post has been published.' });
  }
}
