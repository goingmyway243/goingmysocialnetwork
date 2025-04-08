import { Component, OnInit, signal } from '@angular/core';
import { RequestItemComponent } from "../request-item/request-item.component";
import { AuthService } from '../../common/services/auth.service';
import { FriendshipApiService } from '../../common/services/friendship-api.service';
import { Friendship } from '../../common/models/friendship.model';

@Component({
  selector: 'request-box',
  standalone: true,
  imports: [RequestItemComponent],
  templateUrl: './request-box.component.html',
  styleUrl: './request-box.component.scss'
})
export class RequestBoxComponent implements OnInit {
  pendingRequests = signal<Friendship[]>([]);

  constructor(
    private authSvc: AuthService,
    private friendshipApiSvc: FriendshipApiService
  ) { }

  ngOnInit(): void {
    this.authSvc.currentUser$.subscribe(user => {
      if (user && this.authSvc.isAuthenticated()) {
        this.friendshipApiSvc.getPendingRequests(user.id, { pageIndex: 0, pageSize: 2 })
          .subscribe(fs => {
            this.pendingRequests.set(fs.items);
          });
      }
    });
  }
}
