import { Component, computed, ElementRef, OnDestroy, OnInit, signal, ViewChild } from '@angular/core';
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
import { Util } from '../../common/helpers/util';
import { AppLoaderComponent } from "../../components/app-loader/app-loader.component";

@Component({
  selector: 'app-message-page',
  standalone: true,
  imports: [FormsModule, MatIconModule, UserAvatarComponent, MessageItemComponent, AppLoaderComponent],
  templateUrl: './message-page.component.html',
  styleUrl: './message-page.component.scss'
})
export class MessagePageComponent implements OnInit, OnDestroy {
  @ViewChild('messageScrollDiv') messageScrollDiv!: ElementRef;
  @ViewChild('chatroomScrollDiv') chatroomScrollDiv!: ElementRef;

  currentUser = signal<User | null>(null);
  chatrooms = signal<Chatroom[]>([]);
  selectedChatroom = signal<Chatroom | null>(null);
  chatMessages = signal<ChatMessage[]>([]);
  chatUser = computed(() => this.selectedChatroom()?.participants?.filter(p => p.id !== this.currentUser()?.id)[0] ?? null);
  isMessageLoading = signal(false);
  isChatroomLoading = signal(false);

  initiated: boolean = false;
  inputMessage: string = '';
  chatroomIdParam: string = '';
  lastMessageTimestamp: Date = Util.getUtcNow();
  gotLastMessage: boolean = false;
  searchChatroomIndex: number = 0;
  gotLastChatroom: boolean = false;

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
            pageIndex: this.searchChatroomIndex,
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

    // Reset message's values
    this.chatMessages.set([]);
    this.lastMessageTimestamp = Util.getUtcNow();

    this.loadMessages(chatroom);
  }

  private loadMessages(chatroom: Chatroom) {
    this.isMessageLoading.set(true);

    this.chatMessageApiSvc.searchChatMessages({
      searchText: '',
      chatroomId: chatroom.id,
      pagedRequest: {
        cursorTimestamp: this.lastMessageTimestamp,
        pageSize: 20
      }
    }).subscribe(result => {
      this.chatMessages.update(m => [...m, ...result.items]);

      const lastItem = result.items[result.items.length - 1];
      if (lastItem) {
        this.lastMessageTimestamp = lastItem.createdAt;
      }

      this.gotLastMessage = result.totalCount <= this.chatMessages().length;

      this.isMessageLoading.set(false);
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

  onChatroomContainerScroll() {
    if (this.gotLastChatroom) {
      return;
    }

    const element = this.chatroomScrollDiv.nativeElement;
    const atBottom = element.scrollHeight - element.scrollTop === element.clientHeight;

    // load more chatrooms
    if (this.currentUser() && atBottom && !this.isChatroomLoading()) {
      this.isChatroomLoading.set(true);
      
      this.chatroomApiSvc.searchChatrooms({
        userId: this.currentUser()!.id,
        searchText: '',
        pagedRequest: {
          pageIndex: ++this.searchChatroomIndex,
          pageSize: 20
        }
      }).subscribe(result => {
        this.chatrooms.update(cr => [...cr, ...result.items]);
        this.isChatroomLoading.set(false);
        this.gotLastChatroom = result.totalCount <= this.chatrooms().length;
      });
    }
  }

  onMesasageContainerScroll() {
    if (this.gotLastMessage) {
      return;
    }

    const element = this.messageScrollDiv.nativeElement;
    const atTop = element.scrollHeight + element.scrollTop === element.clientHeight;

    // load more messages
    if (this.selectedChatroom() && atTop && !this.isMessageLoading()) {
      this.loadMessages(this.selectedChatroom()!);
    }
  }
}
