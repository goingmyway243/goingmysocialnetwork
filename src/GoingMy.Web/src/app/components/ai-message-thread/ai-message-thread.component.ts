import { Component, ElementRef, inject, ViewChild, effect } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { AiChatService } from '../../services/ai-chat.service';
import { MessageBubbleComponent } from '../message-bubble/message-bubble.component';

@Component({
  selector: 'app-ai-message-thread',
  standalone: true,
  imports: [MessageBubbleComponent],
  template: `
    <div class="message-panel">
      <div class="message-panel-header">
        <div class="recipient-info">
          <div class="recipient-avatar online ai-recipient-avatar">
            <i class="pi pi-sparkles"></i>
          </div>
          <div class="recipient-details">
            <div class="recipient-name">AI Assistant</div>
            <div class="recipient-status ai-status">Powered by llama3.2 · Local</div>
          </div>
        </div>
      </div>

      <div class="message-stream" #messageStream>
        @for (msg of _aiChat.messages(); track msg.id) {
          <app-message-bubble
            [message]="msg"
            [currentUserId]="_currentUserId"
            [allowActions]="false"
          />
        }

        @if (_aiChat.isAiResponding() && _aiChat.streamingContent()) {
          <div class="ai-streaming-row">
            <div class="ai-streaming-bubble">
              {{ _aiChat.streamingContent() }}<span class="ai-streaming-cursor"></span>
            </div>
          </div>
        }

        @if (_aiChat.isAiResponding() && !_aiChat.streamingContent()) {
          <div class="ai-streaming-row">
            <div class="ai-streaming-bubble ai-typing-bubble">
              <span class="ai-streaming-cursor"></span>
              <span class="ai-streaming-cursor" style="animation-delay:0.2s"></span>
              <span class="ai-streaming-cursor" style="animation-delay:0.4s"></span>
            </div>
          </div>
        }
      </div>
    </div>
  `
})
export class AiMessageThreadComponent {

  // ── 1. Dependencies ─────────────────────────────────────────
  readonly _aiChat = inject(AiChatService);
  private readonly _auth = inject(AuthService);

  // ── 2. State ────────────────────────────────────────────────
  @ViewChild('messageStream') private _streamRef?: ElementRef<HTMLDivElement>;
  readonly _currentUserId = this._auth.getCurrentUserId();

  // ── 3. Lifecycle ─────────────────────────────────────────────
  constructor() {
    effect(() => {
      this._aiChat.messages();
      this._aiChat.streamingContent();
      setTimeout(() => this._scrollToBottom(), 50);
    });
  }

  // ── 4. Private helpers ───────────────────────────────────────
  private _scrollToBottom(): void {
    const el = this._streamRef?.nativeElement;
    if (el) el.scrollTop = el.scrollHeight;
  }
}
