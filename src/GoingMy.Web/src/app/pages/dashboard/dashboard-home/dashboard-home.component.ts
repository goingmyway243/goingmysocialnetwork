import { Component, OnInit, inject, signal, computed, HostListener } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TextareaModule } from 'primeng/textarea';
import { ConfirmationService, MessageService } from 'primeng/api';
import { PostApiService } from '../../../services/post-api.service';
import { AuthService } from '../../../services/auth.service';
import { ComposePostComponent } from '../../../components/compose-post/compose-post.component';
import { PostCardComponent } from '../../../components/post-card/post-card.component';
import { EmptyStateComponent } from '../../../components/empty-state/empty-state.component';
import { Post, Comment, PostCommentsState } from '../../../models/post.model';

@Component({
  selector: 'app-dashboard-home',
  imports: [CommonModule, FormsModule, CardModule, ButtonModule, SkeletonModule, DialogModule, ConfirmDialogModule, TextareaModule, ComposePostComponent, PostCardComponent, EmptyStateComponent],
  providers: [ConfirmationService],
  templateUrl: './dashboard-home.component.html',
  styleUrl: './dashboard-home.component.css'
})
export class DashboardHomeComponent implements OnInit {

  // ── 1. Dependencies ─────────────────────────────────────────
  private readonly _postApi = inject(PostApiService);
  private readonly _authService = inject(AuthService);
  private readonly _router = inject(Router);
  private readonly _messageService = inject(MessageService);
  private readonly _confirmationService = inject(ConfirmationService);

  // ── 2. State ─────────────────────────────────────────────────
  readonly posts = signal<Post[]>([]);
  readonly loading = signal(true);
  readonly loadingMore = signal(false);
  readonly error = signal<string | null>(null);
  readonly editingPost = signal<Post | null>(null);
  readonly editPostContent = signal('');
  readonly editPostSubmitting = signal(false);

  // ── Pagination State ──────────────────────────────────────────
  readonly pageNumber = signal(0);
  readonly pageSize = signal(20);
  readonly hasMore = signal(true);

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
    this.pageNumber.set(0);
    this.hasMore.set(true);

    this._postApi.getPostsPaginated(0, this.pageSize()).subscribe({
      next: (data) => {
        const raw = Array.isArray((data as any).posts) ? (data as any).posts : (Array.isArray(data) ? data : []);
        const posts: Post[] = (raw as Post[]).map(p => ({ ...p, likes: p.likes ?? 0, comments: p.comments ?? 0 }));
        this.posts.set(posts);
        this.hasMore.set(data.hasMore);
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

  /** Loads the next page of posts and appends to the list */
  loadMorePosts(): void {
    if (this.loadingMore() || !this.hasMore() || this.loading()) {
      return;
    }

    this.loadingMore.set(true);
    const nextPage = this.pageNumber() + 1;

    this._postApi.getPostsPaginated(nextPage, this.pageSize()).subscribe({
      next: (data) => {
        const raw = Array.isArray((data as any).posts) ? (data as any).posts : (Array.isArray(data) ? data : []);
        const newPosts: Post[] = (raw as Post[]).map(p => ({ ...p, likes: p.likes ?? 0, comments: p.comments ?? 0 }));
        
        // Append new posts to existing list
        this.posts.update(prev => [...prev, ...newPosts]);
        this.pageNumber.set(nextPage);
        this.hasMore.set(data.hasMore);
        this.loadingMore.set(false);

        // Initialize comment states for new posts
        this._commentStates.update(map => {
          const next = new Map(map);
          newPosts.forEach(p => {
            if (!next.has(p.id)) {
              next.set(p.id, { expanded: false, loading: false, comments: [], newComment: '', submitting: false });
            }
          });
          return next;
        });
      },
      error: () => {
        this.loadingMore.set(false);
        this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load more posts.' });
      }
    });
  }

  /** Detects scroll and loads more posts when near bottom */
  @HostListener('window:scroll', [])
  onWindowScroll(): void {
    // Check if user is near bottom of page
    const scrollPosition = window.innerHeight + window.scrollY;
    const bottomThreshold = document.documentElement.scrollHeight - 500; // Load when 500px from bottom

    if (scrollPosition >= bottomThreshold && this.hasMore() && !this.loadingMore() && !this.loading()) {
      this.loadMorePosts();
    }
  }

  onPostCreated(post: Post): void {
    const normalized: Post = { ...post, likes: 0, comments: 0 };
    // Only prepend if it's not a video post (will arrive via notification)
    // For now, we'll just prepend text posts
    this.posts.update(prev => [normalized, ...prev]);
    this._commentStates.update(map => {
      const next = new Map(map);
      next.set(post.id, { expanded: false, loading: false, comments: [], newComment: '', submitting: false });
      return next;
    });
  }

  // ── 6. Actions — Likes ────────────────────────────────────────
  isLiked(post: Post): boolean {
    return post.userHasLiked ?? false;
  }

  toggleLike(post: Post): void {
    const wasLiked = this.isLiked(post);
    const prevCount = post.likes ?? 0;

    // Optimistic update
    this.posts.update(prev => prev.map(p =>
      p.id === post.id
        ? { ...p, userHasLiked: !wasLiked, likes: wasLiked ? Math.max(0, prevCount - 1) : prevCount + 1 }
        : p
    ));

    if (wasLiked) {
      this._postApi.unlikePost(post.id).subscribe({
        error: () => this.revertLike(post, wasLiked, prevCount)
      });
    } else {
      this._postApi.likePost(post.id).subscribe({
        error: () => this.revertLike(post, wasLiked, prevCount)
      });
    }
  }

  private revertLike(post: Post, wasLiked: boolean, prevCount: number): void {
    this.posts.update(prev => prev.map(p =>
      p.id === post.id
        ? { ...p, userHasLiked: wasLiked, likes: prevCount }
        : p
    ));
    this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to update like.' });
  }

  editPost(post: Post): void {
    this.editPostContent.set(post.content);
    this.editingPost.set(post);
  }

  cancelEditPost(): void {
    this.editingPost.set(null);
    this.editPostContent.set('');
  }

  saveEditPost(): void {
    const post = this.editingPost();
    if (!post || this.editPostSubmitting()) return;

    const content = this.editPostContent().trim();
    if (!content || content === post.content) {
      this.cancelEditPost();
      return;
    }

    this.editPostSubmitting.set(true);
    this._postApi.updatePost(post.id, { content }).subscribe({
      next: (res) => {
        this.posts.update(prev => prev.map(p => p.id === post.id ? { ...p, content: res.post.content } : p));
        this.editingPost.set(null);
        this.editPostContent.set('');
        this.editPostSubmitting.set(false);
        this._messageService.add({ severity: 'success', summary: 'Saved', detail: 'Post updated successfully.' });
      },
      error: () => {
        this.editPostSubmitting.set(false);
        this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to update post.' });
      }
    });
  }

  deletePost(post: Post): void {
    this._confirmationService.confirm({
      message: 'Are you sure you want to delete this post? This action cannot be undone.',
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Delete',
      rejectLabel: 'Cancel',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this._postApi.deletePost(post.id).subscribe({
          next: () => {
            this.posts.update(prev => prev.filter(p => p.id !== post.id));
            this._commentStates.update(map => {
              const next = new Map(map);
              next.delete(post.id);
              return next;
            });
            this._messageService.add({ severity: 'success', summary: 'Deleted', detail: 'Post deleted successfully.' });
          },
          error: () => {
            this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete post.' });
          }
        });
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

  navigateToProfile(userId: string): void {
    this._router.navigate(['/dashboard/profile', userId]);
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
