import { Component, output, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { TextareaModule } from 'primeng/textarea';
import { InputTextModule } from 'primeng/inputtext';
import { PostApiService } from '../../services/post-api.service';
import { Post } from '../../models/post.model';

@Component({
  selector: 'app-compose-post',
  imports: [FormsModule, ButtonModule, DialogModule, TextareaModule, InputTextModule],
  templateUrl: './compose-post.component.html',
  styleUrl: './compose-post.component.css'
})
export class ComposePostComponent {

  // ── 1. Dependencies ─────────────────────────────────────────
  private readonly _postApi = inject(PostApiService);

  // ── 2. Outputs ───────────────────────────────────────────────
  readonly postCreated = output<Post>();

  // ── 3. State ─────────────────────────────────────────────────
  readonly dialogVisible = signal(false);
  readonly content = signal('');
  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);

  // ── 4. Derived State ─────────────────────────────────────────────────
  readonly isValid = computed(() => this.content().trim().length > 0);
  readonly contentLength = computed(() => this.content().length);
  readonly contentTooLong = computed(() => this.contentLength() > 2000);
  readonly canSubmit = computed(() => this.isValid() && !this.contentTooLong() && !this.submitting());

  // ── 5. Actions ───────────────────────────────────────────────
  openDialog(): void {
    this.dialogVisible.set(true);
  }

  closeDialog(): void {
    this.dialogVisible.set(false);
    this.content.set('');
    this.error.set(null);
  }

  submit(): void {
    if (!this.canSubmit()) return;

    this.submitting.set(true);
    this.error.set(null);

    this._postApi.createPost({ content: this.content().trim() }).subscribe({
      next: (response) => {
        this.postCreated.emit(response.post);
        this.submitting.set(false);
        this.closeDialog();
      },
      error: () => {
        this.error.set('Failed to create post. Please try again.');
        this.submitting.set(false);
      }
    });
  }

  onContentChange(value: string): void {
    this.content.set(value);
  }
}
