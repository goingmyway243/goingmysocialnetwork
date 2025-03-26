import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { environment } from '../../../environments/environment';
import { ICreateCommentRequest, ISearchCommentRequest } from '../dtos/comment-api.dto';
import { IPagedResponse } from '../dtos/common-api.dto';
import { Comment } from '../models/comment.model';

@Injectable({
    providedIn: 'root',
})
export class CommentApiService extends BaseApiService {
    protected override apiUrl: string = `${environment.baseUrl}/api/comments`;

    getPostById(commentId: string): Observable<any> {
        return this.get(commentId);
    }

    searchComments(request: ISearchCommentRequest): Observable<IPagedResponse<Comment>> {
        return this.post('search', request);
    }

    createComment(request: ICreateCommentRequest): Observable<Comment> {
        return this.post('', request);
    }

    updateComment(commentId: string, postData: any): Observable<any> {
        return this.put(`${commentId}`, postData);
    }

    deleteComment(commentId: string): Observable<any> {
        return this.delete(`${commentId}`);
    }
}