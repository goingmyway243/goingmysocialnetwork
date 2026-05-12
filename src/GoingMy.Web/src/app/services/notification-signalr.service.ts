import { inject, Injectable, OnDestroy } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../environments/environment';
import { NotificationDto } from '../models/notification.models';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class NotificationSignalRService implements OnDestroy {

  // ── 1. Dependencies ─────────────────────────────────────────
  private readonly _auth = inject(AuthService);

  // ── 2. SignalR connection ─────────────────────────────────────
  private _connection: signalR.HubConnection | null = null;

  // ── 3. Event streams ─────────────────────────────────────────
  readonly notificationReceived$ = new Subject<NotificationDto>();
  readonly unreadCountUpdated$ = new Subject<number>();

  // ── 4. Connection lifecycle ──────────────────────────────────
  async connect(): Promise<void> {
    if (this._connection?.state === signalR.HubConnectionState.Connected) return;

    this._connection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiGatewayUrl}/hubs/notification`, {
        accessTokenFactory: () => this._auth.getAccessToken()
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this._registerHandlers();
    await this._connection.start();
  }

  async disconnect(): Promise<void> {
    if (this._connection) {
      await this._connection.stop();
      this._connection = null;
    }
  }

  ngOnDestroy(): void {
    this.disconnect();
    this.notificationReceived$.complete();
    this.unreadCountUpdated$.complete();
  }

  // ── 5. Private helpers ────────────────────────────────────────
  private _registerHandlers(): void {
    if (!this._connection) return;

    this._connection.on('ReceiveNotification', (notification: NotificationDto) => {
      this.notificationReceived$.next(notification);
    });

    this._connection.on('UnreadCountUpdated', (count: number) => {
      this.unreadCountUpdated$.next(count);
    });
  }
}
