export interface ILike {
    id: string;
    userId: string;
    postId: string;
}

export class Like implements ILike {
    id: string;
    userId: string;
    postId: string;

    constructor(like: ILike) {
        this.id = like.id;
        this.userId = like.userId;
        this.postId = like.postId;
    }
}
