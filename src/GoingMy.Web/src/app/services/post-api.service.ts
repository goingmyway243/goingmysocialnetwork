import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Post, Comment, Like } from '../models/post.model';

/** Request DTO for creating a post. */
export interface CreatePostRequest {
  content: string;
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
  private readonly _baseUrl = `${environment.apiGatewayUrl}/api/posts`;

  // ── 1. Get Posts ─────────────────────────────────────────────

  /** GET /api/posts — Retrieves all posts for the authenticated user. */
  getPosts(): Observable<GetPostsResponse> {
    return this._http.get<GetPostsResponse>(this._baseUrl);
  }

  // ── 2. Get Post by ID ────────────────────────────────────────

  /** GET /api/posts/{id} — Retrieves a specific post by ID. */
  getPostById(id: string): Observable<Post> {
    return this._http.get<Post>(`${this._baseUrl}/${id}`);
  }

  // ── 3. Create Post ───────────────────────────────────────────

  /** POST /api/posts — Creates a new post. */
  createPost(request: CreatePostRequest): Observable<PostResponse> {
    return this._http.post<PostResponse>(this._baseUrl, request);
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
    return this._http.get<Post[]>(`${this._baseUrl}/user/${userId}?page=${page}&pageSize=${pageSize}`);
  }

  /** GET /api/posts/user/{userId}/likes — Retrieves posts liked by a specific user. */
  getUserLikedPosts(userId: string, page = 1, pageSize = 20): Observable<Post[]> {
    return this._http.get<Post[]>(`${this._baseUrl}/user/${userId}/likes?page=${page}&pageSize=${pageSize}`);
  }
}

