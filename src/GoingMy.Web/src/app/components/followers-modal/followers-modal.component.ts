import { Component, effect, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { UserProfile } from '../../models/user.models';

@Component({
  selector: 'app-user-list-modal',
  standalone: true,
  imports: [CommonModule, RouterModule, DialogModule, ButtonModule, SkeletonModule],
  templateUrl: './followers-modal.component.html',
  styleUrl: './followers-modal.component.css'
})
export class UserListModalComponent {
  readonly users = input<UserProfile[]>([]);
  readonly isVisible = input<boolean>(false);
  readonly loading = input<boolean>(false);
  readonly header = input<string>('Users');
  readonly emptyMessage = input<string>('No users found');

  readonly close = output<void>();
  readonly userSelected = output<string>();

  readonly dialogVisible = signal(false);

  constructor() {
    effect(() => {
      this.dialogVisible.set(this.isVisible());
    });
  }

  onClose(): void {
    this.dialogVisible.set(false);
    this.close.emit();
  }

  onUserClick(userId: string): void {
    this.userSelected.emit(userId);
    this.close.emit();
  }

  getInitials(p: UserProfile): string {
    return `${p.firstName.charAt(0)}${p.lastName.charAt(0)}`.toUpperCase();
  }
}

// Backward compatibility exports
export { UserListModalComponent as FollowersModalComponent };
export { UserListModalComponent as FollowingModalComponent };
