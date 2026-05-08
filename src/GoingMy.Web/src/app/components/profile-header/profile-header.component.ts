import { Component, inject, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { switchMap } from 'rxjs/operators';
import { of } from 'rxjs';
import { UserProfile } from '../../models/user.models';
import { GLASS_BTN_PT, FOLLOW_BTN_PT } from '../../configs/app.theme';
import { UploadApiService } from '../../services/upload-api.service';
import { UserProfileService } from '../../services/user-profile.service';

@Component({
  selector: 'app-profile-header',
  standalone: true,
  imports: [CommonModule, RouterModule, ButtonModule, SkeletonModule, ProgressSpinnerModule],
  templateUrl: './profile-header.component.html',
  styleUrl: './profile-header.component.css'
})
export class ProfileHeaderComponent {
  // ── Dependencies ─────────────────────────────────────────
  private readonly _uploadApi = inject(UploadApiService);
  private readonly _profileService = inject(UserProfileService);

  // ── PassThrough configs (PrimeNG v20) ─────────────────────
  readonly glassBtnPt = GLASS_BTN_PT;
  readonly followBtnPt = FOLLOW_BTN_PT;

  // ── Inputs ────────────────────────────────────────────────
  readonly profile = input.required<UserProfile | null>();
  readonly isOwnProfile = input<boolean>(false);
  readonly isFollowing = input<boolean>(false);
  readonly loading = input<boolean>(false);

  // ── Outputs ───────────────────────────────────────────────
  readonly followToggle = output<void>();
  readonly editClick = output<void>();
  readonly followersClick = output<void>();
  readonly followingClick = output<void>();
  readonly messageClick = output<void>();
  readonly profileUpdated = output<void>();

  // ── State ─────────────────────────────────────────────────
  readonly avatarUploading = signal(false);

  // ── Actions ───────────────────────────────────────────────
  onAvatarSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      this.uploadAvatar(file);
    }
    input.value = ''; // Reset input
  }

  private uploadAvatar(file: File): void {
    const p = this.profile();
    if (!p || this.avatarUploading()) {
      return;
    }

    this.avatarUploading.set(true);

    // Resolve the old avatar's file ID (if it was uploaded via UploadService)
    const oldFileKey = this._extractFileKey(p.avatarUrl);
    const oldFileId$ = oldFileKey
      ? this._uploadApi.getFileByKey(oldFileKey)
      : of(null);

    oldFileId$.pipe(
      switchMap(oldFile =>
        this._uploadApi.uploadFile(file, 'Avatar').pipe(
          switchMap(newMedia =>
            this._profileService.updateAvatar(p.id, newMedia.url, oldFile?.id)
          )
        )
      )
    ).subscribe({
      next: () => {
        this.avatarUploading.set(false);
        this.profileUpdated.emit();
      },
      error: () => {
        this.avatarUploading.set(false);
      }
    });
  }

  /** Extracts the file key from an UploadService URL (last path segment). Returns null for external URLs. */
  private _extractFileKey(url: string | undefined): string | null {
    if (!url || !url.includes('/uploads/')) return null;
    const parts = url.split('/');
    return parts[parts.length - 1] || null;
  }

  onFollowToggle(): void {
    this.followToggle.emit();
  }

  onEditClick(): void {
    // this.editClick.emit();
    this.profileUpdated.emit();
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

  getInitials(profile: UserProfile): string {
    return `${profile.firstName.charAt(0)}${profile.lastName.charAt(0)}`.toUpperCase();
  }
}
