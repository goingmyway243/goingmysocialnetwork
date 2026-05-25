import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';
import { ConversationDto, MessageDto, PaginatedResult } from '../models/chat.models';
import { ChatSignalRService } from './chat-signalr.service';

@Injectable({ providedIn: 'root' })
export class AiChatService {

  // ── 1. Dependencies ─────────────────────────────────────────
  private readonly _http = inject(HttpClient);
  private readonly _signalR = inject(ChatSignalRService);
  private readonly _baseUrl = `${environment.apiGatewayUrl}/api/chat`;

  // ── 2. State ─────────────────────────────────────────────────
  readonly aiConversation = signal<ConversationDto | null>(null);
  readonly messages = signal<MessageDto[]>([]);
  readonly isLoading = signal(false);
  readonly isAiResponding = signal(false);
  readonly streamingContent = signal('');
  readonly isSelected = signal(false);

  // ── 3. Derived State ─────────────────────────────────────────
  readonly selectedId = computed(() => this.aiConversation()?.id ?? null);

  // ── 4. Lifecycle ─────────────────────────────────────────────
  constructor() {
    this._registerSignalRListeners();
  }

  private _registerSignalRListeners(): void {
    // User's own message echoed back via ReceiveMessage
    this._signalR.messageReceived$.subscribe(msg => {
      if (this.isSelected() && this.selectedId() === msg.conversationId) {
        this.messages.update(msgs => [...msgs, msg]);
      }
    });

    // AI is streaming: accumulate tokens into streamingContent
    this._signalR.aiTokenReceived$.subscribe(token => {
      if (this.isSelected()) {
        this.streamingContent.update(current => current + token);
      }
    });

    // AI stream finished: swap streaming buffer for the real persisted message
    this._signalR.aiMessageComplete$.subscribe(msg => {
      this.isAiResponding.set(false);
      this.streamingContent.set('');
      if (this.isSelected() && this.selectedId() === msg.conversationId) {
        this.messages.update(msgs => [...msgs, msg]);
      }
      // Refresh conversation list so lastMessagePreview updates
      this._refreshConversation();
    });

    // New AI conversation created via hub
    this._signalR.aiConversationCreated$.subscribe(conv => {
      this.aiConversation.set(conv);
    });
  }

  // ── 5. Actions ───────────────────────────────────────────────

  async loadOrCreateConversation(): Promise<void> {
    this.isLoading.set(true);
    try {
      const result = await firstValueFrom(
        this._http.get<ConversationDto[]>(`${this._baseUrl}/ai/conversations`)
      );
      if (result.length > 0) {
        this.aiConversation.set(result[0]);
        return;
      }

      await this._signalR.connect();
      await this._signalR.createAiConversation();
    } finally {
      this.isLoading.set(false);
    }
  }

  setConversationFromList(conv: ConversationDto | null): void {
    this.aiConversation.set(conv);
  }

  async selectAiConversation(): Promise<void> {
    this.isSelected.set(true);

    if (!this.aiConversation()) {
      await this.loadOrCreateConversation();
    }

    const conversationId = this.selectedId();
    if (!conversationId) return;

    this.messages.set([]);
    this.streamingContent.set('');

    await this._signalR.connect();
    await this._signalR.joinConversation(conversationId);
    await this._loadMessages(conversationId);
  }

  async deselectAiConversation(): Promise<void> {
    const conversationId = this.selectedId();
    this.isSelected.set(false);
    this.isAiResponding.set(false);
    this.streamingContent.set('');
    this.messages.set([]);

    if (conversationId) {
      await this._signalR.leaveConversation(conversationId);
    }
  }

  async sendMessage(content: string): Promise<void> {
    const convId = this.selectedId();
    if (!convId || !content.trim() || this.isAiResponding()) return;

    this.isAiResponding.set(true);
    this.streamingContent.set('');

    try {
      await this._signalR.connect();
      await this._signalR.sendAiMessage(convId, content.trim());
    } catch {
      this.isAiResponding.set(false);
      this.streamingContent.set('');
    }
  }

  // ── 6. Private helpers ───────────────────────────────────────

  private async _loadMessages(conversationId: string): Promise<void> {
    this.isLoading.set(true);
    try {
      const result = await firstValueFrom(
        this._http.get<PaginatedResult<MessageDto>>(
          `${this._baseUrl}/conversations/${conversationId}/messages`
        )
      );
      // Messages come newest-first from API; reverse to display oldest-first
      this.messages.set([...result.items].reverse());
    } finally {
      this.isLoading.set(false);
    }
  }

  private async _refreshConversation(): Promise<void> {
    const result = await firstValueFrom(
      this._http.get<ConversationDto[]>(`${this._baseUrl}/ai/conversations`)
    );
    this.aiConversation.set(result[0] ?? this.aiConversation());
  }
}
