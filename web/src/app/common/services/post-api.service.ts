import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { ICreatePostRequest, ISearchPostRequest } from '../dtos/post-api.dto';
import { environment } from '../../../environments/environment';
import { IPagedResponse } from '../dtos/common-api.dto';
import { Post } from '../models/post.model';
import { ContentType } from '../enums/content-type.enum';

@Injectable({
    providedIn: 'root',
})
export class PostApiService extends BaseApiService {
    protected override apiUrl: string = `${environment.baseUrl}/api/posts`;

    getPostById(postId: string): Observable<any> {
        return this.get(`${postId}`);
    }

    searchPosts(request: ISearchPostRequest): Observable<IPagedResponse<Post>> {
        return this.post('search', request);
    }

    createPost(request: ICreatePostRequest, contentFiles: File[]): Observable<Post> {
        const formData = new FormData();
        formData.append('Caption', request.caption);
        formData.append('UserId', request.userId);
        if (request.sharePostId) {
            formData.append('SharePostId', request.sharePostId);
        }
        
        contentFiles.forEach((value, index) => {
            formData.append(`Contents[${index}].Type`, ContentType.Image.toString());
            formData.append(`Contents[${index}].FormFile`, value);
        });
        
        return this.post('', formData);
    }

    updatePost(postId: string, postData: any): Observable<any> {
        return this.put(`${postId}`, postData);
    }

    deletePost(postId: string): Observable<any> {
        return this.delete(`${postId}`);
    }
}