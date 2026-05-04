import { Component, output, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { TextareaModule } from 'primeng/textarea';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { PostApiService } from '../../services/post-api.service';
import { AiApiService } from '../../services/ai-api.service';
import { Post } from '../../models/post.model';
import { AiAction } from '../../models/ai-assist.model';

const TONES = ['Casual', 'Professional', 'Funny', 'Inspirational'] as const;

@Component({
  selector: 'app-compose-post',
  imports: [FormsModule, ButtonModule, DialogModule, TextareaModule, InputTextModule, SelectModule],
  templateUrl: './compose-post.component.html',
  styleUrl: './compose-post.component.css'
})
export class ComposePostComponent {

  // ── 1. Dependencies ─────────────────────────────────────────
  private readonly _postApi = inject(PostApiService);
  private readonly _aiApi = inject(AiApiService);

  // ── 2. Outputs ───────────────────────────────────────────────
  readonly postCreated = output<Post>();

  // ── 3. State ─────────────────────────────────────────────────
  readonly dialogVisible = signal(false);
  readonly content = signal('');
  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);

  // ── AI State ──────────────────────────────────────────────────
  readonly aiPanelVisible = signal(false);
  readonly aiLoading = signal(false);
  readonly aiSuggestion = signal<string | null>(null);
  readonly aiError = signal<string | null>(null);
  selectedTone = TONES[0];
  readonly toneOptions = TONES.map(t => ({ label: t, value: t }));

  // ── 4. Derived State ─────────────────────────────────────────
  readonly isValid = computed(() => this.content().trim().length > 0);
  readonly contentLength = computed(() => this.content().length);
  readonly contentTooLong = computed(() => this.contentLength() > 2000);
  readonly canSubmit = computed(() => this.isValid() && !this.contentTooLong() && !this.submitting());
  readonly hasAiSuggestion = computed(() => this.aiSuggestion() !== null);

  // ── 5. Actions ───────────────────────────────────────────────
  openDialog(): void {
    this.dialogVisible.set(true);
  }

  closeDialog(): void {
    this.dialogVisible.set(false);
    this.content.set('');
    this.error.set(null);
    this.aiPanelVisible.set(false);
    this.aiSuggestion.set(null);
    this.aiError.set(null);
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

  // ── AI Actions ───────────────────────────────────────────────
  toggleAiPanel(): void {
    this.aiPanelVisible.update(v => !v);
    if (!this.aiPanelVisible()) {
      this.aiSuggestion.set(null);
      this.aiError.set(null);
    }
  }

  runAiAction(action: AiAction): void {
    this.aiLoading.set(true);
    this.aiSuggestion.set(null);
    this.aiError.set(null);

    this._aiApi.assist({
      action,
      content: this.content() || undefined,
      tone: action === 'tone' ? this.selectedTone : undefined
    }).subscribe({
      next: (res) => {
        this.aiSuggestion.set(res.suggestion);
        this.aiLoading.set(false);
      },
      error: () => {
        this.aiError.set('AI assistant is unavailable. Please try again.');
        this.aiLoading.set(false);
      }
    });
  }

  applySuggestion(): void {
    const suggestion = this.aiSuggestion();
    if (suggestion) {
      this.content.set(suggestion);
      this.aiSuggestion.set(null);
    }
  }

  dismissSuggestion(): void {
    this.aiSuggestion.set(null);
  }
}
