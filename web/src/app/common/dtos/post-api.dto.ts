import { IPagedRequest } from "./common-api.dto";

export interface ICreatePostRequest {
    caption: string;
    userId: string;
    sharePostId?: string;
}

export interface ISearchPostRequest {
    ownerId?: string;
    searchText?: string;
    pagedRequest: IPagedRequest;
    currentUserId: string;
}