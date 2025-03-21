import { Component, Input, OnInit } from '@angular/core';
import { UserAvatarComponent } from "../user-avatar/user-avatar.component";
import { MatIconModule } from "@angular/material/icon";
import { Post } from '../../common/models/post.model';
import { User } from '../../common/models/user.model';
import { AuthService } from '../../common/services/auth.service';
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

  constructor(private authSvc: AuthService){}

  ngOnInit(): void {
    this.currentUser = this.authSvc.getCurrentUser();
  }
}
