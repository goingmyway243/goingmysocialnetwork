import { computed, effect, inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';
import { AiConversationResponse, ConversationDto, MessageDto, PaginatedResult } from '../models/chat.models';
import { ChatSignalRService } from './chat-signalr.service';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class AiChatService {

  // ── 1. Dependencies ─────────────────────────────────────────
  private readonly _http = inject(HttpClient);
  private readonly _signalR = inject(ChatSignalRService);
  private readonly _auth = inject(AuthService);
  private readonly _baseUrl = `${environment.apiGatewayUrl}/api/chat`;

  // ── 2. State ─────────────────────────────────────────────────
  readonly conversations = signal<ConversationDto[]>([]);
  readonly selectedConversation = signal<ConversationDto | null>(null);
  readonly messages = signal<MessageDto[]>([]);
  readonly isLoading = signal(false);
  readonly isAiResponding = signal(false);
  readonly streamingContent = signal('');

  // ── 3. Derived State ─────────────────────────────────────────
  readonly hasConversations = computed(() => this.conversations().length > 0);
  readonly selectedId = computed(() => this.selectedConversation()?.id ?? null);
  readonly latestConversation = computed(() => this.conversations().length > 0 ? this.conversations()[0] : null);

  // ── 4. Lifecycle ─────────────────────────────────────────────
  constructor() {
    this._registerSignalRListeners();
  }

  private _registerSignalRListeners(): void {
    // User's own message echoed back via ReceiveMessage
    this._signalR.messageReceived$.subscribe(msg => {
      if (this.selectedId() === msg.conversationId) {
        this.messages.update(msgs => [...msgs, msg]);
      }
    });

    // AI is streaming: accumulate tokens into streamingContent
    this._signalR.aiTokenReceived$.subscribe(token => {
      this.streamingContent.update(current => current + token);
    });

    // AI stream finished: swap streaming buffer for the real persisted message
    this._signalR.aiMessageComplete$.subscribe(msg => {
      this.isAiResponding.set(false);
      this.streamingContent.set('');
      if (this.selectedId() === msg.conversationId) {
        this.messages.update(msgs => [...msgs, msg]);
      }
      // Refresh conversation list so lastMessagePreview updates
      this._refreshConversations();
    });

    // New AI conversation created via hub
    this._signalR.aiConversationCreated$.subscribe(conv => {
      this.conversations.update(convs => {
        const exists = convs.find(c => c.id === conv.id);
        return exists ? convs : [conv, ...convs];
      });
      this.selectConversation(conv);
    });
  }

  // ── 5. Actions ───────────────────────────────────────────────

  async loadConversations(): Promise<void> {
    this.isLoading.set(true);
    try {
      const result = await firstValueFrom(
        this._http.get<ConversationDto[]>(`${this._baseUrl}/ai/conversations`)
      );
      this.conversations.set(result);
    } finally {
      this.isLoading.set(false);
    }
  }

  async createNewConversation(): Promise<void> {
    await this._signalR.connect();
    await this._signalR.createAiConversation();
    // Response handled by aiConversationCreated$ stream above
  }

  async selectConversation(conv: ConversationDto): Promise<void> {
    if (this.selectedId() === conv.id) return;

    const previous = this.selectedId();
    if (previous) await this._signalR.leaveConversation(previous);

    this.selectedConversation.set(conv);
    this.messages.set([]);
    this.streamingContent.set('');
    await this._loadMessages(conv.id);
    await this._signalR.joinConversation(conv.id);
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

  private async _refreshConversations(): Promise<void> {
    const result = await firstValueFrom(
      this._http.get<ConversationDto[]>(`${this._baseUrl}/ai/conversations`)
    );
    this.conversations.set(result);
  }
}
