import { FriendshipStatus } from "../enums/friendship-status.enum";

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

    constructor(data: IFriendship){
        this.id = data.id;
        this.userId = data.userId;
        this.friendId = data.friendId;
        this.status = data.status;
        this.createAt = data.createAt;
        this.modifiedAt = data.modifiedAt;
    }
}