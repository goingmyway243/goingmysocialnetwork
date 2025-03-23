import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { ChatMessage } from '../models/chat-message.model';
import { ICreateChatMessageRequest } from '../dtos/chat-message-api.dto';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class ChatService {
    private hubConnection!: signalR.HubConnection;
    private _receivedMessageSubject = new BehaviorSubject<ChatMessage | null>(null);

    receivedMessage$: Observable<ChatMessage | null> = this._receivedMessageSubject.asObservable();

    constructor() {
        this.startConnection();
    }

    private startConnection() {
        this.hubConnection = new signalR.HubConnectionBuilder()
            .withUrl(environment.chathubUrl, { withCredentials: true }) // Replace with your API URL
            .withAutomaticReconnect()
            .build();

        this.hubConnection.start()
            .then(() => console.log('Connected to SignalR!'))
            .catch(err => console.error('Connection error: ', err));

        this.hubConnection.on('ReceiveMessage', (chatMessage: ChatMessage) => {
            this._receivedMessageSubject.next(chatMessage);
        });
    }

    sendMessage(message: ICreateChatMessageRequest) {
        this.hubConnection.invoke('SendMessage', message)
            .catch(err => console.error('Send error: ', err));
    }
}
