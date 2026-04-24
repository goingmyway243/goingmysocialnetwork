import { computed, inject, Injectable, OnDestroy, signal } from '@angular/core';
import { Subscription } from 'rxjs';
import { ConversationDto, MessageDto, PaginatedResult, ReadReceiptDto, TypingUser } from '../models/chat.models';
import { ChatApiService } from './chat-api.service';
import { ChatSignalRService } from './chat-signalr.service';

@Injectable({ providedIn: 'root' })
export class ChatStateService implements OnDestroy {

  // ── 1. Dependencies ─────────────────────────────────────────
  private readonly _api = inject(ChatApiService);
  private readonly _signalR = inject(ChatSignalRService);
  private readonly _subs: Subscription[] = [];

  // ── 2. State ────────────────────────────────────────────────
  readonly conversations = signal<ConversationDto[]>([]);
  readonly selectedConversationId = signal<string | null>(null);
  readonly messages = signal<Map<string, MessageDto[]>>(new Map());
  readonly readReceipts = signal<Map<string, ReadReceiptDto[]>>(new Map());
  readonly typingUsers = signal<Map<string, TypingUser[]>>(new Map());
  readonly loadingConversations = signal(false);
  readonly loadingMessages = signal(false);
  readonly sendingMessage = signal(false);
  readonly pagination = signal<{ pageNumber: number; hasMore: boolean }>({ pageNumber: 0, hasMore: false });
  readonly searchResults = signal<MessageDto[] | null>(null);
  readonly searchQuery = signal('');
  readonly error = signal<string | null>(null);

  // ── 3. Derived State ─────────────────────────────────────────
  readonly selectedConversation = computed(() =>
    this.conversations().find(c => c.id === this.selectedConversationId()) ?? null
  );

  readonly selectedMessages = computed(() => {
    const id = this.selectedConversationId();
    return id ? (this.messages().get(id) ?? []) : [];
  });

  readonly selectedReadReceipts = computed(() => {
    const id = this.selectedConversationId();
    return id ? (this.readReceipts().get(id) ?? []) : [];
  });

  readonly selectedTypingUsers = computed(() => {
    const id = this.selectedConversationId();
    return id ? (this.typingUsers().get(id) ?? []) : [];
  });

  readonly isSearching = computed(() => this.searchResults() !== null);

  // ── 4. Lifecycle ─────────────────────────────────────────────
  constructor() {
    this._subscribeToSignalR();
  }

  ngOnDestroy(): void {
    this._subs.forEach(s => s.unsubscribe());
    const id = this.selectedConversationId();
    if (id) this._signalR.leaveConversation(id);
  }

  // ── 5. Conversation Actions ──────────────────────────────────
  loadConversations(): void {
    this.loadingConversations.set(true);
    this.error.set(null);
    this._api.getConversations().subscribe({
      next: convs => {
        this.conversations.set(convs);
        this.loadingConversations.set(false);
      },
      error: () => {
        this.error.set('Failed to load conversations.');
        this.loadingConversations.set(false);
      }
    });
  }

  async selectConversation(conversationId: string): Promise<void> {
    const prev = this.selectedConversationId();
    if (prev === conversationId) return;

    if (prev) await this._signalR.leaveConversation(prev);

    this.selectedConversationId.set(conversationId);
    this.searchResults.set(null);
    this.searchQuery.set('');
    this.pagination.set({ pageNumber: 0, hasMore: false });

    await this._signalR.joinConversation(conversationId);
    this.loadMessages(conversationId, 0);
    this._loadReadReceipts(conversationId);
    this.markConversationAsRead(conversationId);
  }

  createConversation(recipientId: string, recipientUsername: string): void {
    this._api.createConversation(recipientId, recipientUsername).subscribe({
      next: conv => {
        this.conversations.update(list => {
          const exists = list.find(c => c.id === conv.id);
          return exists ? list : [conv, ...list];
        });
        this.selectConversation(conv.id);
      },
      error: () => {
        this.error.set('Failed to create conversation.');
      }
    });
  }

  // ── 6. Message Actions ───────────────────────────────────────
  loadMessages(conversationId: string, pageNumber: number): void {
    this.loadingMessages.set(true);
    this._api.getMessages(conversationId, pageNumber).subscribe({
      next: (result: PaginatedResult<MessageDto>) => {
        this.messages.update(map => {
          const existing = pageNumber === 0 ? [] : (map.get(conversationId) ?? []);
          // result.items are newest-first from backend, reverse for display (oldest first)
          const incoming = [...result.items].reverse();
          map.set(conversationId, [...incoming, ...existing]);
          return new Map(map);
        });
        this.pagination.set({ pageNumber, hasMore: result.hasMore });
        this.loadingMessages.set(false);
      },
      error: () => {
        this.error.set('Failed to load messages.');
        this.loadingMessages.set(false);
      }
    });
  }

  loadMoreMessages(): void {
    const id = this.selectedConversationId();
    const { pageNumber, hasMore } = this.pagination();
    if (!id || !hasMore || this.loadingMessages()) return;
    this.loadMessages(id, pageNumber + 1);
  }

  async sendMessage(content: string): Promise<void> {
    const id = this.selectedConversationId();
    if (!id || !content.trim()) return;

    this.sendingMessage.set(true);
    try {
      await this._signalR.sendMessage(id, content);
    } catch {
      // Fallback to REST if SignalR fails
      this._api.sendMessage(id, content).subscribe({
        next: msg => this._addMessage(msg),
        error: () => this.error.set('Failed to send message.')
      });
    } finally {
      this.sendingMessage.set(false);
    }
  }

  async deleteMessage(messageId: string): Promise<void> {
    const id = this.selectedConversationId();
    if (!id) return;

    try {
      await this._signalR.deleteMessage(id, messageId);
    } catch {
      this._api.deleteMessage(id, messageId).subscribe({
        next: () => this._markMessageDeleted(id, messageId),
        error: () => this.error.set('Failed to delete message.')
      });
    }
  }

  async editMessage(messageId: string, newContent: string): Promise<void> {
    const id = this.selectedConversationId();
    if (!id) return;

    try {
      await this._signalR.editMessage(id, messageId, newContent);
    } catch {
      this._api.editMessage(id, messageId, newContent).subscribe({
        next: msg => this._updateMessage(msg),
        error: () => this.error.set('Failed to edit message.')
      });
    }
  }

  markConversationAsRead(conversationId: string): void {
    this._api.markAsRead(conversationId).subscribe({
      next: receipts => this._mergeReadReceipts(conversationId, receipts)
    });
  }

  // ── 7. Search ────────────────────────────────────────────────
  searchMessages(query: string): void {
    const id = this.selectedConversationId();
    if (!id || !query.trim()) {
      this.searchResults.set(null);
      this.searchQuery.set('');
      return;
    }

    this.searchQuery.set(query);
    this._api.searchMessages(id, query).subscribe({
      next: results => this.searchResults.set(results),
      error: () => this.error.set('Search failed.')
    });
  }

  clearSearch(): void {
    this.searchResults.set(null);
    this.searchQuery.set('');
  }

  // ── 8. Typing Indicators ─────────────────────────────────────
  async sendTyping(): Promise<void> {
    const id = this.selectedConversationId();
    if (id) await this._signalR.sendTypingIndicator(id);
  }

  async sendStoppedTyping(): Promise<void> {
    const id = this.selectedConversationId();
    if (id) await this._signalR.sendStoppedTyping(id);
  }

  // ── 9. Private helpers ────────────────────────────────────────
  private _subscribeToSignalR(): void {
    this._subs.push(
      this._signalR.messageReceived$.subscribe(msg => this._addMessage(msg)),
      this._signalR.messageDeleted$.subscribe(evt =>
        this._markMessageDeleted(evt.messageId.split('/')[0] ?? this.selectedConversationId() ?? '', evt.messageId)),
      this._signalR.messageEdited$.subscribe(msg => this._updateMessage(msg)),
      this._signalR.messagesRead$.subscribe(receipts => {
        const id = this.selectedConversationId();
        if (id) this._mergeReadReceipts(id, receipts);
      }),
      this._signalR.userTyping$.subscribe(user => {
        const id = this.selectedConversationId();
        if (!id) return;
        this.typingUsers.update(map => {
          const users = map.get(id) ?? [];
          if (!users.find(u => u.userId === user.userId)) users.push(user);
          map.set(id, users);
          return new Map(map);
        });
        // Auto clear after 3s
        setTimeout(() => this._removeTypingUser(id, user.userId), 3000);
      }),
      this._signalR.userStoppedTyping$.subscribe(evt => {
        const id = this.selectedConversationId();
        if (id) this._removeTypingUser(id, evt.userId);
      })
    );
  }

  private _addMessage(msg: MessageDto): void {
    this.messages.update(map => {
      const list = map.get(msg.conversationId) ?? [];
      map.set(msg.conversationId, [...list, msg]);
      return new Map(map);
    });
    // Update conversation last message preview
    this.conversations.update(convs =>
      convs.map(c => c.id === msg.conversationId
        ? { ...c, lastMessagePreview: msg.content, lastMessageAt: msg.sentAt }
        : c)
    );
  }

  private _markMessageDeleted(conversationId: string, messageId: string): void {
    this.messages.update(map => {
      const list = map.get(conversationId) ?? [];
      map.set(conversationId, list.map(m =>
        m.id === messageId ? { ...m, isDeleted: true } : m));
      return new Map(map);
    });
  }

  private _updateMessage(msg: MessageDto): void {
    this.messages.update(map => {
      const list = map.get(msg.conversationId) ?? [];
      map.set(msg.conversationId, list.map(m => m.id === msg.id ? msg : m));
      return new Map(map);
    });
  }

  private _mergeReadReceipts(conversationId: string, incoming: ReadReceiptDto[]): void {
    this.readReceipts.update(map => {
      const existing = map.get(conversationId) ?? [];
      const merged = [...existing];
      for (const r of incoming) {
        const idx = merged.findIndex(e => e.messageId === r.messageId && e.readByUserId === r.readByUserId);
        if (idx >= 0) merged[idx] = r;
        else merged.push(r);
      }
      map.set(conversationId, merged);
      return new Map(map);
    });
  }

  private _removeTypingUser(conversationId: string, userId: string): void {
    this.typingUsers.update(map => {
      const users = (map.get(conversationId) ?? []).filter(u => u.userId !== userId);
      map.set(conversationId, users);
      return new Map(map);
    });
  }

  private _loadReadReceipts(conversationId: string): void {
    this._api.getReadReceipts(conversationId).subscribe({
      next: receipts => this._mergeReadReceipts(conversationId, receipts)
    });
  }
}
