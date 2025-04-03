import { Component, computed, OnDestroy, OnInit, signal } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { UserAvatarComponent } from "../../components/user-avatar/user-avatar.component";
import { MessageItemComponent } from "../../components/message-item/message-item.component";
import { AuthService } from '../../common/services/auth.service';
import { ChatroomApiService } from '../../common/services/chatroom-api.service';
import { User } from '../../common/models/user.model';
import { Chatroom } from '../../common/models/chatroom.model';
import { ChatMessageApiService } from '../../common/services/chat-message-api.service';
import { ChatMessage } from '../../common/models/chat-message.model';
import { FormsModule } from '@angular/forms';
import { ChatService } from '../../common/services/chat.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-message-page',
  standalone: true,
  imports: [FormsModule, MatIconModule, UserAvatarComponent, MessageItemComponent],
  templateUrl: './message-page.component.html',
  styleUrl: './message-page.component.scss'
})
export class MessagePageComponent implements OnInit, OnDestroy {
  currentUser = signal<User | null>(null);
  chatrooms = signal<Chatroom[]>([]);
  selectedChatroom = signal<Chatroom | null>(null);
  chatMessages = signal<ChatMessage[]>([]);
  chatUser = computed(() => this.selectedChatroom()?.participants?.filter(p => p.id !== this.currentUser()?.id)[0] ?? null);

  initiated: boolean = false;
  inputMessage: string = '';
  chatroomIdParam: string = '';

  constructor(
    private route: ActivatedRoute,
    private authSvc: AuthService,
    private chatroomApiSvc: ChatroomApiService,
    private chatMessageApiSvc: ChatMessageApiService,
    private chatSvc: ChatService
  ) { }

  ngOnInit(): void {
    this.chatSvc.startConnection();
    window.addEventListener('beforeunload', this.chatSvc.closeConnection.bind(this));

    this.route.params.subscribe(params => {
      this.chatroomIdParam = params['id'];
    });

    this.authSvc.currentUser$.subscribe(user => {
      this.currentUser.set(user);

      if (user) {
        this.chatroomApiSvc.searchChatrooms({
          userId: user.id,
          searchText: '',
          pagedRequest: {
            pageIndex: 0,
            pageSize: 20
          }
        }).subscribe(result => {
          this.chatrooms.set(result.items);
          this.initiated = true;

          const selectedChatroom = this.chatroomIdParam
            ? this.chatrooms().find(c => c.id === this.chatroomIdParam)
            : this.chatrooms()[0];
            
          if (selectedChatroom) {
            this.changeChatroom(selectedChatroom);
          }
        });
      }
    });

    this.chatSvc.receivedMessage$.subscribe(message => {
      if (message) {
        if (message.chatroomId === this.selectedChatroom()?.id) {
          this.chatMessages.update(m => [message, ...m]);
        }

        this.chatrooms().forEach(cr => {
          if (cr.id === message.chatroomId) {
            cr.latestMessage = message;
          }
        });
      }
    });
  }

  ngOnDestroy(): void {
    this.chatSvc.closeConnection();
    window.removeEventListener('beforeunload', this.chatSvc.closeConnection.bind(this));
  }

  changeChatroom(chatroom: Chatroom): void {
    if (chatroom.id === this.selectedChatroom()?.id) {
      return;
    }

    this.selectedChatroom.set(chatroom);

    this.chatMessageApiSvc.searchChatMessages({
      searchText: '',
      chatroomId: chatroom.id,
      pagedRequest: {
        pageIndex: 1,
        pageSize: 11
      }
    }).subscribe(result => {
      this.chatMessages.set(result.items);
    });
  }

  onKeyPress(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      this.sendMessage();
    }
  }

  sendMessage(): void {
    if (!this.inputMessage) {
      return;
    }

    this.chatSvc.sendMessage({
      chatroomId: this.selectedChatroom()!.id,
      message: this.inputMessage,
      userId: this.currentUser()!.id
    });

    this.inputMessage = '';
  }
}
