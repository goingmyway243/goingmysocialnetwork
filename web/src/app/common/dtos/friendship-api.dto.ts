import { FriendshipStatus } from "../enums/friendship-status.enum";
import { IPagedRequest } from "./common-api.dto";

export interface ICreateFriendshipRequest {
    userId: string;
    friendId: string;
}

export interface IUpdateFriendshipRequest {
    id: string;
    status: FriendshipStatus;
}

export interface ISearchFriendshipRequest {
    userId: string;
    filterStatus: FriendshipStatus[];
    excludeFriendshipMakeByUser: boolean;
    pagedRequest: IPagedRequest;
}