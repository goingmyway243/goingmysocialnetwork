import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { PostApiService } from '../../../services/post-api.service';
import { AuthService } from '../../../services/auth.service';
import { ComposePostComponent } from '../../../components/compose-post/compose-post.component';
import { PostCardComponent } from '../../../components/post-card/post-card.component';
import { Post, Comment, PostCommentsState } from '../../../models/post.model';

@Component({
  selector: 'app-dashboard-home',
  imports: [CommonModule, CardModule, ButtonModule, SkeletonModule, ToastModule, ComposePostComponent, PostCardComponent],
  providers: [MessageService],
  templateUrl: './dashboard-home.component.html',
  styleUrl: './dashboard-home.component.css'
})
export class DashboardHomeComponent implements OnInit {

  // ── 1. Dependencies ─────────────────────────────────────────
  private readonly _postApi = inject(PostApiService);
  private readonly _authService = inject(AuthService);
  private readonly _router = inject(Router);
  private readonly _messageService = inject(MessageService);

  // ── 2. State ─────────────────────────────────────────────────
  readonly posts = signal<Post[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  /** Post IDs liked by the current user (populated lazily on like action) */
  private readonly _likedPostIds = signal<Set<string>>(new Set());

  /** Per-post comment state keyed by post ID */
  private readonly _commentStates = signal<Map<string, PostCommentsState>>(new Map());

  // ── 3. Derived State ─────────────────────────────────────────
  readonly currentUserId = computed(() => this._authService.getCurrentUserId());

  // ── 4. Lifecycle ─────────────────────────────────────────────
  ngOnInit(): void {
    this.loadPosts();
  }

  // ── 5. Actions — Feed ─────────────────────────────────────────
  loadPosts(): void {
    this.loading.set(true);
    this.error.set(null);
    this._postApi.getPosts().subscribe({
      next: (data) => {
        const raw = Array.isArray((data as any).posts) ? (data as any).posts : (Array.isArray(data) ? data : []);
        const posts: Post[] = (raw as Post[]).map(p => ({ ...p, likes: p.likes ?? 0, comments: p.comments ?? 0 }));
        this.posts.set(posts);
        this.loading.set(false);
        const map = new Map<string, PostCommentsState>();
        posts.forEach(p => map.set(p.id, { expanded: false, loading: false, comments: [], newComment: '', submitting: false }));
        this._commentStates.set(map);
      },
      error: () => {
        this.error.set('Failed to load posts. Please try again later.');
        this.loading.set(false);
      }
    });
  }

  onPostCreated(post: Post): void {
    const normalized: Post = { ...post, likes: 0, comments: 0 };
    this.posts.update(prev => [normalized, ...prev]);
    this._commentStates.update(map => {
      const next = new Map(map);
      next.set(post.id, { expanded: false, loading: false, comments: [], newComment: '', submitting: false });
      return next;
    });
    this._messageService.add({ severity: 'success', summary: 'Posted!', detail: 'Your post has been shared.' });
  }

  // ── 6. Actions — Likes ────────────────────────────────────────
  isLiked(postId: string): boolean {
    return this._likedPostIds().has(postId);
  }

  toggleLike(post: Post): void {
    const wasLiked = this.isLiked(post.id);
    const prevCount = post.likes ?? 0;

    this._likedPostIds.update(set => {
      const next = new Set(set);
      wasLiked ? next.delete(post.id) : next.add(post.id);
      return next;
    });
    this.posts.update(prev => prev.map(p =>
      p.id === post.id ? { ...p, likes: wasLiked ? Math.max(0, prevCount - 1) : prevCount + 1 } : p
    ));

    const action$: Observable<unknown> = wasLiked ? this._postApi.unlikePost(post.id) : this._postApi.likePost(post.id);
    action$.subscribe({
      error: () => {
        this._likedPostIds.update(set => {
          const next = new Set(set);
          wasLiked ? next.add(post.id) : next.delete(post.id);
          return next;
        });
        this.posts.update(prev => prev.map(p =>
          p.id === post.id ? { ...p, likes: prevCount } : p
        ));
        this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to update like.' });
      }
    });
  }

  // ── 7. Actions — Comments ─────────────────────────────────────
  getCommentState(postId: string): PostCommentsState {
    return this._commentStates().get(postId) ?? { expanded: false, loading: false, comments: [], newComment: '', submitting: false };
  }

  private _updateCommentState(postId: string, patch: Partial<PostCommentsState>): void {
    this._commentStates.update(map => {
      const next = new Map(map);
      const existing = next.get(postId) ?? { expanded: false, loading: false, comments: [], newComment: '', submitting: false };
      next.set(postId, { ...existing, ...patch });
      return next;
    });
  }

  toggleComments(post: Post): void {
    const state = this.getCommentState(post.id);
    const willExpand = !state.expanded;
    this._updateCommentState(post.id, { expanded: willExpand });

    if (willExpand && state.comments.length === 0) {
      this._updateCommentState(post.id, { loading: true });
      this._postApi.getComments(post.id).subscribe({
        next: (comments) => this._updateCommentState(post.id, { comments, loading: false }),
        error: () => {
          this._updateCommentState(post.id, { loading: false });
          this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load comments.' });
        }
      });
    }
  }

  onNewCommentChange(postId: string, value: string): void {
    this._updateCommentState(postId, { newComment: value });
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
    this.posts.update(prev => prev.map(p =>
      p.id === post.id ? { ...p, comments: (p.comments ?? 0) + 1 } : p
    ));

    this._postApi.addComment(post.id, content).subscribe({
      next: (saved) => {
        const current = this.getCommentState(post.id);
        this._updateCommentState(post.id, {
          comments: current.comments.map(c => c.id === optimistic.id ? saved : c),
          submitting: false
        });
      },
      error: () => {
        const current = this.getCommentState(post.id);
        this._updateCommentState(post.id, {
          comments: current.comments.filter(c => c.id !== optimistic.id),
          newComment: content,
          submitting: false
        });
        this.posts.update(prev => prev.map(p =>
          p.id === post.id ? { ...p, comments: Math.max(0, (p.comments ?? 0) - 1) } : p
        ));
        this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to post comment.' });
      }
    });
  }

  // ── 8. Navigation ─────────────────────────────────────────────
  viewPostDetail(postId: string): void {
    this._router.navigate(['/posts', postId]);
  }

  // ── 9. Utilities ─────────────────────────────────────────────
  formatDate(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);
    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffHours < 24) return `${diffHours}h ago`;
    if (diffDays < 7) return `${diffDays}d ago`;
    return date.toLocaleDateString();
  }
}
