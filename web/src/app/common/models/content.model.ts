import { ContentType } from "../enums/content-type.enum";

export interface IContent {
    id: string;
    textContent: string;
    linkContent: string;
    type: ContentType;
    postId: string;
    createdAt: Date;
    modifiedAt?: Date;
}

export class Content implements IContent {
    id: string;
    textContent: string;
    linkContent: string;
    type: ContentType;
    postId: string;
    createdAt: Date;
    modifiedAt?: Date | undefined;
    
    constructor(content: IContent) {
        this.id = content.id;
        this.textContent = content.textContent;
        this.linkContent = content.linkContent;
        this.type = content.type;
        this.postId = content.postId;
        this.createdAt = content.createdAt;
        this.modifiedAt = content.modifiedAt;
    }
}
