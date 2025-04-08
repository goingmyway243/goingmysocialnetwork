import { Component, Input, OnInit } from '@angular/core';
import { UserAvatarComponent } from "../user-avatar/user-avatar.component";
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { User } from '../../common/models/user.model';
import { FriendshipStatus } from '../../common/enums/friendship-status.enum';
import { AuthService } from '../../common/services/auth.service';
import { FriendshipApiService } from '../../common/services/friendship-api.service';
import { Router } from '@angular/router';

@Component({
  selector: 'user-item',
  standalone: true,
  imports: [UserAvatarComponent, MatButtonModule, MatIconModule],
  templateUrl: './user-item.component.html',
  styleUrl: './user-item.component.scss'
})
export class UserItemComponent implements OnInit {
  @Input() userData!: User;

  FriendshipStatus = FriendshipStatus;
  currentUserId: string = '';

  constructor(
    private router: Router,
    private authSvc: AuthService,
    private friendshipApiSvc: FriendshipApiService
  ) { }

  ngOnInit(): void {
    this.currentUserId = this.authSvc.getCurrentUserId();
  }

  addFriend(friend: User): void {
    this.friendshipApiSvc.sendFriendRequest({
      userId: this.currentUserId,
      friendId: friend.id
    }).subscribe(fs => {
      friend.friendship = fs;
    })
  }

  unFriend(friend: User): void {
    this.removeFriendRequest(friend);
  }

  removeFriendRequest(friend: User): void {
    this.friendshipApiSvc.removeFriend(friend.friendship!.id).subscribe(() => friend.friendship = undefined);
  }

  public navigateToProfile(): void {
    this.router.navigate(['/profile']);
  }
}
