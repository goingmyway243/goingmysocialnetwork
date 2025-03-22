import { Component, computed, Input, signal } from '@angular/core';
import { UserAvatarComponent } from "../user-avatar/user-avatar.component";
import { Friendship } from '../../common/models/friendship.model';
import { FriendshipApiService } from '../../common/services/friendship-api.service';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'request-item',
  standalone: true,
  imports: [UserAvatarComponent, MatIconModule],
  templateUrl: './request-item.component.html',
  styleUrl: './request-item.component.scss'
})
export class RequestItemComponent {
  @Input() requestData!: Friendship;

  showNotification = signal(false);
  statusMessage = signal('');
  notificationMessage = computed(() => `You have ${this.statusMessage()} this request`);

  constructor(private friendshipApiSvc: FriendshipApiService) { }

  acceptRequest(): void {
    this.friendshipApiSvc.acceptFriendRequest(this.requestData.id)
      .subscribe(() => {
        this.showNotification.set(true);
        this.statusMessage.set('accepted');
      });
  }

  declineRequest(): void {
    this.friendshipApiSvc.declineFriendRequest(this.requestData.id)
      .subscribe(() => {
        this.showNotification.set(true);
        this.statusMessage.set('declined');
      });
  }
}
