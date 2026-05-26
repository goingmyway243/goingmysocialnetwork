import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, map, of, switchMap } from 'rxjs';
import { environment } from '../../environments/environment';
import { Post, Comment, Like } from '../models/post.model';
import { UserApiService, UserProfileSummary } from './user-api.service';

/** Request DTO for creating a post. */
export interface CreatePostRequest {
  content: string;
}

/** Request DTO for creating a post with media (initiates saga). */
export interface CreatePostWithMediaRequest {
  content: string;
  mediaFileIds: string[];
}

/** Request DTO for updating a post. */
export interface UpdatePostRequest {
  content: string;
}

/** Response DTO from GetPosts endpoint (wraps posts with user context). */
export interface GetPostsResponse {
  userId: string;
  username: string;
  posts: Post[];
}

/** Response DTO from CreatePost and UpdatePost endpoints. */
export interface PostResponse {
  message: string;
  post: Post;
}

@Injectable({
  providedIn: 'root'
})
export class PostApiService {
  private readonly _http = inject(HttpClient);
  private readonly _userApi = inject(UserApiService);
  private readonly _baseUrl = `${environment.apiGatewayUrl}/api/posts`;
  private readonly _profileCache = new Map<string, UserProfileSummary | null>();

  // ── 1. Get Posts ─────────────────────────────────────────────

  /** GET /api/posts — Retrieves all posts for the authenticated user. */
  getPosts(): Observable<GetPostsResponse> {
    return this._http.get<GetPostsResponse>(this._baseUrl).pipe(
      switchMap(response => this._hydratePostsWithAuthorProfiles(response.posts).pipe(
        map(posts => ({ ...response, posts }))
      ))
    );
  }

  // ── 2. Get Post by ID ────────────────────────────────────────

  /** GET /api/posts/{id} — Retrieves a specific post by ID. */
  getPostById(id: string): Observable<Post> {
    return this._http.get<Post>(`${this._baseUrl}/${id}`).pipe(
      switchMap(post => this._hydratePostsWithAuthorProfiles([post]).pipe(
        map(posts => posts[0])
      ))
    );
  }

  // ── 3. Create Post ───────────────────────────────────────────

  /** POST /api/posts — Creates a new post. */
  createPost(request: CreatePostRequest): Observable<PostResponse> {
    return this._http.post<PostResponse>(this._baseUrl, request);
  }

  /** POST /api/posts/with-media — Creates a post with media (initiates saga). */
  createPostWithMedia(request: CreatePostWithMediaRequest): Observable<PostResponse> {
    return this._http.post<PostResponse>(`${this._baseUrl}/with-media`, request);
  }

  // ── 4. Update Post ───────────────────────────────────────────

  /** PUT /api/posts/{id} — Updates an existing post. */
  updatePost(id: string, request: UpdatePostRequest): Observable<PostResponse> {
    return this._http.put<PostResponse>(`${this._baseUrl}/${id}`, request);
  }

  // ── 5. Delete Post ───────────────────────────────────────────

  /** DELETE /api/posts/{id} — Deletes a post. */
  deletePost(id: string): Observable<{ message: string }> {
    return this._http.delete<{ message: string }>(`${this._baseUrl}/${id}`);
  }

  // ── 6. Likes ─────────────────────────────────────────────────

  /** POST /api/posts/{id}/likes — Likes a post. */
  likePost(postId: string): Observable<Like> {
    return this._http.post<Like>(`${this._baseUrl}/${postId}/likes`, {});
  }

  /** DELETE /api/posts/{id}/likes — Unlikes a post. */
  unlikePost(postId: string): Observable<void> {
    return this._http.delete<void>(`${this._baseUrl}/${postId}/likes`);
  }

  /** GET /api/posts/{id}/likes — Retrieves all likes for a post. */
  getPostLikes(postId: string): Observable<Like[]> {
    return this._http.get<Like[]>(`${this._baseUrl}/${postId}/likes`);
  }

  // ── 7. Comments ──────────────────────────────────────────────

  /** GET /api/posts/{postId}/comments — Retrieves all comments for a post. */
  getComments(postId: string): Observable<Comment[]> {
    return this._http.get<Comment[]>(`${this._baseUrl}/${postId}/comments`);
  }

  /** POST /api/posts/{postId}/comments — Adds a comment to a post. */
  addComment(postId: string, content: string): Observable<Comment> {
    return this._http.post<Comment>(`${this._baseUrl}/${postId}/comments`, { content });
  }

  /** PUT /api/posts/{postId}/comments/{commentId} — Updates a comment. */
  updateComment(postId: string, commentId: string, content: string): Observable<Comment> {
    return this._http.put<Comment>(`${this._baseUrl}/${postId}/comments/${commentId}`, { content });
  }

  /** DELETE /api/posts/{postId}/comments/{commentId} — Deletes a comment. */
  deleteComment(postId: string, commentId: string): Observable<void> {
    return this._http.delete<void>(`${this._baseUrl}/${postId}/comments/${commentId}`);
  }

  // ── 8. User Profile Posts ────────────────────────────────────

  /** GET /api/posts/user/{userId} — Retrieves paginated posts by a specific user. */
  getUserPosts(userId: string, page = 1, pageSize = 20): Observable<Post[]> {
    return this._http.get<Post[]>(`${this._baseUrl}/user/${userId}?page=${page}&pageSize=${pageSize}`).pipe(
      switchMap(posts => this._hydratePostsWithAuthorProfiles(posts))
    );
  }

  /** GET /api/posts/user/{userId}/likes — Retrieves posts liked by a specific user. */
  getUserLikedPosts(userId: string, page = 1, pageSize = 20): Observable<Post[]> {
    return this._http.get<Post[]>(`${this._baseUrl}/user/${userId}/likes?page=${page}&pageSize=${pageSize}`).pipe(
      switchMap(posts => this._hydratePostsWithAuthorProfiles(posts))
    );
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

