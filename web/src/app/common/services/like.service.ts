import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { environment } from '../../../environments/environment';
import { IToggleLike } from '../dtos/like-api.dto';

@Injectable({
    providedIn: 'root',
})
export class LikeApiService extends BaseApiService {
    protected override apiUrl: string = `${environment.baseUrl}/api/likes`;

    getPostById(likeId: string): Observable<any> {
        return this.get(`${likeId}`);
    }

    // searchLikes(request: ISearchPostRequest): Observable<IPagedResponse<Like>> {
    //     return this.post('search', {});
    // }

    toggleLike(request: IToggleLike): Observable<number> {
        return this.post('', request);
    }

    updateLike(likeId: string, postData: any): Observable<any> {
        return this.put(`${likeId}`, postData);
    }
}