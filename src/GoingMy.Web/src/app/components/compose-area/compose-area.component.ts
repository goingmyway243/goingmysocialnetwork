import { Component, effect, inject, input, OnDestroy, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MessageDto } from '../../models/chat.models';
import { ChatStateService } from '../../services/chat-state.service';

@Component({
  selector: 'app-compose-area',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="compose-area">
      <!-- Left action buttons -->
      @if (!editingMessage()) {
        <button class="compose-action-btn" title="Add attachment">
          <i class="pi pi-paperclip"></i>
        </button>
        <button class="compose-action-btn" title="Add emoji">
          <i class="pi pi-face-smile"></i>
        </button>
      }

      <div class="compose-input-wrapper">
        <textarea
          #textarea
          class="compose-input"
          [(ngModel)]="_content"
          (ngModelChange)="onContentChange($event)"
          (keydown.enter)="onEnter($event)"
          placeholder="{{ editingMessage() ? 'Edit message...' : 'Aa' }}"
          rows="1"
        ></textarea>
      </div>

      <button
        class="send-btn"
        [disabled]="!_content.trim() || _state.sendingMessage()"
        (click)="submit()"
        [title]="editingMessage() ? 'Save edit' : 'Send'"
      >
        @if (_state.sendingMessage()) {
          <i class="pi pi-spin pi-spinner" style="font-size:0.9rem"></i>
        } @else if (editingMessage()) {
          <i class="pi pi-check" style="font-size:0.9rem"></i>
        } @else {
          <i class="pi pi-send" style="font-size:0.9rem"></i>
        }
      </button>
    </div>
  `
})
export class ComposeAreaComponent implements OnDestroy {

  // ── 1. Dependencies ─────────────────────────────────────────
  readonly _state = inject(ChatStateService);

  // ── 2. Inputs / Outputs ──────────────────────────────────────
  editingMessage = input<MessageDto | null>(null);
  editSubmitted = output<{ messageId: string; newContent: string }>();
  editCancelled = output<void>();

  // ── 3. State ────────────────────────────────────────────────
  _content = '';
  private _typingTimer: ReturnType<typeof setTimeout> | null = null;
  private _isTyping = false;

  // ── 4. Effects ───────────────────────────────────────────────
  private readonly _editEffect = effect(() => {
    const msg = this.editingMessage();
    this._content = msg ? (msg.editedContent ?? msg.content) : '';
  });

  // ── 5. Lifecycle ─────────────────────────────────────────────
  ngOnDestroy(): void {
    if (this._typingTimer) clearTimeout(this._typingTimer);
    this._editEffect.destroy();
  }

  // ── 6. Actions ───────────────────────────────────────────────
  onEnter(event: Event): void {
    const ke = event as KeyboardEvent;
    if (!ke.shiftKey) {
      ke.preventDefault();
      this.submit();
    }
  }

  onContentChange(value: string): void {
    if (!value.trim()) {
      this._stopTyping();
      return;
    }
    if (!this._isTyping) {
      this._isTyping = true;
      this._state.sendTyping();
    }
    if (this._typingTimer) clearTimeout(this._typingTimer);
    this._typingTimer = setTimeout(() => this._stopTyping(), 2000);
  }

  async submit(): Promise<void> {
    const content = this._content.trim();
    if (!content) return;

    const editing = this.editingMessage();
    if (editing) {
      await this._state.editMessage(editing.id, content);
      this.editSubmitted.emit({ messageId: editing.id, newContent: content });
    } else {
      await this._state.sendMessage(content);
    }

    this._content = '';
    this._stopTyping();
  }

  // ── 7. Private ───────────────────────────────────────────────
  private _stopTyping(): void {
    if (this._isTyping) {
      this._isTyping = false;
      this._state.sendStoppedTyping();
    }
    if (this._typingTimer) {
      clearTimeout(this._typingTimer);
      this._typingTimer = null;
    }
  }
}
