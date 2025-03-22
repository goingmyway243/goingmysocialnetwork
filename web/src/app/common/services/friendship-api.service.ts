import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { environment } from '../../../environments/environment';
import { ICreateFriendshipRequest, ISearchFriendshipRequest, IUpdateFriendshipRequest } from '../dtos/friendship-api.dto';
import { Friendship } from '../models/friendship.model';
import { IPagedRequest, IPagedResponse } from '../dtos/common-api.dto';
import { FriendshipStatus } from '../enums/friendship-status.enum';

@Injectable({
    providedIn: 'root',
})
export class FriendshipApiService extends BaseApiService {
    protected override apiUrl = `${environment.baseUrl}/api/friendships`;

    // Send a friend request
    sendFriendRequest(request: ICreateFriendshipRequest): Observable<Friendship> {
        return this.post('', request);
    }

    // Accept a friend request
    acceptFriendRequest(requestId: string): Observable<any> {
        const request: IUpdateFriendshipRequest = {
            id: requestId,
            status: FriendshipStatus.Accepted
        };

        return this.put(requestId, request);
    }

    // Decline a friend request
    declineFriendRequest(requestId: string): Observable<any> {
        const request: IUpdateFriendshipRequest = {
            id: requestId,
            status: FriendshipStatus.Declined
        };

        return this.put(requestId, request);
    }

    // Remove a friend
    removeFriend(requestId: string): Observable<any> {
        return this.delete(requestId);
    }

    // Get the list of friends
    getFriends(userId: string, pagedRequest: IPagedRequest): Observable<any> {
        const request: ISearchFriendshipRequest = {
            filterStatus: [FriendshipStatus.Accepted],
            userId: userId,
            pagedRequest: pagedRequest,
            excludeFriendshipMakeByUser: false
        }

        return this.post(`search`, request);
    }

    // Get pending friend requests
    getPendingRequests(userId: string, pagedRequest: IPagedRequest): Observable<IPagedResponse<Friendship>> {
        const request: ISearchFriendshipRequest = {
            filterStatus: [FriendshipStatus.Pending],
            userId: userId,
            pagedRequest: pagedRequest,
            excludeFriendshipMakeByUser: true
        }

        return this.post(`search`, request);
    }
}