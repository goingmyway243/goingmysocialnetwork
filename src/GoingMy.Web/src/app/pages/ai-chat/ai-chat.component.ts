import {
  Component, ElementRef, ViewChild, inject,
  effect, OnDestroy
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SkeletonModule } from 'primeng/skeleton';
import { ScrollPanelModule } from 'primeng/scrollpanel';
import { AiChatService } from '../../services/ai-chat.service';
import { LayoutService } from '../../services/layout.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-ai-chat',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonModule, InputTextModule, SkeletonModule, ScrollPanelModule],
  styles: [`
    :host { display: flex; height: 100%; }

    .ai-chat-container {
      display: flex;
      width: 100%;
      height: calc(100vh - 72px);
      gap: 0;
      overflow: hidden;
    }

    /* Sidebar */
    .conversations-panel {
      width: 280px;
      min-width: 280px;
      background: rgba(255,255,255,0.06);
      backdrop-filter: blur(20px);
      border-right: 1px solid rgba(255,255,255,0.1);
      display: flex;
      flex-direction: column;
    }

    .panel-header {
      padding: 20px 16px 12px;
      border-bottom: 1px solid rgba(255,255,255,0.08);
      display: flex;
      align-items: center;
      justify-content: space-between;
    }

    .panel-title {
      font-size: 1rem;
      font-weight: 600;
      color: var(--text-primary, #fff);
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .panel-title i { font-size: 1.1rem; color: var(--primary-color, #7c6af7); }

    .conv-list { flex: 1; overflow-y: auto; padding: 8px 0; }

    .conv-item {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 12px 16px;
      cursor: pointer;
      transition: background 0.15s;
    }

    .conv-item:hover { background: rgba(255,255,255,0.06); }
    .conv-item.active { background: rgba(124, 106, 247, 0.15); }

    .conv-avatar {
      width: 38px; height: 38px;
      border-radius: 50%;
      background: linear-gradient(135deg, #7c6af7, #a78bf0);
      display: flex; align-items: center; justify-content: center;
      font-size: 0.9rem; color: #fff; font-weight: 600;
      flex-shrink: 0;
    }

    .conv-info { flex: 1; min-width: 0; }

    .conv-name {
      font-size: 0.85rem; font-weight: 600;
      color: var(--text-primary, #fff); white-space: nowrap;
      overflow: hidden; text-overflow: ellipsis;
    }

    .conv-preview {
      font-size: 0.75rem;
      color: var(--text-muted, rgba(255,255,255,0.5));
      white-space: nowrap; overflow: hidden; text-overflow: ellipsis;
    }

    .empty-sidebar {
      display: flex; flex-direction: column;
      align-items: center; justify-content: center;
      flex: 1; gap: 12px; padding: 24px;
      color: var(--text-muted, rgba(255,255,255,0.45));
      text-align: center; font-size: 0.85rem;
    }

    .empty-sidebar i { font-size: 2rem; opacity: 0.5; }

    /* Chat Area */
    .chat-panel {
      flex: 1;
      display: flex;
      flex-direction: column;
      min-width: 0;
      background: rgba(255,255,255,0.03);
    }

    .chat-header {
      padding: 16px 20px;
      border-bottom: 1px solid rgba(255,255,255,0.08);
      display: flex; align-items: center; gap: 12px;
      background: rgba(255,255,255,0.04);
    }

    .chat-header-avatar {
      width: 36px; height: 36px; border-radius: 50%;
      background: linear-gradient(135deg, #7c6af7, #a78bf0);
      display: flex; align-items: center; justify-content: center;
      color: #fff; font-size: 1rem;
    }

    .chat-header-name {
      font-size: 0.95rem; font-weight: 600;
      color: var(--text-primary, #fff);
    }

    .chat-header-sub {
      font-size: 0.75rem;
      color: var(--text-muted, rgba(255,255,255,0.5));
    }

    /* Messages */
    .messages-area {
      flex: 1; overflow-y: auto;
      padding: 16px 20px; display: flex;
      flex-direction: column; gap: 12px;
    }

    .bubble-row {
      display: flex;
      &.user { justify-content: flex-end; }
      &.ai   { justify-content: flex-start; }
    }

    .bubble {
      max-width: 65%;
      padding: 10px 14px;
      border-radius: 16px;
      font-size: 0.88rem;
      line-height: 1.5;
    }

    .bubble-row.user .bubble {
      background: var(--primary-color, #7c6af7);
      color: #fff;
      border-bottom-right-radius: 4px;
    }

    .bubble-row.ai .bubble {
      background: rgba(255,255,255,0.09);
      color: var(--text-primary, #fff);
      border-bottom-left-radius: 4px;
    }

    .bubble-meta {
      font-size: 0.7rem;
      color: rgba(255,255,255,0.4);
      margin-top: 4px;
      display: block;
    }

    .streaming-cursor {
      display: inline-block;
      width: 2px; height: 1em;
      background: currentColor;
      animation: blink 0.8s step-end infinite;
      vertical-align: text-bottom;
      margin-left: 2px;
    }
    @keyframes blink { 50% { opacity: 0; } }

    .streaming-row { justify-content: flex-start; }

    .deleted-label {
      font-style: italic;
      color: rgba(255,255,255,0.35) !important;
    }

    /* Empty state */
    .empty-chat {
      flex: 1; display: flex; flex-direction: column;
      align-items: center; justify-content: center; gap: 16px;
      color: var(--text-muted, rgba(255,255,255,0.4));
      text-align: center;
    }

    .empty-chat i { font-size: 3rem; opacity: 0.3; }
    .empty-chat h3 { font-size: 1.1rem; font-weight: 600; color: var(--text-primary, #fff); }
    .empty-chat p  { font-size: 0.85rem; max-width: 300px; }

    /* Compose */
    .compose-area {
      padding: 14px 16px;
      border-top: 1px solid rgba(255,255,255,0.08);
      display: flex; gap: 10px; align-items: flex-end;
    }

    .compose-input {
      flex: 1;
      background: rgba(255,255,255,0.06) !important;
      border: 1px solid rgba(255,255,255,0.12) !important;
      border-radius: 20px !important;
      padding: 10px 16px !important;
      color: var(--text-primary, #fff) !important;
      font-size: 0.88rem;
      outline: none;
      resize: none;
      min-height: 42px;
      max-height: 120px;
    }

    .compose-input:disabled { opacity: 0.5; }
  `],
  template: `
<div class="ai-chat-container">

  <!-- ── Conversation Sidebar ──────────────────────────────── -->
  <aside class="conversations-panel">
    <div class="panel-header">
      <span class="panel-title">
        <i class="pi pi-sparkles"></i> AI Assistant
      </span>
      <p-button
        icon="pi pi-plus"
        [text]="true"
        [rounded]="true"
        size="small"
        title="New conversation"
        [loading]="_aiChat.isLoading()"
        (onClick)="newConversation()">
      </p-button>
    </div>

    <div class="conv-list">
      @if (_aiChat.isLoading() && !_aiChat.hasConversations()) {
        @for (_ of [1,2,3]; track $index) {
          <div style="padding: 12px 16px; display: flex; gap: 10px; align-items: center;">
            <p-skeleton shape="circle" size="38px" />
            <div style="flex:1"><p-skeleton width="60%" height="12px" /><p-skeleton width="80%" height="10px" styleClass="mt-1" /></div>
          </div>
        }
      }

      @for (conv of _aiChat.conversations(); track conv.id) {
        <div
          class="conv-item"
          [class.active]="_aiChat.selectedId() === conv.id"
          (click)="_aiChat.selectConversation(conv)">
          <div class="conv-avatar"><i class="pi pi-sparkles"></i></div>
          <div class="conv-info">
            <div class="conv-name">AI Assistant</div>
            <div class="conv-preview">{{ conv.lastMessagePreview ?? 'Start a conversation' }}</div>
          </div>
        </div>
      }

      @if (!_aiChat.isLoading() && !_aiChat.hasConversations()) {
        <div class="empty-sidebar">
          <i class="pi pi-sparkles"></i>
          <span>No conversations yet.<br>Tap <b>+</b> to start chatting with AI.</span>
        </div>
      }
    </div>
  </aside>

  <!-- ── Chat Area ─────────────────────────────────────────── -->
  <section class="chat-panel">

    @if (_aiChat.selectedConversation()) {
      <!-- Header -->
      <div class="chat-header">
        <div class="chat-header-avatar"><i class="pi pi-sparkles"></i></div>
        <div>
          <div class="chat-header-name">AI Assistant</div>
          <div class="chat-header-sub">Powered by llama3.2 · Local</div>
        </div>
      </div>

      <!-- Messages -->
      <div class="messages-area" #messagesArea>
        @if (_aiChat.isLoading()) {
          @for (_ of [1,2,3]; track $index) {
            <p-skeleton height="40px" styleClass="my-1" />
          }
        }

        @for (msg of _aiChat.messages(); track msg.id) {
          <div class="bubble-row" [class.user]="msg.senderId !== 'ai-assistant'" [class.ai]="msg.senderId === 'ai-assistant'">
            <div class="bubble">
              @if (msg.isDeleted) {
                <span class="deleted-label">This message was deleted</span>
              } @else {
                {{ msg.editedContent ?? msg.content }}
              }
              <span class="bubble-meta">{{ msg.sentAt | date:'shortTime' }}</span>
            </div>
          </div>
        }

        <!-- Streaming (in-progress) AI response -->
        @if (_aiChat.isAiResponding() && _aiChat.streamingContent()) {
          <div class="bubble-row ai streaming-row">
            <div class="bubble">
              {{ _aiChat.streamingContent() }}<span class="streaming-cursor"></span>
            </div>
          </div>
        }

        <!-- AI typing indicator (before first token arrives) -->
        @if (_aiChat.isAiResponding() && !_aiChat.streamingContent()) {
          <div class="bubble-row ai">
            <div class="bubble" style="display:flex; gap:4px; align-items:center;">
              <span class="streaming-cursor"></span>
              <span class="streaming-cursor" style="animation-delay:0.2s"></span>
              <span class="streaming-cursor" style="animation-delay:0.4s"></span>
            </div>
          </div>
        }
      </div>

      <!-- Compose -->
      <div class="compose-area">
        <input
          class="compose-input"
          type="text"
          placeholder="Message AI Assistant…"
          [disabled]="_aiChat.isAiResponding()"
          [(ngModel)]="_inputValue"
          (keydown.enter)="sendMessage()"
        />
        <p-button
          icon="pi pi-send"
          [rounded]="true"
          size="small"
          [disabled]="!_inputValue.trim() || _aiChat.isAiResponding()"
          [loading]="_aiChat.isAiResponding()"
          (onClick)="sendMessage()">
        </p-button>
      </div>

    } @else {
      <!-- No conversation selected -->
      <div class="empty-chat">
        <i class="pi pi-sparkles"></i>
        <h3>GoingMy AI Assistant</h3>
        <p>Your personal AI powered by a local LLM. Ask questions, get content ideas, or just chat.</p>
        <p-button
          label="Start New Conversation"
          icon="pi pi-plus"
          [rounded]="true"
          (onClick)="newConversation()">
        </p-button>
      </div>
    }

  </section>

</div>
  `
})
export class AiChatComponent implements OnDestroy {

  // ── 1. Dependencies ─────────────────────────────────────────
  readonly _aiChat = inject(AiChatService);
  private readonly _layout = inject(LayoutService);

  // ── 2. State ─────────────────────────────────────────────────
  _inputValue = '';

  @ViewChild('messagesArea') private _messagesArea?: ElementRef<HTMLDivElement>;

  // ── 3. Lifecycle ─────────────────────────────────────────────
  constructor() {
    this._layout.hideSidebar.set(true);
    this._aiChat.loadConversations();

    // Scroll to bottom when messages or streaming content change
    effect(() => {
      this._aiChat.messages();
      this._aiChat.streamingContent();
      setTimeout(() => this._scrollToBottom(), 50);
    });
  }

  ngOnDestroy(): void {
    this._layout.hideSidebar.set(false);
  }

  // ── 4. Actions ───────────────────────────────────────────────
  async newConversation(): Promise<void> {
    await this._aiChat.createNewConversation();
  }

  async sendMessage(): Promise<void> {
    const content = this._inputValue.trim();
    if (!content || this._aiChat.isAiResponding()) return;
    this._inputValue = '';
    await this._aiChat.sendMessage(content);
  }

  // ── 5. Private helpers ───────────────────────────────────────
  private _scrollToBottom(): void {
    const el = this._messagesArea?.nativeElement;
    if (el) el.scrollTop = el.scrollHeight;
  }
}
