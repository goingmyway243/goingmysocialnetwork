import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { UserProfile } from '../../models/user.models';

@Component({
  selector: 'app-followers-modal',
  standalone: true,
  imports: [CommonModule, RouterModule, DialogModule, ButtonModule, SkeletonModule],
  templateUrl: './followers-modal.component.html',
  styleUrl: './followers-modal.component.css'
})
export class FollowersModalComponent {
  readonly followers = input<UserProfile[]>([]);
  readonly isVisible = input<boolean>(false);
  readonly loading = input<boolean>(false);

  readonly close = output<void>();
  readonly userSelected = output<string>();

  onClose(): void {
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
