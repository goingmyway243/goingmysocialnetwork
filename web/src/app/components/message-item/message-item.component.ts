import { Component, Input, OnInit } from '@angular/core';
import { Chatroom } from '../../common/models/chatroom.model';
import { UserAvatarComponent } from "../user-avatar/user-avatar.component";
import { User } from '../../common/models/user.model';
import { AuthService } from '../../common/services/auth.service';
import { CommonModule } from '@angular/common';
import { Util } from '../../common/helpers/util';

@Component({
  selector: 'message-item',
  standalone: true,
  imports: [UserAvatarComponent, CommonModule],
  templateUrl: './message-item.component.html',
  styleUrl: './message-item.component.scss'
})
export class MessageItemComponent implements OnInit {
  @Input() chatroomData!: Chatroom;

  chatUser: User | null = null;

  constructor(private authSvc: AuthService) { }

  ngOnInit(): void {
    const currentUserId = this.authSvc.getCurrentUserId();
    this.chatUser = this.chatroomData.participants.filter(p => p.id !== currentUserId)[0];
  }

  getLocalDate(date?: Date): Date {
    if (!date) {
      return new Date();
    }

    return Util.getLocalDate(date);
  }
}
