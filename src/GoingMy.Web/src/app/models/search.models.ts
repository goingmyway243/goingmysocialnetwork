// ── Search result models matching GoingMy.Search.API DTOs ────────

export interface SearchResultDto<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface UserSearchResult {
  id: string;
  username: string;
  firstName: string;
  lastName: string;
  bio?: string;
  avatarUrl?: string;
  location?: string;
  followersCount: number;
  isVerified: boolean;
  isPrivate: boolean;
}

export interface PostSearchResult {
  id: string;
  userId: string;
  username: string;
  content: string;
  likes: number;
  comments: number;
  createdAt: string;
  mediaAttachments?: string[];
}

export interface TrendingPost {
  postId: string;
  userId: string;
  username: string;
  content: string;
  likes: number;
  comments: number;
  engagementScore: number;
  createdAt: string;
  mediaAttachments?: string[];
}

export interface SuggestionResult {
  text: string;
  type: 'user' | 'post';
}

export interface SearchAllResult {
  users: UserSearchResult[];
  posts: PostSearchResult[];
}

export type SearchType = 'all' | 'users' | 'posts';
export type SortBy = 'relevance' | 'recent';
export type TimeWindow = 'day' | 'week' | 'month';
