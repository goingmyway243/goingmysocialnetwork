import { inject, Injectable, OnDestroy } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../environments/environment';
import { MessageDto, ReadReceiptDto, TypingUser } from '../models/chat.models';
import { AuthService } from './auth.service';

export interface MessageDeletedEvent { messageId: string; deletedBy: string; }
export interface MessagesReadEvent { receipts: ReadReceiptDto[]; }

@Injectable({ providedIn: 'root' })
export class ChatSignalRService implements OnDestroy {

  // ── 1. Dependencies ─────────────────────────────────────────
  private readonly _auth = inject(AuthService);

  // ── 2. SignalR connection ─────────────────────────────────────
  private _connection: signalR.HubConnection | null = null;

  // ── 3. Event streams ─────────────────────────────────────────
  readonly messageReceived$ = new Subject<MessageDto>();
  readonly messageDeleted$ = new Subject<MessageDeletedEvent>();
  readonly messageEdited$ = new Subject<MessageDto>();
  readonly messagesRead$ = new Subject<ReadReceiptDto[]>();
  readonly userTyping$ = new Subject<TypingUser>();
  readonly userStoppedTyping$ = new Subject<{ userId: string }>();

  // ── 4. Connection lifecycle ──────────────────────────────────
  async connect(): Promise<void> {
    if (this._connection?.state === signalR.HubConnectionState.Connected) return;

    this._connection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiGatewayUrl}/hubs/chat`, {
        accessTokenFactory: () => this._auth.getAccessToken()
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this._registerHandlers();
    await this._connection.start();
  }

  async disconnect(): Promise<void> {
    if (this._connection) {
      await this._connection.stop();
      this._connection = null;
    }
  }

  ngOnDestroy(): void {
    this.disconnect();
    this.messageReceived$.complete();
    this.messageDeleted$.complete();
    this.messageEdited$.complete();
    this.messagesRead$.complete();
    this.userTyping$.complete();
    this.userStoppedTyping$.complete();
  }

  // ── 5. Group management ──────────────────────────────────────
  async joinConversation(conversationId: string): Promise<void> {
    await this._invoke('JoinConversation', conversationId);
  }

  async leaveConversation(conversationId: string): Promise<void> {
    await this._invoke('LeaveConversation', conversationId);
  }

  // ── 6. Actions ───────────────────────────────────────────────
  async sendMessage(conversationId: string, content: string): Promise<void> {
    await this._invoke('SendMessage', conversationId, content);
  }

  async deleteMessage(conversationId: string, messageId: string): Promise<void> {
    await this._invoke('DeleteMessage', conversationId, messageId);
  }

  async editMessage(conversationId: string, messageId: string, newContent: string): Promise<void> {
    await this._invoke('EditMessage', conversationId, messageId, newContent);
  }

  async markAsRead(conversationId: string): Promise<void> {
    await this._invoke('MarkAsRead', conversationId);
  }

  async sendTypingIndicator(conversationId: string): Promise<void> {
    await this._invoke('SendTypingIndicator', conversationId);
  }

  async sendStoppedTyping(conversationId: string): Promise<void> {
    await this._invoke('SendStoppedTyping', conversationId);
  }

  // ── 7. Private helpers ───────────────────────────────────────
  private _registerHandlers(): void {
    if (!this._connection) return;

    this._connection.on('ReceiveMessage', (msg: MessageDto) => this.messageReceived$.next(msg));
    this._connection.on('MessageDeleted', (evt: MessageDeletedEvent) => this.messageDeleted$.next(evt));
    this._connection.on('MessageEdited', (msg: MessageDto) => this.messageEdited$.next(msg));
    this._connection.on('MessagesRead', (receipts: ReadReceiptDto[]) => this.messagesRead$.next(receipts));
    this._connection.on('UserTyping', (user: TypingUser) => this.userTyping$.next(user));
    this._connection.on('UserStoppedTyping', (evt: { userId: string }) => this.userStoppedTyping$.next(evt));
  }

  private async _invoke(method: string, ...args: unknown[]): Promise<void> {
    if (!this._connection || this._connection.state !== signalR.HubConnectionState.Connected) {
      await this.connect();
    }
    await this._connection!.invoke(method, ...args);
  }
}
