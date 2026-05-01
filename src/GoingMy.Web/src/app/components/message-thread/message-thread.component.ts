import { AfterViewChecked, Component, ElementRef, inject, output, signal, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SkeletonModule } from 'primeng/skeleton';
import { ConversationDto, MessageDto } from '../../models/chat.models';
import { ChatStateService } from '../../services/chat-state.service';
import { AuthService } from '../../services/auth.service';
import { MessageBubbleComponent } from '../message-bubble/message-bubble.component';

@Component({
  selector: 'app-message-thread',
  standalone: true,
  imports: [MessageBubbleComponent, FormsModule, SkeletonModule],
  template: `
    <div class="message-panel">
      <!-- Header — Facebook Messenger style -->
      <div class="message-panel-header">
        @if (_state.selectedConversation(); as conv) {
          <div class="recipient-info">
            <div class="recipient-avatar online">
              {{ getRecipientName(conv).charAt(0).toUpperCase() }}
            </div>
            <div class="recipient-details" [hidden]="_showSearch()">
              <div class="recipient-name">{{ getRecipientName(conv) }}</div>
              <div class="recipient-status">Active now</div>
            </div>
          </div>
        }

        <!-- Action buttons (right side) -->
        <div class="header-actions">
          <!-- Search toggle -->
          <div class="header-search-bar" [class.visible]="_showSearch()">
            <i class="pi pi-search" style="color:rgba(255,255,255,0.35);font-size:0.75rem;flex-shrink:0"></i>
            @if (_state.isSearching()) {
              <span class="header-search-count">{{ _state.searchResults()?.length }}</span>
            }
            <input
              type="text"
              [(ngModel)]="_searchQuery"
              (ngModelChange)="onSearchChange($event)"
              placeholder="Search messages..."
            />
            @if (_searchQuery) {
              <button class="mini-header-btn" (click)="clearSearch()" style="font-size:0.6rem">
                <i class="pi pi-times"></i>
              </button>
            }
          </div>

          <button class="header-action-btn" [class.active]="_showSearch()" title="Search" (click)="toggleSearch()">
            <i class="pi pi-search"></i>
          </button>
          <button class="header-action-btn" title="Voice call">
            <i class="pi pi-phone"></i>
          </button>
          <button class="header-action-btn" title="Video call">
            <i class="pi pi-video"></i>
          </button>
          <button class="header-action-btn" title="Conversation info">
            <i class="pi pi-info-circle"></i>
          </button>
        </div>
      </div>

      <!-- Message Stream -->
      <div #messageStream class="message-stream">
        @if (_state.pagination().hasMore) {
          <button class="load-more-btn action-btn" (click)="_state.loadMoreMessages()">
            @if (_state.loadingMessages()) {
              <i class="pi pi-spin pi-spinner" style="font-size:0.75rem"></i>
            } @else {
              <i class="pi pi-chevron-up" style="font-size:0.65rem"></i>
              Load older messages
            }
          </button>
        }

        @if (_state.isSearching()) {
          @if ((_state.searchResults() ?? []).length === 0) {
            <div class="chat-empty">
              <i class="pi pi-search empty-icon"></i>
              <div class="empty-sub">No messages found for "{{ _searchQuery }}"</div>
            </div>
          }
          @for (msg of (_state.searchResults() ?? []); track msg.id) {
            <app-message-bubble
              [message]="msg"
              [currentUserId]="_currentUserId"
              [readReceipts]="_state.selectedReadReceipts()"
              (edit)="startEdit($event)"
              (delete)="deleteMessage($event)"
            />
          }
        } @else {
          @if (_state.loadingMessages() && _state.selectedMessages().length === 0) {
            @for (_ of [1,2,3,4]; track $index) {
              <div [style.align-self]="$index % 2 === 0 ? 'flex-start' : 'flex-end'" style="max-width:55%">
                <p-skeleton width="100%" height="44px" borderRadius="18px" />
              </div>
            }
          }
          @for (msg of _state.selectedMessages(); track msg.id) {
            <app-message-bubble
              [message]="msg"
              [currentUserId]="_currentUserId"
              [readReceipts]="_state.selectedReadReceipts()"
              (edit)="startEdit($event)"
              (delete)="deleteMessage($event)"
            />
          }
        }
      </div>

      @if (_state.selectedTypingUsers().length > 0) {
        <div class="typing-indicator">
          <div class="typing-dots">
            <span></span><span></span><span></span>
          </div>
          {{ typingNames() }}
          {{ _state.selectedTypingUsers().length === 1 ? 'is' : 'are' }} typing…
        </div>
      }

      @if (_editingMessage) {
        <div class="edit-indicator">
          <i class="pi pi-pencil" style="font-size:0.75rem"></i>
          Editing message
          <button class="cancel-edit" (click)="cancelEdit()">Cancel</button>
        </div>
      }
    </div>
  `
})
export class MessageThreadComponent implements AfterViewChecked {
  // ── 1. Dependencies ─────────────────────────────────────────
  readonly _state = inject(ChatStateService);
  private readonly _auth = inject(AuthService);

  // ── 2. State ────────────────────────────────────────────────
  @ViewChild('messageStream') private _streamRef!: ElementRef<HTMLDivElement>;
  _currentUserId = this._auth.getCurrentUserId();
  _editingMessage: MessageDto | null = null;
  _searchQuery = '';
  readonly _showSearch = signal(false);
  private _shouldScroll = true;
  private _lastMessageCount = 0;

  // ── Computed for template ────────────────────────────────────
  readonly typingNames = () => this._state.selectedTypingUsers().map(u => u.username).join(', ');

  // ── 3. Lifecycle ─────────────────────────────────────────────
  ngAfterViewChecked(): void {
    const msgs = this._state.selectedMessages();
    if (msgs.length !== this._lastMessageCount) {
      this._lastMessageCount = msgs.length;
      if (this._shouldScroll) this._scrollToBottom();
    }
  }

  // ── 4. Actions ───────────────────────────────────────────────
  startEdit(msg: MessageDto): void {
    this._editingMessage = msg;
    this.editMessageRequested.emit(msg);
  }

  cancelEdit(): void {
    this._editingMessage = null;
    this.editCancel.emit();
  }

  deleteMessage(id: string): void {
    this._state.deleteMessage(id);
  }

  toggleSearch(): void {
    this._showSearch.update(v => !v);
    if (!this._showSearch()) this.clearSearch();
  }

  onSearchChange(query: string): void {
    if (!query.trim()) {
      this._state.clearSearch();
      this._shouldScroll = true;
    } else {
      this._state.searchMessages(query);
      this._shouldScroll = false;
    }
  }

  clearSearch(): void {
    this._searchQuery = '';
    this._state.clearSearch();
    this._shouldScroll = true;
  }

  getRecipientName(conv: ConversationDto): string {
    const myId = this._auth.getCurrentUserId();
    const idx = conv.participantIds.findIndex(id => id !== myId);
    return conv.participantUsernames[idx] ?? conv.participantUsernames[0] ?? 'Unknown';
  }

  // Outputs for compose area coordination
  readonly editMessageRequested = output<MessageDto>();
  readonly editCancel = output<void>();

  private _scrollToBottom(): void {
    try {
      const el = this._streamRef?.nativeElement;
      if (el) el.scrollTop = el.scrollHeight;
    } catch { /* ignore */ }
  }
}
