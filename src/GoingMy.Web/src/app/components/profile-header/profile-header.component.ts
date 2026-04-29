import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { UserProfile } from '../../models/user.models';
import { GLASS_BTN_PT, FOLLOW_BTN_PT } from '../../configs/app.theme';

@Component({
  selector: 'app-profile-header',
  standalone: true,
  imports: [CommonModule, RouterModule, ButtonModule, SkeletonModule],
  templateUrl: './profile-header.component.html',
  styleUrl: './profile-header.component.css'
})
export class ProfileHeaderComponent {

  // ── 1. Inputs ────────────────────────────────────────────────
  readonly profile = input.required<UserProfile | null>();
  readonly isOwnProfile = input<boolean>(false);
  readonly isFollowing = input<boolean>(false);
  readonly loading = input<boolean>(false);

  // ── 2. Outputs ───────────────────────────────────────────────
  readonly followToggle = output<void>();
  readonly editClick = output<void>();
  readonly followersClick = output<void>();
  readonly followingClick = output<void>();
  readonly messageClick = output<void>();

  // ── 3. Actions ───────────────────────────────────────────────
  onFollowToggle(): void {
    this.followToggle.emit();
  }

  onEditClick(): void {
    this.editClick.emit();
  }

  onFollowersClick(): void {
    this.followersClick.emit();
  }

  onFollowingClick(): void {
    this.followingClick.emit();
  }

  onMessageClick(): void {
    this.messageClick.emit();
  }

  // ── 4. Actions ───────────────────────────────────────────────
  getInitials(profile: UserProfile): string {
    return `${profile.firstName.charAt(0)}${profile.lastName.charAt(0)}`.toUpperCase();
  }

  // ── 5. PassThrough configs (PrimeNG v20) ─────────────────────
  readonly glassBtnPt = GLASS_BTN_PT;
  readonly followBtnPt = FOLLOW_BTN_PT;
}
