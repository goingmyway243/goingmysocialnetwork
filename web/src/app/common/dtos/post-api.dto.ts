import { IPagedRequest } from "./common-api.dto";

export interface ICreatePostRequest {
    caption: string;
    userId: string;
    sharePostId?: string;
}

export interface ISearchPostRequest extends IPagedRequest {
    ownerId?: string;
    searchText?: string;
}