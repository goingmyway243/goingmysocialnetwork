import { NotificationDto, NotificationType } from '../models/notification.models';

export function getNotificationText(n: NotificationDto): string {
  switch (n.type) {
    case NotificationType.PostLiked:
      return `<strong>${n.actorUsername}</strong> liked your post`;
    case NotificationType.PostCommented:
      return `<strong>${n.actorUsername}</strong> commented on your post`;
    case NotificationType.NewFollower:
      return `<strong>${n.actorUsername}</strong> started following you`;
    case NotificationType.Mentioned:
      return `<strong>${n.actorUsername}</strong> mentioned you`;
    case NotificationType.PostShared:
      return `<strong>${n.actorUsername}</strong> shared your post`;
    case NotificationType.FollowRequestAccepted:
      return `<strong>${n.actorUsername}</strong> accepted your follow request`;
    case NotificationType.PostWithMediaCreated:
      return 'Your post with attachments has been published successfully';
    case NotificationType.PostWithMediaFailed:
      return n.referencePreview ?? 'Your post with attachments could not be published';
    default:
      return `<strong>${n.actorUsername}</strong>`;
  }
}

export function getNotificationIcon(type: NotificationType): string {
  switch (type) {
    case NotificationType.PostLiked: return 'pi pi-heart-fill';
    case NotificationType.PostCommented: return 'pi pi-comment';
    case NotificationType.NewFollower: return 'pi pi-user-plus';
    case NotificationType.Mentioned: return 'pi pi-at';
    case NotificationType.PostShared: return 'pi pi-share-alt';
    case NotificationType.FollowRequestAccepted: return 'pi pi-check-circle';
    case NotificationType.PostWithMediaCreated: return 'pi pi-images';
    case NotificationType.PostWithMediaFailed: return 'pi pi-exclamation-triangle';
    default: return 'pi pi-bell';
  }
}

export function getNotificationRouterLink(n: NotificationDto): string[] {
  switch (n.type) {
    case NotificationType.PostLiked:
    case NotificationType.PostCommented:
    case NotificationType.Mentioned:
    case NotificationType.PostShared:
      return n.referenceId ? ['/posts', n.referenceId] : ['/dashboard/home'];
    case NotificationType.NewFollower:
    case NotificationType.FollowRequestAccepted:
      return n.referenceId ? ['/dashboard/profile', n.referenceId] : ['/dashboard/home'];
    case NotificationType.PostWithMediaCreated:
      return n.referenceId ? ['/posts', n.referenceId] : ['/dashboard/home'];
    case NotificationType.PostWithMediaFailed:
      return ['/dashboard/create-post'];
    default:
      return ['/dashboard/notifications'];
  }
}
