export enum NotificationType {
  PostLiked = 0,
  PostCommented = 1,
  NewFollower = 2,
  Mentioned = 3,
  PostShared = 4,
  FollowRequestAccepted = 5,
  PostWithMediaCreated = 6,
  PostWithMediaFailed = 7
}

export interface NotificationDto {
  id: string;
  recipientUserId: string;
  actorUserId: string;
  actorUsername: string;
  actorAvatarUrl: string | null;
  type: NotificationType;
  referenceId: string | null;
  referencePreview: string | null;
  isRead: boolean;
  createdAt: string;
}

export interface NotificationPagedResult {
  items: NotificationDto[];
  hasMore: boolean;
  pageNumber: number;
  pageSize: number;
}
