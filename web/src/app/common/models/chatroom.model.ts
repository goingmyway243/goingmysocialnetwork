import { IUser } from "./user.model";

export interface IChatroom {
    id: string;
    chatroomName: string;
    participants: IUser[];
}

export class Chatroom implements IChatroom {
    id: string;
    chatroomName: string;
    participants: IUser[];

    constructor(chatroom: IChatroom) {
        this.id = chatroom.id;
        this.chatroomName = chatroom.chatroomName;
        this.participants = chatroom.participants;
    }
}
