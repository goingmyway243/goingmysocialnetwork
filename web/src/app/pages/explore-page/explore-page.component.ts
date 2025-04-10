import { Component, effect, OnInit, signal } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { ActivatedRoute } from '@angular/router';
import { PostApiService } from '../../common/services/post-api.service';
import { UserApiService } from '../../common/services/user-api.service';
import { Post } from '../../common/models/post.model';
import { User } from '../../common/models/user.model';
import { PostItemComponent } from "../../components/post-item/post-item.component";
import { UserItemComponent } from "../../components/user-item/user-item.component";
import { AuthService } from '../../common/services/auth.service';
import { Util } from '../../common/helpers/util';
import { AppLoaderComponent } from "../../components/app-loader/app-loader.component";

@Component({
  selector: 'app-explore-page',
  standalone: true,
  imports: [MatTabsModule, MatIconModule, PostItemComponent, UserItemComponent, AppLoaderComponent],
  templateUrl: './explore-page.component.html',
  styleUrl: './explore-page.component.scss'
})
export class ExplorePageComponent implements OnInit {
  searchText = signal<string | null>(null);
  postItems = signal<Post[]>([]);
  userItems = signal<User[]>([]);
  currentUserId = signal('');
  isLoading = signal(false);

  lastPostTimestamp: Date = Util.getUtcNow();
  gotLastPost: boolean = false;
  lastUserTimestamp: Date = Util.getUtcNow();
  gotLastUser: boolean = false;

  currentTabIndex: number = 0;
  initiated: boolean = false;

  constructor(
    private route: ActivatedRoute,
    private postApiSvc: PostApiService,
    private userApiSvc: UserApiService,
    private authSvc: AuthService
  ) {
    effect(() => {
      if (this.currentUserId() && (this.searchText() || this.searchText() === '')) {
        this.performSearch();
      }
    });
  }

  ngOnInit(): void {
    this.route.queryParamMap.subscribe(query => this.searchText.set(query.get('q') ?? ''));
    this.authSvc.currentUser$.subscribe(user => this.currentUserId.set(user?.id ?? ''));
    window.addEventListener('scroll', this.onScroll.bind(this));
  }

  onSelectedTabChange(index: number): void {
    this.currentTabIndex = index;
    this.performSearch();
  }

  performSearch(): void {
    switch (this.currentTabIndex) {
      case 0:
        this.performSearchPosts();
        break;

      case 1:
        this.performSearchUsers();
        break;

      default:
        break;
    }
  }

  performSearchPosts(): void {
    this.postApiSvc.searchPosts({
      searchText: this.searchText() ?? '',
      pagedRequest: {
        cursorTimestamp: this.lastPostTimestamp,
        pageSize: 10,
      },
      currentUserId: this.currentUserId()
    }).subscribe(result => {
      this.postItems.update(current => [...current, ...result.items]);

      const lastItem = result.items[result.items.length - 1];
      if (lastItem) {
        this.lastPostTimestamp = lastItem.modifiedAt ?? lastItem.createdAt;
      }

      this.gotLastPost = result.totalCount <= this.postItems().length;

      this.initiated = true;
    });
  }

  performSearchUsers(): void {
    this.userApiSvc.searchUsers({
      searchText: this.searchText() ?? '',
      includeFriendship: true,
      requestUserId: this.currentUserId(),
      pagedRequest: {
        cursorTimestamp: this.lastUserTimestamp,
        pageSize: 10,
      }
    }).subscribe(result => {
      this.userItems.update(current => [...current, ...result.items]);

      const lastItem = result.items[result.items.length - 1];
      if (lastItem) {
        this.lastUserTimestamp = lastItem.modifiedAt ?? lastItem.createdAt;
      }

      this.gotLastUser = result.totalCount <= this.userItems().length;

      this.initiated = true;
    });
  }

  onScroll(): void {
    let gotLastItem = true;
    const element = document.documentElement;

    switch (this.currentTabIndex) {
      case 0:
        gotLastItem = this.gotLastPost;
        break;

      case 1:
        gotLastItem = this.gotLastUser;
        break;

      default:
        break;
    }

    if (gotLastItem) {
      return;
    }

    const atBottom = element.scrollHeight - element.scrollTop === element.clientHeight;
    
    // load more items
    if (atBottom && !this.isLoading()) {
      this.performSearch();
    }
  }
}
