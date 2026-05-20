export enum NotificationType {
  PostLiked = 'PostLiked',
  PostCommented = 'PostCommented',
  NewFollower = 'NewFollower',
  Mentioned = 'Mentioned',
  PostShared = 'PostShared',
  FollowRequestAccepted = 'FollowRequestAccepted',
  PostWithMediaCreated = 'PostWithMediaCreated',
  PostWithMediaFailed = 'PostWithMediaFailed'
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
