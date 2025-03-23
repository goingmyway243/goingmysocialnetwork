import { ChatMessage } from "./chat-message.model";
import { User } from "./user.model";

export interface IChatroom {
    id: string;
    chatroomName: string;
    participants: User[];
}

export class Chatroom implements IChatroom {
    id: string;
    chatroomName: string;
    participants: User[];
    latestMessage?: ChatMessage;

    constructor(chatroom: IChatroom) {
        this.id = chatroom.id;
        this.chatroomName = chatroom.chatroomName;
        this.participants = chatroom.participants;
    }
}
