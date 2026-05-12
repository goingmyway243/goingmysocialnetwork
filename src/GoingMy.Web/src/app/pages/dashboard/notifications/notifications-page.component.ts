import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { SelectButtonModule } from 'primeng/selectbutton';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { AvatarModule } from 'primeng/avatar';
import { FormsModule } from '@angular/forms';
import { NotificationStateService } from '../../../services/notification-state.service';
import { NotificationDto, NotificationType } from '../../../models/notification.models';
import { getNotificationIcon, getNotificationRouterLink, getNotificationText } from '../../../utils/notification.utils';

interface FilterOption {
  label: string;
  value: string;
}

@Component({
  selector: 'app-notifications-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, ButtonModule, SelectButtonModule, ProgressSpinnerModule, AvatarModule, FormsModule],
  templateUrl: './notifications-page.component.html',
  styleUrl: './notifications-page.component.css'
})
export class NotificationsPageComponent implements OnInit {

  // ── 1. Dependencies ─────────────────────────────────────────
  readonly state = inject(NotificationStateService);

  // ── 2. State ────────────────────────────────────────────────
  readonly activeFilter = signal<string>('all');

  readonly filterOptions: FilterOption[] = [
    { label: 'All', value: 'all' },
    { label: 'Likes', value: NotificationType.PostLiked },
    { label: 'Comments', value: NotificationType.PostCommented },
    { label: 'Follows', value: NotificationType.NewFollower },
    { label: 'Mentions', value: NotificationType.Mentioned },
  ];

  // ── 3. Lifecycle ─────────────────────────────────────────────
  ngOnInit(): void {
    this.state.loadNotifications(true);
  }

  // ── 4. Derived helpers ────────────────────────────────────────
  get filteredNotifications(): NotificationDto[] {
    const filter = this.activeFilter();
    if (filter === 'all') return this.state.notifications();
    return this.state.notifications().filter(n => n.type === filter);
  }

  // ── 5. Actions ───────────────────────────────────────────────
  onFilterChange(value: string): void {
    this.activeFilter.set(value);
  }

  onNotificationClick(n: NotificationDto): void {
    if (!n.isRead) this.state.markAsRead(n.id);
  }

  loadMore(): void {
    this.state.loadNotifications(false);
  }

  // ── 6. Helpers ────────────────────────────────────────────────
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
    if (mins < 60) return `${mins}m ago`;
    const hrs = Math.floor(mins / 60);
    if (hrs < 24) return `${hrs}h ago`;
    const days = Math.floor(hrs / 24);
    return `${days}d ago`;
  }
}
