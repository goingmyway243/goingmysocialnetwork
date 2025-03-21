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

  _searchPostsIndex = 0;
  _searchUsersIndex = 0;
  _currentTabIndex = 0;


  constructor(
    private route: ActivatedRoute,
    private postApiSvc: PostApiService,
    private userApiSvc: UserApiService
  ) {
    effect(() => {
      if (this.searchText() !== null) {
        this.performSearch();
      }
    });
  }

  ngOnInit(): void {
    this.route.queryParamMap.subscribe(query => this.searchText.set(query.get('q') ?? ''));
  }

  onSelectedTabChange(index: number): void {
    this._currentTabIndex = index;
    this.performSearch();
  }

  performSearch(): void {
    switch (this._currentTabIndex) {
      case 0:
        if (this.postItems.length === 0) {
          this.performSearchPosts();
        }
        break;

      case 1:
        if (this.userItems.length === 0) {
          this.performSearchUsers();
        }

        break;

      default:
        break;
    }
  }

  performSearchPosts(pageIndex: number = 0): void {
    this.postApiSvc.searchPosts({
      pageIndex: pageIndex,
      pageSize: 10,
      searchText: this.searchText() ?? ''
    }).subscribe(result => {
      this.postItems.update(current => {
        pageIndex === 0 ? current = result.items : current.push(...result.items);
        return current;
      });
    });
  }

  performSearchUsers(pageIndex: number = 0): void {
    this.userApiSvc.searchUsers({
      pageIndex: pageIndex,
      pageSize: 10,
      searchText: this.searchText() ?? ''
    }).subscribe(result => {
      this.userItems.update(current => {
        pageIndex === 0 ? current = result.items : current.push(...result.items);
        return current;
      });
    });
  }
}
