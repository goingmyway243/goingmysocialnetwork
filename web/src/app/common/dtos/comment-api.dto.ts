import { IPagedRequest } from "./common-api.dto";

export interface ICreateCommentRequest {
    comment: string;
    userId: string;
    postId: string;
}

export interface ISearchCommentRequest {
    postId: string;
    pagedRequest: IPagedRequest;
}