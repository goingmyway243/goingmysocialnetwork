import { Component, output, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TextareaModule } from 'primeng/textarea';
import { InputTextModule } from 'primeng/inputtext';
import { PostApiService } from '../../services/post-api.service';
import { Post } from '../../models/post.model';

@Component({
  selector: 'app-compose-post',
  imports: [FormsModule, CardModule, ButtonModule, TextareaModule, InputTextModule],
  templateUrl: './compose-post.component.html',
  styleUrl: './compose-post.component.css'
})
export class ComposePostComponent {

  // ── 1. Dependencies ─────────────────────────────────────────
  private readonly _postApi = inject(PostApiService);

  // ── 2. Outputs ───────────────────────────────────────────────
  readonly postCreated = output<Post>();

  // ── 3. State ─────────────────────────────────────────────────
  readonly title = signal('');
  readonly content = signal('');
  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);

  // ── 4. Derived State ─────────────────────────────────────────
  readonly isValid = computed(() => this.title().trim().length > 0 && this.content().trim().length > 0);
  readonly contentLength = computed(() => this.content().length);
  readonly titleLength = computed(() => this.title().length);
  readonly contentTooLong = computed(() => this.contentLength() > 2000);
  readonly titleTooLong = computed(() => this.titleLength() > 200);
  readonly canSubmit = computed(() => this.isValid() && !this.contentTooLong() && !this.titleTooLong() && !this.submitting());

  // ── 5. Actions ───────────────────────────────────────────────
  submit(): void {
    if (!this.canSubmit()) return;

    this.submitting.set(true);
    this.error.set(null);

    this._postApi.createPost({ title: this.title().trim(), content: this.content().trim() }).subscribe({
      next: (response) => {
        this.postCreated.emit(response.post);
        this.title.set('');
        this.content.set('');
        this.submitting.set(false);
      },
      error: () => {
        this.error.set('Failed to create post. Please try again.');
        this.submitting.set(false);
      }
    });
  }

  onTitleChange(value: string): void {
    this.title.set(value);
  }

  onContentChange(value: string): void {
    this.content.set(value);
  }
}
