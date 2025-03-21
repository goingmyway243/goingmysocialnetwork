import { Component, computed, OnInit, signal } from '@angular/core';
import { PostItemComponent } from "../../components/post-item/post-item.component";
import { CreatePostComponent } from "../../components/create-post/create-post.component";
import { Router } from '@angular/router';
import { User } from '../../common/models/user.model';
import { AuthService } from '../../common/services/auth.service';
import { PostApiService } from '../../common/services/post-api.service';
import { Post } from '../../common/models/post.model';

@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [PostItemComponent, CreatePostComponent],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.scss'
})
export class HomePageComponent implements OnInit {
  currentUser = signal<User | null>(null);
  placeholderText = computed(() =>
    this.currentUser()
      ? `What's on your mind, ${this.currentUser()?.fullName}?`
      : 'What\'s on your mind?'
  );
  postItems = signal<Post[]>([]);

  constructor(
    private router: Router,
    private authSvc: AuthService,
    private postApiSvc: PostApiService
  ) { }

  ngOnInit(): void {
    this.authSvc.currentUser$.subscribe(user => {
      this.currentUser.set(user);
    });

    this.postApiSvc.searchPosts({
      pageIndex: 0,
      pageSize: 10
    }).subscribe(result => {
      this.postItems.set(result.items);
    });
  }

  onPostCreated(post: Post) {
    this.postItems.update(current => {
      current.unshift(post);
      return current;
    });
  }
}
