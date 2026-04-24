export interface ConversationDto {
  id: string;
  participantIds: string[];
  participantUsernames: string[];
  lastMessagePreview: string | null;
  lastMessageAt: string | null;
  createdAt: string;
  unreadCount?: number;
}

export interface MessageDto {
  id: string;
  conversationId: string;
  senderId: string;
  senderUsername: string;
  content: string;
  sentAt: string;
  isDeleted: boolean;
  editedContent: string | null;
  editedAt: string | null;
}

export interface ReadReceiptDto {
  messageId: string;
  readByUserId: string;
  readByUsername: string;
  readAt: string;
}

export interface PaginatedResult<T> {
  items: T[];
  hasMore: boolean;
  pageNumber: number;
  pageSize: number;
}

export interface TypingUser {
  userId: string;
  username: string;
}
