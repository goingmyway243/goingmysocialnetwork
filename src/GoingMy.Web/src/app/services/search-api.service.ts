import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
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
  private readonly _base = `${environment.apiGatewayUrl}/api/search`;

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
}
