import { Component, inject } from '@angular/core';
import { DatePipe, NgClass } from '@angular/common';
import { SkeletonModule } from 'primeng/skeleton';
import { FormsModule } from '@angular/forms';
import { ConversationDto } from '../../models/chat.models';
import { ChatStateService } from '../../services/chat-state.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-conversation-list',
  standalone: true,
  imports: [NgClass, DatePipe, SkeletonModule, FormsModule],
  template: `
    <div class="conversation-panel">
      <div class="conversation-panel-header">
        <!-- Title row: "Messages" + action buttons -->
        <div class="conversation-panel-title-row">
          <span class="conversation-panel-title">Messages</span>
          <div class="conversation-panel-actions">
            <button class="new-chat-btn" title="New conversation">
              <i class="pi pi-pencil-edit" style="font-size:0.78rem"></i>
            </button>
          </div>
        </div>
        <!-- Search bar -->
        <div style="position:relative;margin-top:2px">
          <i class="pi pi-search" style="position:absolute;left:11px;top:50%;transform:translateY(-50%);color:rgba(255,255,255,0.35);font-size:0.75rem;pointer-events:none;z-index:1"></i>
          <input
            class="chat-search-input"
            type="text"
            placeholder="Search Messenger"
            [(ngModel)]="_searchTerm"
          />
        </div>
      </div>

      <div class="conversation-list">
        @if (_state.loadingConversations()) {
          @for (_ of [1,2,3,4,5]; track $index) {
            <div style="display:flex;align-items:center;gap:12px;padding:10px 10px">
              <p-skeleton shape="circle" size="44px" />
              <div style="flex:1">
                <p-skeleton width="100px" height="12px" styleClass="mb-2" />
                <p-skeleton width="160px" height="10px" />
              </div>
              <p-skeleton width="28px" height="9px" />
            </div>
          }
        } @else if (filteredConversations().length === 0) {
          <div style="padding:40px 20px;text-align:center;color:rgba(255,255,255,0.38)">
            <i class="pi pi-comments" style="font-size:2rem;display:block;margin-bottom:10px;opacity:0.4"></i>
            <div style="font-size:0.85rem;font-weight:600;margin-bottom:4px">No conversations yet</div>
            <div style="font-size:0.75rem;opacity:0.7">Start chatting from someone's profile</div>
          </div>
        } @else {
          @for (conv of filteredConversations(); track conv.id) {
            <div
              class="conversation-item"
              [ngClass]="{ active: _state.selectedConversationId() === conv.id }"
              (click)="selectConversation(conv)"
            >
              <div class="conversation-avatar online">{{ getInitial(conv) }}</div>
              <div class="conversation-meta">
                <div class="conversation-name">{{ getRecipientName(conv) }}</div>
                @if (conv.lastMessagePreview) {
                  <div class="conversation-preview" [ngClass]="{ unread: (conv.unreadCount ?? 0) > 0 }">
                    {{ conv.lastMessagePreview }}
                  </div>
                }
              </div>
              <div class="conversation-tail">
                @if (conv.lastMessageAt) {
                  <span class="conversation-time">{{ conv.lastMessageAt | date:'shortTime' }}</span>
                }
                @if ((conv.unreadCount ?? 0) > 0) {
                  <div class="unread-badge">{{ conv.unreadCount }}</div>
                }
              </div>
            </div>
          }
        }
      </div>
    </div>
  `
})
export class ConversationListComponent {
  readonly _state = inject(ChatStateService);
  private readonly _auth = inject(AuthService);

  _searchTerm = '';

  filteredConversations() {
    const term = this._searchTerm.toLowerCase();
    if (!term) return this._state.conversations();
    return this._state.conversations().filter(c =>
      c.participantUsernames.some(u => u.toLowerCase().includes(term))
    );
  }

  selectConversation(conv: ConversationDto): void {
    this._state.selectConversation(conv.id);
  }

  getRecipientName(conv: ConversationDto): string {
    const myId = this._auth.getCurrentUserId();
    const idx = conv.participantIds.findIndex(id => id !== myId);
    return conv.participantUsernames[idx] ?? conv.participantUsernames[0] ?? 'Unknown';
  }

  getInitial(conv: ConversationDto): string {
    return this.getRecipientName(conv).charAt(0).toUpperCase();
  }
}

