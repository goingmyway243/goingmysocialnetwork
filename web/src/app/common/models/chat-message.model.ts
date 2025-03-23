import { User } from "./user.model";

export interface IChatMessage {
    id: string;
    message: string;
    userId: string;
    chatroomId: string;
    createdAt: Date;
    modifiedAt?: Date;
}

export class ChatMessage implements IChatMessage {
    id: string;
    message: string;
    userId: string;
    chatroomId: string;
    createdAt: Date;
    modifiedAt?: Date | undefined;

    user?: User;

    constructor(chatMessage: IChatMessage) {
        this.id = chatMessage.id;
        this.message = chatMessage.message;
        this.userId = chatMessage.userId;
        this.chatroomId = chatMessage.chatroomId;
        this.createdAt = chatMessage.createdAt;
        this.modifiedAt = chatMessage.modifiedAt;
    }
}
