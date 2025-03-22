import { Content } from "./content.model";
import { User } from "./user.model";

export interface IPost {
    id: string;
    caption: string;
    userId: string;
    sharePostId?: string;
    likeCount: number;
    commentCount: number;
    createdAt: Date;
    modifiedAt?: Date;
}

export class Post implements IPost {
    id: string;
    caption: string;
    userId: string;
    sharePostId?: string | undefined;
    likeCount: number;
    commentCount: number;
    createdAt: Date;
    modifiedAt?: Date | undefined;

    user?: User;
    contents?: Content[];

    constructor(post: IPost) {
        this.id = post.id;
        this.caption = post.caption;
        this.userId = post.userId;
        this.sharePostId = post.sharePostId;
        this.likeCount = post.likeCount;
        this.commentCount = post.commentCount;
        this.createdAt = post.createdAt;
        this.modifiedAt = post.modifiedAt;
    }
}
