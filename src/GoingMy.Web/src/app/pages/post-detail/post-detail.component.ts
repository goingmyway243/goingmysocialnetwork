import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { TextareaModule } from 'primeng/textarea';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageService, ConfirmationService } from 'primeng/api';
import { PostApiService } from '../../services/post-api.service';
import { AuthService } from '../../services/auth.service';
import { Post, Comment } from '../../models/post.model';

@Component({
  selector: 'app-post-detail',
  imports: [CommonModule, FormsModule, CardModule, ButtonModule, SkeletonModule, TextareaModule, ToastModule, ConfirmDialogModule],
  providers: [MessageService, ConfirmationService],
  templateUrl: './post-detail.component.html',
  styleUrl: './post-detail.component.css'
})
export class PostDetailComponent implements OnInit {

  // ── 1. Dependencies ─────────────────────────────────────────
  private readonly _route = inject(ActivatedRoute);
  private readonly _router = inject(Router);
  private readonly _postApi = inject(PostApiService);
  private readonly _authService = inject(AuthService);
  private readonly _messageService = inject(MessageService);
  private readonly _confirmationService = inject(ConfirmationService);

  // ── 2. State ─────────────────────────────────────────────────
  readonly post = signal<Post | null>(null);
  readonly comments = signal<Comment[]>([]);
  readonly loadingPost = signal(true);
  readonly loadingComments = signal(true);
  readonly liked = signal(false);
  readonly likeSubmitting = signal(false);
  readonly newComment = signal('');
  readonly commentSubmitting = signal(false);
  readonly editingCommentId = signal<string | null>(null);
  readonly editingContent = signal('');
  readonly editSubmitting = signal(false);

  // ── 3. Derived State ─────────────────────────────────────────
  readonly currentUserId = computed(() => this._authService.getCurrentUserId());
  readonly canSubmitComment = computed(() => this.newComment().trim().length > 0 && !this.commentSubmitting());

  // ── 4. Lifecycle ─────────────────────────────────────────────
  ngOnInit(): void {
    const id = this._route.snapshot.paramMap.get('id');
    if (!id) {
      this._router.navigate(['/dashboard/home']);
      return;
    }
    this.loadPost(id);
    this.loadComments(id);
    this.loadLikeStatus(id);
  }

  // ── 5. Actions — Post ─────────────────────────────────────────
  private loadPost(id: string): void {
    this.loadingPost.set(true);
    this._postApi.getPostById(id).subscribe({
      next: (post) => {
        this.post.set(post);
        this.loadingPost.set(false);
      },
      error: () => {
        this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load post.' });
        this.loadingPost.set(false);
      }
    });
  }

  private loadComments(postId: string): void {
    this.loadingComments.set(true);
    this._postApi.getComments(postId).subscribe({
      next: (comments) => {
        this.comments.set(comments);
        this.loadingComments.set(false);
      },
      error: () => {
        this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load comments.' });
        this.loadingComments.set(false);
      }
    });
  }

  private loadLikeStatus(postId: string): void {
    this._postApi.getPostLikes(postId).subscribe({
      next: (likes) => {
        const uid = this.currentUserId();
        this.liked.set(likes.some(l => l.userId === uid));
      },
      error: () => { /* non-critical, ignore */ }
    });
  }

  // ── 6. Actions — Likes ────────────────────────────────────────
  toggleLike(): void {
    const p = this.post();
    if (!p || this.likeSubmitting()) return;

    const wasLiked = this.liked();
    const prevCount = p.likes ?? 0;

    // Optimistic update
    this.liked.set(!wasLiked);
    this.post.set({ ...p, likes: wasLiked ? Math.max(0, prevCount - 1) : prevCount + 1 });
    this.likeSubmitting.set(true);

    const action$: Observable<unknown> = wasLiked ? this._postApi.unlikePost(p.id) : this._postApi.likePost(p.id);
    action$.subscribe({
      next: () => this.likeSubmitting.set(false),
      error: () => {
        // Revert optimistic update
        this.liked.set(wasLiked);
        this.post.set({ ...p, likes: prevCount });
        this.likeSubmitting.set(false);
        this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to update like.' });
      }
    });
  }

  // ── 7. Actions — Comments ─────────────────────────────────────
  submitComment(): void {
    const p = this.post();
    if (!p || !this.canSubmitComment()) return;

    const content = this.newComment().trim();
    this.commentSubmitting.set(true);

    // Optimistic comment
    const optimistic: Comment = {
      id: `temp-${Date.now()}`,
      postId: p.id,
      userId: this.currentUserId(),
      username: this._authService.getCurrentUsername(),
      content,
      createdAt: new Date().toISOString()
    };
    this.comments.update(prev => [optimistic, ...prev]);
    this.post.set({ ...p, comments: (p.comments ?? 0) + 1 });
    this.newComment.set('');

    this._postApi.addComment(p.id, content).subscribe({
      next: (saved) => {
        // Replace the optimistic comment with the real one
        this.comments.update(prev => prev.map(c => c.id === optimistic.id ? saved : c));
        this.commentSubmitting.set(false);
      },
      error: () => {
        // Revert optimistic comment
        this.comments.update(prev => prev.filter(c => c.id !== optimistic.id));
        this.post.set({ ...p, comments: p.comments ?? 0 });
        this.newComment.set(content);
        this.commentSubmitting.set(false);
        this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to add comment.' });
      }
    });
  }

  startEditComment(comment: Comment): void {
    this.editingCommentId.set(comment.id);
    this.editingContent.set(comment.content);
  }

  cancelEditComment(): void {
    this.editingCommentId.set(null);
    this.editingContent.set('');
  }

  saveEditComment(comment: Comment): void {
    const p = this.post();
    if (!p || this.editSubmitting()) return;

    const newContent = this.editingContent().trim();
    if (!newContent || newContent === comment.content) {
      this.cancelEditComment();
      return;
    }

    this.editSubmitting.set(true);
    this._postApi.updateComment(p.id, comment.id, newContent).subscribe({
      next: (updated) => {
        this.comments.update(prev => prev.map(c => c.id === comment.id ? updated : c));
        this.editingCommentId.set(null);
        this.editSubmitting.set(false);
      },
      error: () => {
        this.editSubmitting.set(false);
        this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to update comment.' });
      }
    });
  }

  confirmDeleteComment(comment: Comment, event: Event): void {
    this._confirmationService.confirm({
      target: event.target as EventTarget,
      message: 'Delete this comment?',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Delete',
      rejectLabel: 'Cancel',
      acceptButtonStyleClass: 'p-button-danger p-button-sm',
      accept: () => this.deleteComment(comment)
    });
  }

  private deleteComment(comment: Comment): void {
    const p = this.post();
    if (!p) return;

    // Optimistic remove
    this.comments.update(prev => prev.filter(c => c.id !== comment.id));
    this.post.set({ ...p, comments: Math.max(0, (p.comments ?? 0) - 1) });

    this._postApi.deleteComment(p.id, comment.id).subscribe({
      error: () => {
        // Revert
        this.comments.update(prev => [comment, ...prev].sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()));
        this.post.set({ ...p, comments: p.comments ?? 0 });
        this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete comment.' });
      }
    });
  }

  onEditContentChange(value: string): void {
    this.editingContent.set(value);
  }

  onNewCommentChange(value: string): void {
    this.newComment.set(value);
  }

  // ── 8. Navigation ─────────────────────────────────────────────
  goBack(): void {
    this._router.navigate(['/dashboard/home']);
  }

  // ── 9. Utilities ───────────────────────────────────────────────
  isOwnComment(comment: Comment): boolean {
    return comment.userId === this.currentUserId();
  }

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
