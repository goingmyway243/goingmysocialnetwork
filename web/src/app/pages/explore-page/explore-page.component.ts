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

@Component({
  selector: 'app-explore-page',
  standalone: true,
  imports: [MatTabsModule, MatIconModule, PostItemComponent, UserItemComponent],
  templateUrl: './explore-page.component.html',
  styleUrl: './explore-page.component.scss'
})
export class ExplorePageComponent implements OnInit {
  searchText = signal<string | null>(null);
  postItems = signal<Post[]>([]);
  userItems = signal<User[]>([]);
  currentUserId = signal('');

  _searchPostsIndex = 0;
  _searchUsersIndex = 0;
  _currentTabIndex = 0;


  constructor(
    private route: ActivatedRoute,
    private postApiSvc: PostApiService,
    private userApiSvc: UserApiService,
    private authSvc: AuthService
  ) {
    effect(() => {
      if (this.searchText() || this.searchText() === '') {
        this.performSearch();
      }
    });
  }

  ngOnInit(): void {
    this.route.queryParamMap.subscribe(query => this.searchText.set(query.get('q') ?? ''));
    this.authSvc.currentUser$.subscribe(user => this.currentUserId.set(user?.id ?? ''));
  }

  onSelectedTabChange(index: number): void {
    this._currentTabIndex = index;
    this.performSearch();
  }

  performSearch(): void {
    switch (this._currentTabIndex) {
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

  performSearchPosts(pageIndex: number = 0): void {
    const searchPostSub = this.postApiSvc.searchPosts({
      searchText: this.searchText() ?? '',
      pagedRequest: {
        pageIndex: pageIndex,
        pageSize: 10,
      },
      currentUserId: this.currentUserId()
    }).subscribe(result => {
      this.postItems.update(current => {
        pageIndex === 0 ? current = result.items : current.push(...result.items);
        return current;
      });

      searchPostSub.unsubscribe();
    });
  }

  performSearchUsers(pageIndex: number = 0): void {
    const searchUserSub = this.userApiSvc.searchUsers({
      searchText: this.searchText() ?? '',
      includeFriendship: true,
      requestUserId: this.currentUserId(),
      pagedRequest: {
        pageIndex: pageIndex,
        pageSize: 10,
      }
    }).subscribe(result => {
      this.userItems.update(current => {
        pageIndex === 0 ? current = result.items : current.push(...result.items);
        return current;
      });

      searchUserSub.unsubscribe();
    });
  }
}
