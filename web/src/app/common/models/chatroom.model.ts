import { IUser, User } from "./user.model";

export interface IChatroom {
    id: string;
    chatroomName: string;
    participants: IUser[];
}

export class Chatroom implements IChatroom {
    id: string;
    chatroomName: string;
    participants: User[];

    constructor(chatroom: IChatroom) {
        this.id = chatroom.id;
        this.chatroomName = chatroom.chatroomName;
        this.participants = chatroom.participants;
    }
}
