import { IPagedRequest } from "./common-api.dto";

export interface ISearchChatMessageRequest {
    searchText: string;
    chatroomId: string;
    pagedRequest: IPagedRequest;
}

export interface ICreateChatMessageRequest {
    message: string;
    userId: string;
    chatroomId: string;
}