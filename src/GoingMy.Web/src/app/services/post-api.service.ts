import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Post } from '../models/post.model';

/** Request DTO for creating a post. */
export interface CreatePostRequest {
  title: string;
  content: string;
}

/** Request DTO for updating a post. */
export interface UpdatePostRequest {
  title: string;
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
}
