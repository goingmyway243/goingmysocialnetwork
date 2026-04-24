import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ConversationDto, MessageDto, PaginatedResult, ReadReceiptDto } from '../models/chat.models';

@Injectable({ providedIn: 'root' })
export class ChatApiService {

  // ── 1. Dependencies ─────────────────────────────────────────
  private readonly _http = inject(HttpClient);
  private readonly _baseUrl = `${environment.apiGatewayUrl}/api/chat`;

  // ── 2. Conversations ─────────────────────────────────────────
  getConversations(): Observable<ConversationDto[]> {
    return this._http.get<ConversationDto[]>(`${this._baseUrl}/conversations`);
  }

  createConversation(recipientId: string, recipientUsername: string): Observable<ConversationDto> {
    return this._http.post<ConversationDto>(`${this._baseUrl}/conversations`, { recipientId, recipientUsername });
  }

  // ── 3. Messages ──────────────────────────────────────────────
  getMessages(conversationId: string, pageNumber = 0, pageSize = 50): Observable<PaginatedResult<MessageDto>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    return this._http.get<PaginatedResult<MessageDto>>(
      `${this._baseUrl}/conversations/${conversationId}/messages`, { params });
  }

  sendMessage(conversationId: string, content: string): Observable<MessageDto> {
    return this._http.post<MessageDto>(
      `${this._baseUrl}/conversations/${conversationId}/messages`, { content });
  }

  deleteMessage(conversationId: string, messageId: string): Observable<void> {
    return this._http.delete<void>(
      `${this._baseUrl}/conversations/${conversationId}/messages/${messageId}`);
  }

  editMessage(conversationId: string, messageId: string, newContent: string): Observable<MessageDto> {
    return this._http.put<MessageDto>(
      `${this._baseUrl}/conversations/${conversationId}/messages/${messageId}`, { newContent });
  }

  searchMessages(conversationId: string, q: string, limit = 20): Observable<MessageDto[]> {
    const params = new HttpParams().set('q', q).set('limit', limit);
    return this._http.get<MessageDto[]>(
      `${this._baseUrl}/conversations/${conversationId}/messages/search`, { params });
  }

  // ── 4. Read Receipts ─────────────────────────────────────────
  markAsRead(conversationId: string): Observable<ReadReceiptDto[]> {
    return this._http.post<ReadReceiptDto[]>(
      `${this._baseUrl}/conversations/${conversationId}/read`, {});
  }

  getReadReceipts(conversationId: string): Observable<ReadReceiptDto[]> {
    return this._http.get<ReadReceiptDto[]>(
      `${this._baseUrl}/conversations/${conversationId}/read-receipts`);
  }
}
