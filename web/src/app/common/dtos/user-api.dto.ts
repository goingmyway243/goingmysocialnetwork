import { IPagedRequest } from "./common-api.dto";

export interface ISearchUserRequest extends IPagedRequest {
    searchText: string;
}