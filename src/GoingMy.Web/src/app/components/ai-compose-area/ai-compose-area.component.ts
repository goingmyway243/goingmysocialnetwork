import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AiChatService } from '../../services/ai-chat.service';

@Component({
  selector: 'app-ai-compose-area',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="compose-area">
      <div class="compose-input-wrapper">
        <textarea
          class="compose-input"
          [(ngModel)]="_content"
          (keydown.enter)="onEnter($event)"
          placeholder="Message AI Assistant..."
          rows="1"
          [disabled]="_aiChat.isAiResponding()"
        ></textarea>
      </div>

      <button
        class="send-btn"
        [disabled]="!_content.trim() || _aiChat.isAiResponding()"
        (click)="submit()"
        title="Send"
      >
        @if (_aiChat.isAiResponding()) {
          <i class="pi pi-spin pi-spinner" style="font-size:0.9rem"></i>
        } @else {
          <i class="pi pi-send" style="font-size:0.9rem"></i>
        }
      </button>
    </div>
  `
})
export class AiComposeAreaComponent {

  // ── 1. Dependencies ─────────────────────────────────────────
  readonly _aiChat = inject(AiChatService);

  // ── 2. State ────────────────────────────────────────────────
  _content = '';

  // ── 3. Actions ───────────────────────────────────────────────
  onEnter(event: Event): void {
    const keyboardEvent = event as KeyboardEvent;
    if (!keyboardEvent.shiftKey) {
      keyboardEvent.preventDefault();
      this.submit();
    }
  }

  async submit(): Promise<void> {
    const content = this._content.trim();
    if (!content || this._aiChat.isAiResponding()) return;

    this._content = '';
    await this._aiChat.sendMessage(content);
  }
}
