import { environment } from "../../../environments/environment";
import { UserRole } from "../enums/user-role.enum";
import { Friendship } from "./friendship.model";

export interface IUser {
    id: string;
    email: string;
    role: UserRole;
    dateOfBirth?: Date;
    fullName?: string;
    profilePicture?: string;
    bio?: string;
    location?: string;
    website?: string;
    gender?: string;
    address?: string;
    city?: string;
}

export class User implements IUser {
    id: string;
    email: string;
    role: UserRole;
    dateOfBirth?: Date;
    fullName?: string;
    profilePicture?: string;
    bio?: string;
    location?: string;
    website?: string;

    friendship?: Friendship;

    constructor(user: IUser) {
        this.id = user.id;
        this.email = user.email;
        this.role = user.role;
        this.dateOfBirth = user.dateOfBirth;
        this.fullName = user.fullName;
        this.profilePicture = user.profilePicture;
        this.bio = user.bio;
        this.location = user.location;
        this.website = user.website;
    }

    getProfilePicture(): string {
        return this.profilePicture ?? environment.defaultAvatar;
    }
}