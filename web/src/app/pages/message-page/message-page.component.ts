import { Component, computed, OnInit, signal } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { UserAvatarComponent } from "../../components/user-avatar/user-avatar.component";
import { MessageItemComponent } from "../../components/message-item/message-item.component";
import { AuthService } from '../../common/services/auth.service';
import { ChatroomApiService } from '../../common/services/chatroom-api.service';
import { User } from '../../common/models/user.model';
import { Chatroom } from '../../common/models/chatroom.model';

@Component({
  selector: 'app-message-page',
  standalone: true,
  imports: [MatIconModule, UserAvatarComponent, MessageItemComponent],
  templateUrl: './message-page.component.html',
  styleUrl: './message-page.component.scss'
})
export class MessagePageComponent implements OnInit {
  currentUser = signal<User | null>(null);
  chatrooms = signal<Chatroom[]>([]);
  selectedChatroom = signal<Chatroom | null>(null);
  chatUser = computed(() => this.selectedChatroom()?.participants?.filter(p => p.id !== this.currentUser()?.id)[0] ?? null);

  constructor(
    private authSvc: AuthService,
    private chatroomApiSvc: ChatroomApiService
  ) { }

  ngOnInit(): void {
    this.authSvc.currentUser$.subscribe(user => {
      this.currentUser.set(user);

      if (user) {
        this.chatroomApiSvc.searchChatrooms({
          userId: user.id,
          searchText: '',
          pagedRequest: {
            pageIndex: 0,
            pageSize: 10
          }
        }).subscribe(result => {
          this.chatrooms.set(result.items);
          this.selectedChatroom.set(this.chatrooms()[0]);
        });
      }
    });
  }

  onMessageItemClick(chatroom: Chatroom) {
    if (chatroom.id === this.selectedChatroom()?.id) {
      return;
    }
    
    this.selectedChatroom.set(chatroom);
  }
}
