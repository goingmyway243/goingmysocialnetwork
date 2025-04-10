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
    createdAt: Date;
    modifiedAt?: Date;
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
    createdAt: Date;
    modifiedAt?: Date;

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
        this.createdAt = user.createdAt;
        this.modifiedAt = user.modifiedAt
    }
}