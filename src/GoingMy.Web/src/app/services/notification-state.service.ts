import { computed, inject, Injectable, OnDestroy, signal } from '@angular/core';
import { Subscription } from 'rxjs';
import { NotificationDto } from '../models/notification.models';
import { NotificationApiService } from './notification-api.service';
import { NotificationSignalRService } from './notification-signalr.service';

@Injectable({ providedIn: 'root' })
export class NotificationStateService implements OnDestroy {

  // ── 1. Dependencies ─────────────────────────────────────────
  private readonly _api = inject(NotificationApiService);
  private readonly _signalR = inject(NotificationSignalRService);
  private readonly _subs: Subscription[] = [];

  // ── 2. State ────────────────────────────────────────────────
  readonly notifications = signal<NotificationDto[]>([]);
  readonly unreadCount = signal(0);
  readonly isLoading = signal(false);
  readonly hasMore = signal(false);
  readonly pageNumber = signal(0);

  // ── 3. Derived State ─────────────────────────────────────────
  readonly hasUnread = computed(() => this.unreadCount() > 0);
  readonly displayCount = computed(() => {
    const count = this.unreadCount();
    return count > 99 ? '99+' : count.toString();
  });

  // ── 4. Lifecycle ─────────────────────────────────────────────
  constructor() {
    this._subscribeToSignalR();
    this._initSignalR();
    this.loadUnreadCount();
  }

  ngOnDestroy(): void {
    this._subs.forEach(s => s.unsubscribe());
    this._signalR.disconnect();
  }

  // ── 5. Data actions ──────────────────────────────────────────
  loadNotifications(reset = false): void {
    if (reset) {
      this.notifications.set([]);
      this.pageNumber.set(0);
      this.hasMore.set(false);
    }

    const page = reset ? 0 : this.pageNumber();
    this.isLoading.set(true);

    this._api.getNotifications(page, 20).subscribe({
      next: result => {
        const current = reset ? [] : this.notifications();
        this.notifications.set([...current, ...result.items]);
        this.hasMore.set(result.hasMore);
        this.pageNumber.set(page + 1);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  loadUnreadCount(): void {
    this._api.getUnreadCount().subscribe({
      next: result => this.unreadCount.set(result.count)
    });
  }

  markAsRead(id: string): void {
    this._api.markAsRead(id).subscribe({
      next: () => {
        this.notifications.update(items =>
          items.map(n => n.id === id ? { ...n, isRead: true } : n)
        );
        this.unreadCount.update(c => Math.max(0, c - 1));
      }
    });
  }

  markAllAsRead(): void {
    this._api.markAllAsRead().subscribe({
      next: () => {
        this.notifications.update(items => items.map(n => ({ ...n, isRead: true })));
        this.unreadCount.set(0);
      }
    });
  }

  deleteNotification(id: string): void {
    const wasUnread = this.notifications().find(n => n.id === id)?.isRead === false;
    this._api.delete(id).subscribe({
      next: () => {
        this.notifications.update(items => items.filter(n => n.id !== id));
        if (wasUnread) this.unreadCount.update(c => Math.max(0, c - 1));
      }
    });
  }

  // ── 6. Private ────────────────────────────────────────────────
  private async _initSignalR(): Promise<void> {
    try {
      await this._signalR.connect();
    } catch {
      // SignalR connection failure is non-fatal — polling fallback not required
    }
  }

  private _subscribeToSignalR(): void {
    this._subs.push(
      this._signalR.notificationReceived$.subscribe(notification => {
        this.notifications.update(items => [notification, ...items]);
        this.unreadCount.update(c => c + 1);
      }),
      this._signalR.unreadCountUpdated$.subscribe(count => {
        this.unreadCount.set(count);
      })
    );
  }
}
