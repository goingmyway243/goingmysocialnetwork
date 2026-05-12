import { ChangeDetectionStrategy, Component, inject, ViewChild } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BadgeModule } from 'primeng/badge';
import { ButtonModule } from 'primeng/button';
import { Popover, PopoverModule } from 'primeng/popover';
import { ScrollPanelModule } from 'primeng/scrollpanel';
import { AvatarModule } from 'primeng/avatar';
import { TooltipModule } from 'primeng/tooltip';
import { NotificationStateService } from '../../services/notification-state.service';
import { getNotificationIcon, getNotificationRouterLink, getNotificationText } from '../../utils/notification.utils';
import { NotificationDto } from '../../models/notification.models';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ButtonModule, BadgeModule, PopoverModule, ScrollPanelModule, AvatarModule, TooltipModule, RouterLink],
  templateUrl: './notification-bell.component.html',
  styleUrl: './notification-bell.component.css'
})
export class NotificationBellComponent {

  // ── 1. Dependencies ─────────────────────────────────────────
  @ViewChild('op') popover!: Popover;
  readonly state = inject(NotificationStateService);

  // ── 2. Actions ───────────────────────────────────────────────
  toggle(event: Event): void {
    this.popover.toggle(event);
  }

  onOpen(): void {
    this.state.loadNotifications(true);
  }

  onNotificationClick(notification: NotificationDto): void {
    if (!notification.isRead) {
      this.state.markAsRead(notification.id);
    }
    this.popover.hide();
  }

  // ── 3. Helpers ────────────────────────────────────────────────
  getIcon(n: NotificationDto): string {
    return getNotificationIcon(n.type);
  }

  getText(n: NotificationDto): string {
    return getNotificationText(n);
  }

  getRouterLink(n: NotificationDto): string[] {
    return getNotificationRouterLink(n);
  }

  getInitial(username: string): string {
    return username.charAt(0).toUpperCase();
  }

  timeAgo(dateStr: string): string {
    const diff = Date.now() - new Date(dateStr).getTime();
    const mins = Math.floor(diff / 60000);
    if (mins < 1) return 'just now';
    if (mins < 60) return `${mins}m`;
    const hrs = Math.floor(mins / 60);
    if (hrs < 24) return `${hrs}h`;
    const days = Math.floor(hrs / 24);
    return `${days}d`;
  }
}
