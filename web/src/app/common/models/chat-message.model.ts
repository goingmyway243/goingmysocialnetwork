export interface IChatMessage {
    id: string;
    message: string;
    userId: string;
    chatroomId: string;
    createdAt: Date;
    modifiedAt?: Date;
    createdBy: string;
    modifiedBy?: string;
}

export class ChatMessage implements IChatMessage {
    id: string;
    message: string;
    userId: string;
    chatroomId: string;
    createdAt: Date;
    modifiedAt?: Date | undefined;
    createdBy: string;
    modifiedBy?: string | undefined;

    constructor(chatMessage: IChatMessage) {
        this.id = chatMessage.id;
        this.message = chatMessage.message;
        this.userId = chatMessage.userId;
        this.chatroomId = chatMessage.chatroomId;
        this.createdAt = chatMessage.createdAt;
        this.modifiedAt = chatMessage.modifiedAt;
        this.createdBy = chatMessage.createdBy;
        this.modifiedBy = chatMessage.modifiedBy;
    }
}
