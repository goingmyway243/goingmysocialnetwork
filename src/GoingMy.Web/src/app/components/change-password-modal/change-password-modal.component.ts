import { Component, inject, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { PasswordModule } from 'primeng/password';
import { MessageService } from 'primeng/api';
import { UserApiService } from '../../services/user-api.service';
import { CANCEL_BTN_PT, SAVE_BTN_PT } from '../../configs/app.theme';

@Component({
  selector: 'app-change-password-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogModule, ButtonModule, PasswordModule],
  templateUrl: './change-password-modal.component.html',
  styleUrl: './change-password-modal.component.css'
})
export class ChangePasswordModalComponent {
  private readonly _userApi = inject(UserApiService);
  private readonly _messageService = inject(MessageService);

  // ── Inputs / Outputs ─────────────────────────────────────────
  readonly userId = input.required<string>();
  readonly isVisible = input<boolean>(false);

  readonly close = output<void>();

  // ── Form State ────────────────────────────────────────────────
  currentPassword = '';
  newPassword = '';
  confirmPassword = '';
  saving = false;
  validationError = '';

  // ── PassThrough configs ───────────────────────────────────────
  readonly cancelBtnPt = CANCEL_BTN_PT;
  readonly saveBtnPt = SAVE_BTN_PT;

  // ── Actions ───────────────────────────────────────────────────
  onSave(): void {
    this.validationError = '';

    if (!this.currentPassword || !this.newPassword || !this.confirmPassword) {
      this.validationError = 'All fields are required.';
      return;
    }

    if (this.newPassword.length < 6) {
      this.validationError = 'New password must be at least 6 characters.';
      return;
    }

    if (this.newPassword !== this.confirmPassword) {
      this.validationError = 'New passwords do not match.';
      return;
    }

    if (this.saving) return;
    this.saving = true;

    this._userApi.changePassword(this.userId(), {
      currentPassword: this.currentPassword,
      newPassword: this.newPassword
    }).subscribe({
      next: () => {
        this.saving = false;
        this._messageService.add({
          severity: 'success',
          summary: 'Password changed',
          detail: 'Your password has been updated successfully.'
        });
        this._resetForm();
        this.close.emit();
      },
      error: (err) => {
        this.saving = false;
        const message = err?.error?.message ?? 'Failed to change password. Please try again.';
        this.validationError = message;
      }
    });
  }

  onClose(): void {
    this._resetForm();
    this.close.emit();
  }

  private _resetForm(): void {
    this.currentPassword = '';
    this.newPassword = '';
    this.confirmPassword = '';
    this.validationError = '';
  }
}
