import { FriendshipStatus } from "../enums/friendship-status.enum";
import { User } from "./user.model";

export interface IFriendship {
    id: string;
    userId: string;
    friendId: string;
    status: FriendshipStatus;
    createAt: Date;
    modifiedAt?: Date;
}

export class Friendship implements IFriendship {
    id: string;
    userId: string;
    friendId: string;
    status: FriendshipStatus;
    createAt: Date;
    modifiedAt?: Date | undefined;

    user?: User;

    constructor(data: IFriendship){
        this.id = data.id;
        this.userId = data.userId;
        this.friendId = data.friendId;
        this.status = data.status;
        this.createAt = data.createAt;
        this.modifiedAt = data.modifiedAt;
    }
}