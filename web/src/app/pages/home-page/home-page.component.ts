import { Component, computed, OnInit, signal } from '@angular/core';
import { PostItemComponent } from "../../components/post-item/post-item.component";
import { CreatePostComponent } from "../../components/create-post/create-post.component";
import { Router } from '@angular/router';
import { User } from '../../common/models/user.model';
import { AuthService } from '../../common/services/auth.service';
import { PostApiService } from '../../common/services/post-api.service';
import { Post } from '../../common/models/post.model';
import { Util } from '../../common/helpers/util';
import { AppLoaderComponent } from "../../components/app-loader/app-loader.component";

@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [PostItemComponent, CreatePostComponent, AppLoaderComponent],
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
  isLoading = signal(false);

  postLoaded: boolean = false;
  lastPostTimestamp: Date = Util.getUtcNow();
  gotLastPost: boolean = false;

  constructor(
    private router: Router,
    private authSvc: AuthService,
    private postApiSvc: PostApiService,
  ) { }

  ngOnInit(): void {
    window.addEventListener('scroll', this.onScroll.bind(this));

    this.authSvc.currentUser$.subscribe(user => {
      if (!user) {
        return;
      }

      this.currentUser.set(user);

      this.postApiSvc.searchPosts({
        pagedRequest: {
          cursorTimestamp: Util.getUtcNow(),
          pageSize: 10
        },
        currentUserId: this.currentUser()!.id
      }).subscribe(result => {
        this.postItems.set(result.items);
        this.postLoaded = true;

        const lastItem = result.items[result.items.length - 1];
        if (lastItem) {
          this.lastPostTimestamp = lastItem.modifiedAt ?? lastItem.createdAt;
        }

        this.gotLastPost = result.totalCount <= this.postItems().length;
      });
    });
  }

  onPostCreated(post: Post) {
    this.postItems.update(current => {
      current.unshift(post);
      return current;
    });
  }

  onScroll(): void {
    if (this.gotLastPost) {
      return;
    }

    const element = document.documentElement;
    const atBottom = element.scrollHeight - element.scrollTop === element.clientHeight;

    // load more items
    if (atBottom && !this.isLoading()) {
      this.postApiSvc.searchPosts({
        pagedRequest: {
          cursorTimestamp: this.lastPostTimestamp,
          pageSize: 10
        },
        currentUserId: this.currentUser()!.id
      }).subscribe(result => {
        this.postItems.set(result.items);

        const lastItem = result.items[result.items.length - 1];
        if (lastItem) {
          this.lastPostTimestamp = lastItem.modifiedAt ?? lastItem.createdAt;
        }

        this.gotLastPost = result.totalCount <= this.postItems().length;
      });
    }
  }
}
