import { Component, Input, OnInit } from '@angular/core';
import { UserAvatarComponent } from "../user-avatar/user-avatar.component";
import { MatIconModule } from "@angular/material/icon";
import { Post } from '../../common/models/post.model';
import { User } from '../../common/models/user.model';
import { AuthService } from '../../common/services/auth.service';
import { LikeApiService } from '../../common/services/like.service';
@Component({
  selector: 'post-item',
  standalone: true,
  imports: [UserAvatarComponent, MatIconModule],
  templateUrl: './post-item.component.html',
  styleUrl: './post-item.component.scss'
})
export class PostItemComponent implements OnInit {
  @Input() postData!: Post;

  currentUser: User | null = null;
  liked: boolean = false;

  constructor(
    private authSvc: AuthService,
    private likeApiSvc: LikeApiService
  ) { }

  ngOnInit(): void {
    this.currentUser = this.authSvc.getCurrentUser();
    this.liked = !!this.postData.isLikedByUser;
  }

  likePost(): void {
    if (!this.currentUser) {
      return;
    }

    this.likeApiSvc.toggleLike({
      postId: this.postData.id,
      userId: this.currentUser.id,
      isLiked: !this.liked
    }).subscribe(result => {
      if (this.postData.likeCount !== result) {
        this.postData.likeCount = result;
        this.liked = !this.liked;
      }
    });
  }
}
