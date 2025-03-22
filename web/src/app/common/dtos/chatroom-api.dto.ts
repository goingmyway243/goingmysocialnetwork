import { IPagedRequest } from "./common-api.dto";

export interface ISearchChatroomRequest {
    searchText: string;
    userId: string;
    pagedRequest: IPagedRequest;
}