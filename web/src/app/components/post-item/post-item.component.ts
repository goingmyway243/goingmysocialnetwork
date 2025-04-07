import { Component, ElementRef, Input, OnInit, signal, ViewChild } from '@angular/core';
import { UserAvatarComponent } from "../user-avatar/user-avatar.component";
import { MatIconModule } from "@angular/material/icon";
import { Post } from '../../common/models/post.model';
import { User } from '../../common/models/user.model';
import { AuthService } from '../../common/services/auth.service';
import { LikeApiService } from '../../common/services/like.service';
import { CommentApiService } from '../../common/services/comment-api.service';
import { Comment } from '../../common/models/comment.model';
import { FormsModule } from '@angular/forms';
import { Util } from '../../common/helpers/util';

@Component({
  selector: 'post-item',
  standalone: true,
  imports: [UserAvatarComponent, MatIconModule, FormsModule],
  templateUrl: './post-item.component.html',
  styleUrl: './post-item.component.scss'
})
export class PostItemComponent implements OnInit {
  @ViewChild('commentInput') commentInput?: ElementRef;
  @Input() postData!: Post;

  postComments = signal<Comment[]>([]);

  currentUser: User | null = null;
  commentInputText: string = '';
  liked: boolean = false;
  showComments: boolean = false;
  timeDiff: string = '';

  constructor(
    private authSvc: AuthService,
    private likeApiSvc: LikeApiService,
    private commentApiSvc: CommentApiService
  ) { }

  ngOnInit(): void {
    this.currentUser = this.authSvc.getCurrentUser();
    this.liked = !!this.postData.isLikedByUser;
    this.timeDiff = this.getTimeDiff(this.postData.modifiedAt ?? this.postData.createdAt);
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

  commentPost(): void {
    if (this.commentInputText && this.currentUser) {
      const sub = this.commentApiSvc.createComment({
        comment: this.commentInputText,
        postId: this.postData.id,
        userId: this.currentUser.id
      }).subscribe(result => {
        result.user = this.currentUser!;
        this.postComments.update(m => [result, ...m]);
        this.commentInputText = '';
        this.postData.commentCount++;
        sub.unsubscribe();
      });
    }
  }

  onCommentInputKeyPress(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      this.commentPost();
    }
  }

  onCommentButtonClick(): void {
    if (!this.showComments) {
      this.showComments = true;
      this.commentApiSvc.searchComments({
        postId: this.postData.id,
        pagedRequest: {
          pageIndex: 0,
          pageSize: 10
        }
      }).subscribe(result => this.postComments.set(result.items));
    }

    setTimeout(() => {
      this.commentInput?.nativeElement.focus();
    }, 200);
  }

  getTimeDiff(value: Date): string {
    return Util.getTimeDiff(Util.getLocalDate(value));
  }
}
