import { Component, computed, effect, inject, signal, untracked } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { ConversationDto, MessageDto } from '../../models/chat.models';
import { ChatStateService } from '../../services/chat-state.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-mini-chat',
  standalone: true,
  imports: [FormsModule, RouterLink, ToastModule],
  providers: [MessageService],
  template: `
    <p-toast position="bottom-right" key="mini-chat-toast"></p-toast>
    <div class="mini-chat-host">

      <!-- Mini conversations panel -->
      @if (_showPanel() && !_activeConversation()) {
        <div class="mini-conversations-panel">
          <div class="mini-panel-header">
            <span class="mini-panel-title">Messaging</span>
            <button class="mini-panel-close" (click)="closePanel()">
              <i class="pi pi-times"></i>
            </button>
          </div>

          <div class="mini-conv-list">
            @if (_state.conversations().length === 0) {
              <div class="mini-empty">
                <p>No recent conversations</p>
                <a routerLink="/dashboard/messages">Open Messages</a>
              </div>
            } @else {
              @for (conv of _state.conversations(); track conv.id) {
                <div class="mini-conv-item" (click)="openConversation(conv)">
                  <div class="mini-avatar">{{ getInitial(conv) }}</div>
                  <div class="mini-conv-info">
                    <div class="mini-conv-name">{{ getRecipientName(conv) }}</div>
                    @if (conv.lastMessagePreview) {
                      <div class="mini-conv-preview">{{ conv.lastMessagePreview }}</div>
                    }
                  </div>
                  @if ((conv.unreadCount ?? 0) > 0) {
                    <div class="mini-conv-unread">{{ conv.unreadCount }}</div>
                  }
                </div>
              }
            }
          </div>
        </div>
      }

      <!-- Mini chatbox (single open conversation) -->
      @if (_activeConversation(); as conv) {
        <div class="mini-chatbox">
          <div class="mini-chatbox-header">
            <div class="mini-avatar">{{ getInitial(conv) }}</div>
            <div style="flex:1;min-width:0">
              <div class="mini-chatbox-name">{{ getRecipientName(conv) }}</div>
              <div class="mini-chatbox-status">Active now</div>
            </div>
            <div class="mini-header-actions">
              <button class="mini-header-btn" title="Open full chat" (click)="goToMessages(conv.id)">
                <i class="pi pi-external-link"></i>
              </button>
              <button class="mini-header-btn" title="Back to list" (click)="backToList()">
                <i class="pi pi-chevron-down"></i>
              </button>
              <button class="mini-header-btn" title="Close" (click)="closePanel()">
                <i class="pi pi-times"></i>
              </button>
            </div>
          </div>

          <div class="mini-chatbox-body">
            @if (_miniMessages().length === 0) {
              <div style="flex:1;display:flex;align-items:center;justify-content:center;color:rgba(255,255,255,0.35);font-size:0.8rem;text-align:center;padding:16px">
                No messages yet. Say hi! 👋
              </div>
            } @else {
              @for (msg of _miniMessages(); track msg.id) {
                <div class="mini-msg" [class.sent]="msg.senderId === _currentUserId" [class.received]="msg.senderId !== _currentUserId">
                  {{ msg.editedContent ?? msg.content }}
                </div>
              }
            }
          </div>

          <div class="mini-chatbox-compose">
            <input
              class="mini-compose-input"
              type="text"
              [(ngModel)]="_miniContent"
              placeholder="Aa"
              (keydown.enter)="sendMiniMessage()"
            />
            <button
              class="mini-send-btn"
              (click)="sendMiniMessage()"
              [disabled]="!_miniContent.trim()"
              title="Send"
            >
              <i class="pi pi-send" style="font-size:0.85rem"></i>
            </button>
          </div>
        </div>
      }

      <!-- Floating toggle button -->
      <button class="mini-chat-toggle" (click)="togglePanel()" title="Messaging">
        @if (_showPanel() || _activeConversation()) {
          <i class="pi pi-times"></i>
        } @else {
          <i class="pi pi-comments"></i>
        }
        @if (_totalUnread() > 0 && !_showPanel() && !_activeConversation()) {
          <span class="mini-chat-badge">{{ _totalUnread() > 9 ? '9+' : _totalUnread() }}</span>
        }
      </button>
    </div>
  `
})
export class MiniChatComponent {

  // ── 1. Dependencies ─────────────────────────────────────────
  readonly _state = inject(ChatStateService);
  private readonly _auth = inject(AuthService);
  private readonly _router = inject(Router);
  private readonly _messageService = inject(MessageService);

  // ── 2. State ────────────────────────────────────────────────
  readonly _showPanel = signal(false);
  private readonly _activeConvId = signal<string | null>(null);
  _miniContent = '';
  readonly _currentUserId = this._auth.getCurrentUserId();

  // ── 3. Derived ───────────────────────────────────────────────
  readonly _activeConversation = computed(() => {
    const id = this._activeConvId();
    return id ? (this._state.conversations().find(c => c.id === id) ?? null) : null;
  });

  readonly _miniMessages = computed((): MessageDto[] => {
    const id = this._activeConvId();
    if (!id) return [];
    return this._state.messages().get(id) ?? [];
  });

  readonly _totalUnread = computed(() =>
    this._state.conversations().reduce((sum, c) => sum + (c.unreadCount ?? 0), 0)
  );

  constructor() {
    // React to profile page requesting a specific conversation to be opened.
    // untracked() isolates signal writes so the set(null) doesn't re-schedule
    // this effect before the _activeConvId change is committed.
    effect(() => {
      const id = this._state.requestedConversationId();
      if (id) {
        untracked(() => {
          this._activeConvId.set(id);
          this._showPanel.set(false);
          this._state.requestedConversationId.set(null);
        });
      }
    });

    // Show toast notification for inbound messages from non-active conversations
    effect(() => {
      const notification = this._state.newMessageNotification();
      if (notification) {
        untracked(() => {
          const truncated = notification.content.length > 60
            ? notification.content.substring(0, 60) + '…'
            : notification.content;
          this._messageService.add({
            key: 'mini-chat-toast',
            severity: 'info',
            summary: `@${notification.senderUsername}`,
            detail: truncated,
            life: 4000
          });
          this._state.newMessageNotification.set(null);
        });
      }
    });
  }
  togglePanel(): void {
    if (this._activeConvId()) {
      this._activeConvId.set(null);
      this._showPanel.set(false);
    } else {
      this._showPanel.update(v => !v);
    }
  }

  closePanel(): void {
    this._showPanel.set(false);
    this._activeConvId.set(null);
  }

  openConversation(conv: ConversationDto): void {
    this._activeConvId.set(conv.id);
    this._showPanel.set(false);
  }

  backToList(): void {
    this._activeConvId.set(null);
    this._showPanel.set(true);
  }

  goToMessages(convId: string): void {
    this._state.selectConversation(convId);
    this._router.navigate(['/dashboard/messages']);
    this.closePanel();
  }

  sendMiniMessage(): void {
    const content = this._miniContent.trim();
    const convId = this._activeConvId();
    if (!content || !convId) return;

    // Optimistically add message to state for display
    const optimistic: MessageDto = {
      id: `mini-${Date.now()}`,
      conversationId: convId,
      senderId: this._currentUserId,
      senderUsername: this._auth.getCurrentUsername(),
      content,
      sentAt: new Date().toISOString(),
      isDeleted: false,
      editedContent: null,
      editedAt: null
    };

    this._state.messages.update(map => {
      const existing = map.get(convId) ?? [];
      map.set(convId, [...existing, optimistic]);
      return new Map(map);
    });

    this._miniContent = '';
  }

  // ── 5. Helpers ───────────────────────────────────────────────
  getRecipientName(conv: ConversationDto): string {
    const myId = this._auth.getCurrentUserId();
    const idx = conv.participantIds.findIndex(id => id !== myId);
    return conv.participantUsernames[idx] ?? conv.participantUsernames[0] ?? 'Unknown';
  }

  getInitial(conv: ConversationDto): string {
    return this.getRecipientName(conv).charAt(0).toUpperCase();
  }
}
