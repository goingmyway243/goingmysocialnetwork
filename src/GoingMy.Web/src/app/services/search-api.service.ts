import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, map, of, switchMap } from 'rxjs';
import { environment } from '../../environments/environment';
import { Post } from '../models/post.model';
import {
  SearchResultDto,
  UserSearchResult,
  PostSearchResult,
  TrendingPost,
  SuggestionResult,
  SearchAllResult,
  SearchType,
  SortBy,
  TimeWindow
} from '../models/search.models';
import { UserApiService, UserProfileSummary } from './user-api.service';

export interface SearchAllHydratedResult {
  users: UserSearchResult[];
  posts: Post[];
}

export interface SearchParams {
  q?: string;
  type?: SearchType;
  dateFrom?: string;
  dateTo?: string;
  location?: string;
  sortBy?: SortBy;
  page?: number;
  pageSize?: number;
}

@Injectable({
  providedIn: 'root'
})
export class SearchApiService {
  private readonly _http = inject(HttpClient);
  private readonly _userApi = inject(UserApiService);
  private readonly _base = `${environment.apiGatewayUrl}/api/search`;
  private readonly _profileCache = new Map<string, UserProfileSummary | null>();

  search(params: SearchParams): Observable<SearchResultDto<UserSearchResult> | SearchResultDto<PostSearchResult> | SearchAllResult> {
    let httpParams = new HttpParams();
    if (params.q) httpParams = httpParams.set('q', params.q);
    if (params.type) httpParams = httpParams.set('type', params.type);
    if (params.dateFrom) httpParams = httpParams.set('dateFrom', params.dateFrom);
    if (params.dateTo) httpParams = httpParams.set('dateTo', params.dateTo);
    if (params.location) httpParams = httpParams.set('location', params.location);
    if (params.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
    if (params.page != null) httpParams = httpParams.set('page', params.page);
    if (params.pageSize != null) httpParams = httpParams.set('pageSize', params.pageSize);
    return this._http.get<any>(this._base, { params: httpParams });
  }

  searchAll(params: Omit<SearchParams, 'type'>): Observable<SearchAllResult> {
    return this.search({ ...params, type: 'all' }) as Observable<SearchAllResult>;
  }

  searchAllWithHydratedPosts(params: Omit<SearchParams, 'type'>): Observable<SearchAllHydratedResult> {
    return this.searchAll(params).pipe(
      switchMap(result => {
        const mappedPosts = (result.posts ?? []).map(post => this._mapSearchResultToPost(post));
        return this._hydratePostsWithAuthorProfiles(mappedPosts).pipe(
          map(posts => ({
            users: result.users ?? [],
            posts
          }))
        );
      })
    );
  }

  searchUsers(params: Omit<SearchParams, 'type'>): Observable<SearchResultDto<UserSearchResult>> {
    return this.search({ ...params, type: 'users' }) as Observable<SearchResultDto<UserSearchResult>>;
  }

  searchPosts(params: Omit<SearchParams, 'type'>): Observable<SearchResultDto<PostSearchResult>> {
    return this.search({ ...params, type: 'posts' }) as Observable<SearchResultDto<PostSearchResult>>;
  }

  suggest(q: string, type?: 'users' | 'posts'): Observable<SuggestionResult[]> {
    let httpParams = new HttpParams().set('q', q);
    if (type) httpParams = httpParams.set('type', type);
    return this._http.get<SuggestionResult[]>(`${this._base}/suggest`, { params: httpParams });
  }

  getTrending(timeWindow: TimeWindow = 'week', size = 10): Observable<TrendingPost[]> {
    const params = new HttpParams()
      .set('timeWindow', timeWindow)
      .set('size', size);
    return this._http.get<TrendingPost[]>(`${this._base}/trending`, { params });
  }

  getTrendingHydrated(timeWindow: TimeWindow = 'week', size = 10): Observable<Post[]> {
    return this.getTrending(timeWindow, size).pipe(
      switchMap(posts => {
        const mappedPosts = posts.map(post => this._mapTrendingToPost(post));
        return this._hydratePostsWithAuthorProfiles(mappedPosts);
      })
    );
  }

  private _mapSearchResultToPost(r: PostSearchResult): Post {
    return {
      id: r.id,
      content: r.content,
      userId: r.userId,
      username: r.username,
      createdAt: r.createdAt,
      likes: r.likes,
      comments: r.comments,
      mediaAttachments: r.mediaAttachments
    };
  }

  private _mapTrendingToPost(t: TrendingPost): Post {
    return {
      id: t.postId,
      content: t.content,
      userId: t.userId,
      username: t.username,
      createdAt: t.createdAt,
      likes: t.likes,
      comments: t.comments,
      mediaAttachments: t.mediaAttachments
    };
  }

  private _hydratePostsWithAuthorProfiles(posts: Post[]): Observable<Post[]> {
    if (posts.length === 0) {
      return of(posts);
    }

    const userIds = [...new Set(posts.map(p => p.author?.id ?? p.userId).filter(Boolean))];
    const uncachedUserIds = userIds.filter(id => !this._profileCache.has(id));

    if (uncachedUserIds.length === 0) {
      return of(this._applyCachedProfiles(posts));
    }

    return this._userApi.getUserProfilesByIdsBatch(uncachedUserIds).pipe(
      catchError(() => of({} as Record<string, UserProfileSummary>)),
      map(results => {
        uncachedUserIds.forEach(userId => {
          this._profileCache.set(userId, results[userId] ?? null);
        });
        return this._applyCachedProfiles(posts);
      })
    );
  }

  private _applyCachedProfiles(posts: Post[]): Post[] {
    return posts.map(post => {
      const authorId = post.author?.id ?? post.userId;
      const cachedProfile = this._profileCache.get(authorId);

      if (cachedProfile === undefined) {
        return post;
      }

      const firstName = cachedProfile?.firstName ?? post.author?.firstName ?? post.username;
      const lastName = cachedProfile?.lastName ?? post.author?.lastName ?? '';
      const userName = cachedProfile?.username ?? post.author?.userName ?? post.username;

      return {
        ...post,
        author: {
          id: post.author?.id ?? post.userId,
          userName,
          firstName,
          lastName,
          avatarUrl: cachedProfile?.avatarUrl ?? undefined,
          isVerified: cachedProfile?.isVerified ?? false
        }
      };
    });
  }
}
