import { Author } from './author.model';
import { MediaAttachment } from './media.model';

export interface Post {
  id: string;
  content: string;
  userId: string;
  username: string;
  createdAt: string;
  updatedAt?: string;
  likes?: number;
  comments?: number;
  author?: Author;
  userHasLiked?: boolean;
  mediaAttachments?: MediaAttachment[];
}

export interface Comment {
  id: string;
  postId: string;
  userId: string;
  username: string;
  content: string;
  createdAt: string;
  updatedAt?: string;
}

export interface Like {
  id: string;
  postId: string;
  userId: string;
  username: string;
  createdAt: string;
}

export interface PostCommentsState {
  expanded: boolean;
  loading: boolean;
  comments: Comment[];
  newComment: string;
  submitting: boolean;
}
