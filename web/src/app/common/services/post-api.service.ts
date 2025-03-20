import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { ICreatePostRequest } from '../models/post-api.model';
import { environment } from '../../../environments/environment';

@Injectable({
    providedIn: 'root',
})
export class PostApiService extends BaseApiService {
    protected override apiUrl: string = `${environment.baseUrl}/api/post`;

    // Fetch a single post by ID
    getPostById(postId: string): Observable<any> {
        return this.get(`/${postId}`);
    }

    // Create a new post
    createPost(request: ICreatePostRequest): Observable<any> {
        return this.post('', request);
    }

    // Update an existing post
    updatePost(postId: string, postData: any): Observable<any> {
        return this.put(`/${postId}`, postData);
    }

    // Delete a post
    deletePost(postId: string): Observable<any> {
        return this.delete(`/${postId}`);
    }
}