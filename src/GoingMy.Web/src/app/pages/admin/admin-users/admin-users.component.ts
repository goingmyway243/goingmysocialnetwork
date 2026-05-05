import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageService, ConfirmationService } from 'primeng/api';
import { AdminApiService, AdminUser, PagedResult } from '../../../services/admin-api.service';
import { debounceTime, distinctUntilChanged, Subject } from 'rxjs';

@Component({
  selector: 'app-admin-users',
  imports: [
    CommonModule, FormsModule,
    TableModule, InputTextModule, ButtonModule, TagModule,
    ToggleSwitchModule, ToastModule, ConfirmDialogModule
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './admin-users.component.html',
  styleUrl: './admin-users.component.css'
})
export class AdminUsersComponent implements OnInit {
  private readonly _adminApi = inject(AdminApiService);
  private readonly _toast = inject(MessageService);
  private readonly _confirm = inject(ConfirmationService);

  // ── State ─────────────────────────────────────────────────────
  readonly isLoading = signal(false);
  readonly result = signal<PagedResult<AdminUser> | null>(null);
  readonly searchTerm = signal('');
  readonly currentPage = signal(1);
  readonly pageSize = 20;

  private readonly _searchSubject = new Subject<string>();

  readonly users = computed(() => this.result()?.items ?? []);
  readonly totalCount = computed(() => this.result()?.totalCount ?? 0);

  readonly revoking = signal<Set<string>>(new Set());
  readonly toggling = signal<Set<string>>(new Set());

  // ── Lifecycle ─────────────────────────────────────────────────
  ngOnInit(): void {
    this._loadUsers();

    // Debounce search input
    this._searchSubject.pipe(
      debounceTime(350),
      distinctUntilChanged()
    ).subscribe(() => {
      this.currentPage.set(1);
      this._loadUsers();
    });
  }

  // ── Actions ───────────────────────────────────────────────────
  onSearch(value: string): void {
    this.searchTerm.set(value);
    this._searchSubject.next(value);
  }

  onPageChange(event: { first: number; rows: number }): void {
    this.currentPage.set(Math.floor(event.first / event.rows) + 1);
    this._loadUsers();
  }

  toggleStatus(user: AdminUser): void {
    const newStatus = !user.isActive;
    const action = newStatus ? 'activate' : 'deactivate';

    this._confirm.confirm({
      message: `Are you sure you want to ${action} <strong>${user.username}</strong>?${!newStatus ? ' This will also revoke all their current tokens.' : ''}`,
      header: `${newStatus ? 'Activate' : 'Deactivate'} User`,
      icon: newStatus ? 'pi pi-check-circle' : 'pi pi-ban',
      accept: () => this._doToggleStatus(user, newStatus)
    });
  }

  private _doToggleStatus(user: AdminUser, isActive: boolean): void {
    const toggling = new Set(this.toggling());
    toggling.add(user.id);
    this.toggling.set(toggling);

    this._adminApi.setUserStatus(user.id, isActive).subscribe({
      next: updated => {
        this.result.update(r => r ? {
          ...r,
          items: r.items.map(u => u.id === updated.id ? updated : u)
        } : r);
        const t = new Set(this.toggling());
        t.delete(user.id);
        this.toggling.set(t);
        this._toast.add({ severity: 'success', summary: 'Done', detail: `User ${updated.username} ${isActive ? 'activated' : 'deactivated'}` });
      },
      error: () => {
        const t = new Set(this.toggling());
        t.delete(user.id);
        this.toggling.set(t);
        this._toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to update user status' });
      }
    });
  }

  revokeTokens(user: AdminUser): void {
    this._confirm.confirm({
      message: `Force <strong>${user.username}</strong> to re-login? Their current access tokens will be invalidated immediately.`,
      header: 'Revoke Tokens',
      icon: 'pi pi-key',
      accept: () => this._doRevokeTokens(user)
    });
  }

  private _doRevokeTokens(user: AdminUser): void {
    const revoking = new Set(this.revoking());
    revoking.add(user.id);
    this.revoking.set(revoking);

    this._adminApi.revokeUserTokens(user.id).subscribe({
      next: () => {
        const r = new Set(this.revoking());
        r.delete(user.id);
        this.revoking.set(r);
        this._toast.add({ severity: 'success', summary: 'Tokens Revoked', detail: `${user.username} will be logged out on next request` });
      },
      error: () => {
        const r = new Set(this.revoking());
        r.delete(user.id);
        this.revoking.set(r);
        this._toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to revoke tokens' });
      }
    });
  }

  // ── Helpers ───────────────────────────────────────────────────
  isRoleAdmin(user: AdminUser): boolean {
    // UserRole.Admin = 1 in the backend enum
    return user.roles.includes(1);
  }

  isToggling(userId: string): boolean {
    return this.toggling().has(userId);
  }

  isRevoking(userId: string): boolean {
    return this.revoking().has(userId);
  }

  private _loadUsers(): void {
    this.isLoading.set(true);
    this._adminApi.getUsers({
      page: this.currentPage(),
      pageSize: this.pageSize,
      search: this.searchTerm() || undefined
    }).subscribe({
      next: result => {
        this.result.set(result);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this._toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to load users' });
      }
    });
  }
}
