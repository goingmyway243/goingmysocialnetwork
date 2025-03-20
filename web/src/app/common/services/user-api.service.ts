import { Injectable } from "@angular/core";
import { BaseApiService } from "./base-api.service";
import { Observable } from "rxjs";
import { User } from "../models/user.model";
import { environment } from "../../../environments/environment";

@Injectable({
    providedIn: 'root'
})
export class UserApiService extends BaseApiService {
    protected override apiUrl: string = `${environment.baseUrl}/api/user`;

    getUserById(id: string): Observable<User> {
        return this.get(id);
    }
}