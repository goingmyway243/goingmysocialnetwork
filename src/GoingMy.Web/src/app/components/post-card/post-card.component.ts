import { Component, input, output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { TextareaModule } from 'primeng/textarea';
import { TooltipModule } from 'primeng/tooltip';
import { MenuModule } from 'primeng/menu';
import { MenuItem } from 'primeng/api';
import { Post, PostCommentsState } from '../../models/post.model';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-post-card',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, CardModule, ButtonModule, SkeletonModule, TextareaModule, TooltipModule, MenuModule],
  templateUrl: './post-card.component.html',
  styleUrl: './post-card.component.css'
})
export class PostCardComponent {

  // ── 0. Dependencies ───────────────────────────────────────────
  private readonly _authService = inject(AuthService);

  // ── 1. Inputs ─────────────────────────────────────────────────
  readonly post = input.required<Post>();
  readonly commentState = input.required<PostCommentsState>();
  readonly liked = input<boolean>(false);

  // ── 2. Outputs ─────────────────────────────────────────────────
  readonly likeToggle = output<Post>();
  readonly commentsToggle = output<Post>();
  readonly newCommentInput = output<{ postId: string; value: string }>();
  readonly commentSubmit = output<Post>();
  readonly detailView = output<string>();
  readonly deletePost = output<Post>();
  readonly editPost = output<Post>();
  readonly authorClick = output<string>();

  // ── 3. Utilities ─────────────────────────────────────────────────
  isPostOwner(): boolean {
    const currentUserId = this._authService.getCurrentUserId();
    return currentUserId === this.post().userId;
  }

  getPostMenuItems(): MenuItem[] {
    if (!this.isPostOwner()) return [];
    return [
      {
        label: 'Edit',
        icon: 'pi pi-pencil',
        command: () => this.onEditPost()
      },
      {
        label: 'Delete',
        icon: 'pi pi-trash',
        styleClass: 'p-menuitem-danger',
        command: () => this.onDeletePost()
      }
    ];
  }

  // ── 3. Actions ───────────────────────────────────────────────
  onLikeClick(): void {
    this.likeToggle.emit(this.post());
  }

  onCommentsClick(): void {
    this.commentsToggle.emit(this.post());
  }

  onCommentInput(value: string): void {
    this.newCommentInput.emit({ postId: this.post().id, value });
  }

  onCommentSubmit(): void {
    this.commentSubmit.emit(this.post());
  }

  onViewDetail(): void {
    this.detailView.emit(this.post().id);
  }

  onDeletePost(): void {
    this.deletePost.emit(this.post());
  }

  onEditPost(): void {
    this.editPost.emit(this.post());
  }

  onAuthorClick(): void {
    const userId = this.post().author?.id ?? this.post().userId;
    if (userId) this.authorClick.emit(userId);
  }

  // ── 4. Utilities ─────────────────────────────────────────────
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
