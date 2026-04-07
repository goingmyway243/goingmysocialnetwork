import { Author } from './author.model';

export interface Post {
  id: string;
  title: string;
  content: string;
  userId: string;
  username: string;
  createdAt: string;
  updatedAt?: string;
  likes: number;
  comments: number;
  author?: Author;
}
