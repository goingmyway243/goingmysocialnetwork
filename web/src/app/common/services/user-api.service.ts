import { Injectable } from "@angular/core";
import { BaseApiService } from "./base-api.service";
import { Observable } from "rxjs";
import { User } from "../models/user.model";
import { environment } from "../../../environments/environment";
import { IPagedResponse } from "../dtos/common-api.dto";
import { ISearchUserRequest } from "../dtos/user-api.dto";

@Injectable({
    providedIn: 'root'
})
export class UserApiService extends BaseApiService {
    protected override apiUrl: string = `${environment.baseUrl}/api/users`;

    searchUsers(request: ISearchUserRequest): Observable<IPagedResponse<User>> {
        return this.post('search', request);
    }

    getUserById(id: string): Observable<User> {
        return this.get(id);
    }
}