import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { environment } from '../../../environments/environment';
import { ICreateFriendshipRequest } from '../dtos/friendship-api.dto';
import { Friendship } from '../models/friendship.model';

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
        return this.post(`${this.apiUrl}/accept`, { requestId });
    }

    // Decline a friend request
    declineFriendRequest(requestId: string): Observable<any> {
        return this.post(`${this.apiUrl}/decline`, { requestId });
    }

    // Remove a friend
    removeFriend(friendId: string): Observable<any> {
        return this.delete(`${this.apiUrl}/remove/${friendId}`);
    }

    // Get the list of friends
    getFriends(): Observable<any> {
        return this.get(`${this.apiUrl}/list`);
    }

    // Get pending friend requests
    getPendingRequests(): Observable<any> {
        return this.get(`${this.apiUrl}/pending`);
    }
}