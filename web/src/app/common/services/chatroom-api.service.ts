import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { environment } from '../../../environments/environment';
import { ISearchChatroomRequest } from '../dtos/chatroom-api.dto';
import { IPagedResponse } from '../dtos/common-api.dto';
import { Chatroom } from '../models/chatroom.model';

@Injectable({
    providedIn: 'root',
})
export class ChatroomApiService extends BaseApiService {
    protected override apiUrl: string = `${environment.baseUrl}/api/chatrooms`;

    constructor(http: HttpClient) {
        super(http);
    }

    searchChatrooms(request: ISearchChatroomRequest): Observable<IPagedResponse<Chatroom>> {
        return this.post('search', request);
    }

    getChatroomById(id: string): Observable<any> {
        return this.get(id);
    }

    createChatroom(data: any): Observable<any> {
        return this.post('', data);
    }

    updateChatroom(id: string, data: any): Observable<any> {
        return this.put(id, data);
    }

    deleteChatroom(id: string): Observable<any> {
        return this.delete(id);
    }
}