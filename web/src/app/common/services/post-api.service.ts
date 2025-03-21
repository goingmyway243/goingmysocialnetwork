import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { ICreatePostRequest, ISearchPostRequest } from '../dtos/post-api.dto';
import { environment } from '../../../environments/environment';
import { IPagedResponse } from '../dtos/common-api.dto';
import { Post } from '../models/post.model';

@Injectable({
    providedIn: 'root',
})
export class PostApiService extends BaseApiService {
    protected override apiUrl: string = `${environment.baseUrl}/api/post`;

    getPostById(postId: string): Observable<any> {
        return this.get(`${postId}`);
    }

    searchPost(request: ISearchPostRequest): Observable<IPagedResponse<Post>> {
        return this.post('search', request);
    }

    createPost(request: ICreatePostRequest): Observable<any> {
        return this.post('', request);
    }

    updatePost(postId: string, postData: any): Observable<any> {
        return this.put(`${postId}`, postData);
    }

    deletePost(postId: string): Observable<any> {
        return this.delete(`${postId}`);
    }
}