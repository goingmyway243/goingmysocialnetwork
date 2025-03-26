import { Component, OnInit, signal, ViewEncapsulation } from '@angular/core';
import { SearchBarComponent } from "../search-bar/search-bar.component";
import { MessageItemComponent } from "../message-item/message-item.component";
import { AuthService } from '../../common/services/auth.service';
import { Chatroom } from '../../common/models/chatroom.model';
import { ChatroomApiService } from '../../common/services/chatroom-api.service';

@Component({
  selector: 'message-box',
  standalone: true,
  imports: [SearchBarComponent, MessageItemComponent],
  templateUrl: './message-box.component.html',
  styleUrl: './message-box.component.scss',
  encapsulation: ViewEncapsulation.None
})
export class MessageBoxComponent implements OnInit {
  chatrooms = signal<Chatroom[]>([]);

  constructor(
    private authSvc: AuthService,
    private chatroomApiSvc: ChatroomApiService
  ) { }

  ngOnInit(): void {
    this.authSvc.currentUser$.subscribe(user => {
      if (user) {
        this.chatroomApiSvc.searchChatrooms({
          searchText: '',
          userId: user.id,
          pagedRequest: {
            pageIndex: 0,
            pageSize: 10
          }
        }).subscribe(result =>{
          this.chatrooms.set(result.items);
        });
      }
    });
  }
}
