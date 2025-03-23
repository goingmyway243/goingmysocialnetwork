import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { environment } from '../../../environments/environment';
import { IPagedResponse } from '../dtos/common-api.dto';
import { ChatMessage } from '../models/chat-message.model';
import { ICreateChatMessageRequest, ISearchChatMessageRequest } from '../dtos/chat-message-api.dto';

@Injectable({
    providedIn: 'root',
})
export class ChatMessageApiService extends BaseApiService {
    protected override apiUrl: string = `${environment.baseUrl}/api/chatmessages`;

    constructor(http: HttpClient) {
        super(http);
    }

    searchChatMessages(request: ISearchChatMessageRequest): Observable<IPagedResponse<ChatMessage>> {
        return this.post('search', request);
    }

    getChatMessageById(id: string): Observable<any> {
        return this.get(id);
    }

    createChatMessage(request: ICreateChatMessageRequest): Observable<ChatMessage> {
        return this.post('', request);
    }

    updateChatMessage(id: string, data: any): Observable<any> {
        return this.put(id, data);
    }

    deleteChatMessage(id: string): Observable<any> {
        return this.delete(id);
    }
}