import { IPagedRequest } from "./common-api.dto";

export interface ISearchUserRequest {
    searchText: string;
    includeFriendship: boolean;
    requestUserId: string;
    pagedRequest: IPagedRequest;
}