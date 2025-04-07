import { User } from "./user.model";

export interface IComment {
    id: string;
    comment: string;
    postId: string;
    userId: string;
    createdAt: Date;
    modifiedAt?: Date;
}

export class Comment implements IComment {
    id: string;
    comment: string;
    postId: string;
    userId: string;
    createdAt: Date;
    modifiedAt?: Date;

    user?: User;

    constructor(comment: IComment) {
        this.id = comment.id;
        this.comment = comment.comment;
        this.postId = comment.postId;
        this.userId = comment.userId;
        this.createdAt = comment.createdAt;
        this.modifiedAt = comment.modifiedAt;
    }
    
}
