import { Component, OnInit, OnDestroy, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { debounceTime, distinctUntilChanged, Subject, takeUntil, switchMap, of } from 'rxjs';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { ToastModule } from 'primeng/toast';
import { TabsModule } from 'primeng/tabs';
import { SelectButtonModule } from 'primeng/selectbutton';
import { AvatarModule } from 'primeng/avatar';
import { MessageService } from 'primeng/api';
import { EmptyStateComponent } from '../../components/empty-state/empty-state.component';
import { PostCardComponent } from '../../components/post-card/post-card.component';
import { SearchApiService } from '../../services/search-api.service';
import { UserApiService } from '../../services/user-api.service';
import { PostApiService } from '../../services/post-api.service';
import { AuthService } from '../../services/auth.service';
import {
  UserSearchResult,
  PostSearchResult,
  TrendingPost,
  SuggestionResult,
  TimeWindow
} from '../../models/search.models';
import { Post, Comment, PostCommentsState } from '../../models/post.model';

@Component({
  selector: 'app-discover',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    InputTextModule,
    ButtonModule,
    TabsModule,
    SelectButtonModule,
    SkeletonModule,
    ToastModule,
    AvatarModule,
    EmptyStateComponent,
    PostCardComponent
  ],
  templateUrl: './discover.component.html',
  styleUrl: './discover.component.css'
})
export class DiscoverComponent implements OnInit, OnDestroy {

  // ── 1. Dependencies ─────────────────────────────────────────
  private readonly _searchApi = inject(SearchApiService);
  private readonly _userApi = inject(UserApiService);
  private readonly _postApi = inject(PostApiService);
  private readonly _authService = inject(AuthService);
  private readonly _router = inject(Router);
  private readonly _activatedRoute = inject(ActivatedRoute);
  private readonly _messageService = inject(MessageService);
  private readonly _destroy$ = new Subject<void>();
  private readonly _suggestInput$ = new Subject<string>();

  // ── 2. State ────────────────────────────────────────────────
  readonly searchTerm = signal('');
  readonly locationFilter = signal('');
  readonly sortBy = signal<'relevance' | 'recent'>('relevance');
  readonly activeTab = signal<'users' | 'posts' | 'trending'>('users');
  readonly trendingWindow = signal<TimeWindow>('week');
  readonly hasSearched = signal(false);
  readonly isLoading = signal(false);
  readonly isTrendingLoading = signal(false);

  readonly userResults = signal<UserSearchResult[]>([]);
  readonly postResults = signal<Post[]>([]);
  readonly trendingPosts = signal<Post[]>([]);
  readonly suggestions = signal<SuggestionResult[]>([]);

  private readonly _commentStates = signal<Map<string, PostCommentsState>>(new Map());
  private readonly _followingIds = signal<Set<string>>(new Set());
  private readonly _followLoading = signal<Set<string>>(new Set());

  // ── 3. Derived State ─────────────────────────────────────────
  readonly currentUserId = computed(() => this._authService.getCurrentUserId());
  readonly hasUserResults = computed(() => this.userResults().length > 0);
  readonly hasPostResults = computed(() => this.postResults().length > 0);
  readonly hasTrending = computed(() => this.trendingPosts().length > 0);
  readonly isEmpty = computed(() =>
    this.hasSearched() && !this.isLoading() && !this.hasUserResults() && !this.hasPostResults()
  );

  readonly sortOptions = [
    { label: 'Relevance', value: 'relevance' },
    { label: 'Recent', value: 'recent' }
  ];
  readonly trendingWindowOptions = [
    { label: '24h', value: 'day' },
    { label: 'Week', value: 'week' },
    { label: 'Month', value: 'month' }
  ];

  // ── 4. Lifecycle ─────────────────────────────────────────────
  ngOnInit(): void {
    this._loadTrending();

    this._suggestInput$.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(q => q.length >= 2 ? this._searchApi.suggest(q) : of([])),
      takeUntil(this._destroy$)
    ).subscribe(results => this.suggestions.set(results));

    this._activatedRoute.queryParams.pipe(takeUntil(this._destroy$)).subscribe(params => {
      if (params['search']) {
        this.searchTerm.set(params['search']);
        this.onSearch();
      }
    });
  }

  ngOnDestroy(): void {
    this._destroy$.next();
    this._destroy$.complete();
  }

  // ── 5. Actions ───────────────────────────────────────────────
  onSearch(): void {
    this.hasSearched.set(true);
    this.isLoading.set(true);
    this.userResults.set([]);
    this.postResults.set([]);

    this._searchApi.searchAll({
      q: this.searchTerm().trim(),
      location: this.locationFilter() || undefined,
      sortBy: this.sortBy()
    }).pipe(takeUntil(this._destroy$)).subscribe({
      next: result => {
        this.userResults.set(result.users ?? []);
        const posts = (result.posts ?? []).map(p => this._mapSearchResultToPost(p));
        this.postResults.set(posts);
        this._initCommentStates(posts);
        this.isLoading.set(false);
        this.activeTab.set(
          (result.users?.length ?? 0) === 0 && (result.posts?.length ?? 0) > 0 ? 'posts' : 'users'
        );
        if ((result.users?.length ?? 0) > 0) {
          this._loadFollowingIds(result.users!.map(u => u.id));
        }
      },
      error: () => {
        this.isLoading.set(false);
        this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Search failed. Please try again.' });
      }
    });
  }

  onClearSearch(): void {
    this.searchTerm.set('');
    this.locationFilter.set('');
    this.hasSearched.set(false);
    this.userResults.set([]);
    this.postResults.set([]);
    this.suggestions.set([]);
  }

  onSearchKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter') this.onSearch();
  }

  onSearchInputChange(q: string): void {
    this.searchTerm.set(q);
    this._suggestInput$.next(q);
  }

  onTrendingWindowChange(window: TimeWindow): void {
    this.trendingWindow.set(window);
    this._loadTrending();
  }

  isFollowing(userId: string): boolean {
    return this._followingIds().has(userId);
  }

  isFollowLoading(userId: string): boolean {
    return this._followLoading().has(userId);
  }

  toggleFollow(user: UserSearchResult): void {
    if (this.isFollowLoading(user.id)) return;

    this._followLoading.update(s => new Set([...s, user.id]));
    const alreadyFollowing = this.isFollowing(user.id);

    const obs = alreadyFollowing
      ? this._userApi.unfollowUser(user.id)
      : this._userApi.followUser(user.id);

    obs.pipe(takeUntil(this._destroy$)).subscribe({
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
    this._router.navigate(['/dashboard/profile', userId]);
  }

  navigateToPost(postId: string): void {
    this._router.navigate(['/posts', postId]);
  }

  getUserInitials(user: UserSearchResult): string {
    return `${user.firstName?.charAt(0) ?? ''}${user.lastName?.charAt(0) ?? ''}`.toUpperCase();
  }

  formatRelativeTime(dateStr: string): string {
    const diff = Date.now() - new Date(dateStr).getTime();
    const mins = Math.floor(diff / 60000);
    if (mins < 1) return 'just now';
    if (mins < 60) return `${mins}m`;
    const hours = Math.floor(mins / 60);
    if (hours < 24) return `${hours}h`;
    return `${Math.floor(hours / 24)}d`;
  }

  // ── 7. PostCard Action Handlers ───────────────────────────────
  getCommentState(postId: string): PostCommentsState {
    return this._commentStates().get(postId) ?? { expanded: false, loading: false, comments: [], newComment: '', submitting: false };
  }

  toggleLike(post: Post): void {
    const wasLiked = post.userHasLiked ?? false;
    const prevCount = post.likes ?? 0;
    const update = (list: Post[]) => list.map(p =>
      p.id === post.id ? { ...p, userHasLiked: !wasLiked, likes: wasLiked ? Math.max(0, prevCount - 1) : prevCount + 1 } : p
    );
    this.postResults.update(update);
    this.trendingPosts.update(update);

    const obs = (wasLiked ? this._postApi.unlikePost(post.id) : this._postApi.likePost(post.id)) as import('rxjs').Observable<unknown>;
    obs.pipe(takeUntil(this._destroy$)).subscribe({
      error: () => {
        const revert = (list: Post[]) => list.map(p =>
          p.id === post.id ? { ...p, userHasLiked: wasLiked, likes: prevCount } : p
        );
        this.postResults.update(revert);
        this.trendingPosts.update(revert);
        this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to update like.' });
      }
    });
  }

  toggleComments(post: Post): void {
    const state = this.getCommentState(post.id);
    const willExpand = !state.expanded;
    this._updateCommentState(post.id, { expanded: willExpand });

    if (willExpand && state.comments.length === 0) {
      this._updateCommentState(post.id, { loading: true });
      this._postApi.getComments(post.id).pipe(takeUntil(this._destroy$)).subscribe({
        next: comments => this._updateCommentState(post.id, { comments, loading: false }),
        error: () => {
          this._updateCommentState(post.id, { loading: false });
          this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load comments.' });
        }
      });
    }
  }

  onNewCommentInput(event: { postId: string; value: string }): void {
    this._updateCommentState(event.postId, { newComment: event.value });
  }

  submitComment(post: Post): void {
    const state = this.getCommentState(post.id);
    const content = state.newComment.trim();
    if (!content || state.submitting) return;

    this._updateCommentState(post.id, { submitting: true });

    const optimistic: Comment = {
      id: `temp-${Date.now()}`,
      postId: post.id,
      userId: this.currentUserId(),
      username: this._authService.getCurrentUsername(),
      content,
      createdAt: new Date().toISOString()
    };

    this._updateCommentState(post.id, { comments: [optimistic, ...state.comments], newComment: '' });
    const updateCount = (list: Post[]) => list.map(p =>
      p.id === post.id ? { ...p, comments: (p.comments ?? 0) + 1 } : p
    );
    this.postResults.update(updateCount);
    this.trendingPosts.update(updateCount);

    this._postApi.addComment(post.id, content).pipe(takeUntil(this._destroy$)).subscribe({
      next: saved => {
        const cur = this.getCommentState(post.id);
        this._updateCommentState(post.id, { comments: cur.comments.map(c => c.id === optimistic.id ? saved : c), submitting: false });
      },
      error: () => {
        const cur = this.getCommentState(post.id);
        this._updateCommentState(post.id, { comments: cur.comments.filter(c => c.id !== optimistic.id), newComment: content, submitting: false });
        const revertCount = (list: Post[]) => list.map(p =>
          p.id === post.id ? { ...p, comments: Math.max(0, (p.comments ?? 0) - 1) } : p
        );
        this.postResults.update(revertCount);
        this.trendingPosts.update(revertCount);
        this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to post comment.' });
      }
    });
  }

  // ── Private ──────────────────────────────────────────────────
  private _initCommentStates(posts: Post[]): void {
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

  private _updateCommentState(postId: string, patch: Partial<PostCommentsState>): void {
    this._commentStates.update(map => {
      const next = new Map(map);
      const existing = next.get(postId) ?? { expanded: false, loading: false, comments: [], newComment: '', submitting: false };
      next.set(postId, { ...existing, ...patch });
      return next;
    });
  }

  private _mapSearchResultToPost(r: PostSearchResult): Post {
    return {
      id: r.id,
      content: r.content,
      userId: r.userId,
      username: r.username,
      createdAt: r.createdAt,
      likes: r.likes,
      comments: r.comments,
      mediaAttachments: r.mediaAttachments
    };
  }

  private _mapTrendingToPost(t: TrendingPost): Post {
    return {
      id: t.postId,
      content: t.content,
      userId: t.userId,
      username: t.username,
      createdAt: t.createdAt,
      likes: t.likes,
      comments: t.comments,
      mediaAttachments: t.mediaAttachments
    };
  }

  private _loadTrending(): void {
    this.isTrendingLoading.set(true);
    this._searchApi.getTrending(this.trendingWindow(), 10)
      .pipe(takeUntil(this._destroy$))
      .subscribe({
        next: posts => {
          const mapped = posts.map(p => this._mapTrendingToPost(p));
          this.trendingPosts.set(mapped);
          this._initCommentStates(mapped);
          this.isTrendingLoading.set(false);
        },
        error: () => this.isTrendingLoading.set(false)
      });
  }

  private _loadFollowingIds(userIds: string[]): void {
    this._userApi.getFollowingStatusBatch(userIds)
      .pipe(takeUntil(this._destroy$))
      .subscribe(ids => this._followingIds.set(new Set(ids)));
  }
}
