import { Component, inject, input, output, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { UserProfile, UpdateProfileRequest, Gender } from '../../models/user.models';
import { UserProfileService } from '../../services/user-profile.service';
import { CANCEL_BTN_PT, SAVE_BTN_PT, SELECT_PT } from '../../configs/app.theme';

@Component({
  selector: 'app-edit-profile-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogModule, ButtonModule, InputTextModule, TextareaModule, SelectModule],
  templateUrl: './edit-profile-modal.component.html',
  styleUrl: './edit-profile-modal.component.css'
})
export class EditProfileModalComponent {
  private readonly _profileService = inject(UserProfileService);

  // ── Inputs / Outputs ─────────────────────────────────────────
  readonly profile = input.required<UserProfile | null>();
  readonly isVisible = input<boolean>(false);

  readonly close = output<void>();
  readonly profileUpdated = output<void>();

  // ── Form State ────────────────────────────────────────────────
  firstName = '';
  lastName = '';
  bio = '';
  location = '';
  websiteUrl = '';
  isPrivate = false;
  saving = false;

  readonly genderOptions = [
    { label: 'Male', value: Gender.Male },
    { label: 'Female', value: Gender.Female },
    { label: 'Other', value: Gender.Other }
  ];

  selectedGender: { label: string; value: Gender } | null = null;

  constructor() {
    // Populate form when profile is available and modal opens
    effect(() => {
      const p = this.profile();
      if (p && this.isVisible()) {
        this.firstName = p.firstName;
        this.lastName = p.lastName;
        this.bio = p.bio ?? '';
        this.location = p.location ?? '';
        this.websiteUrl = p.websiteUrl ?? '';
        this.isPrivate = p.isPrivate;
        this.selectedGender = this.genderOptions.find(o => o.value === p.gender) ?? null;
      }
    });
  }

  // ── Actions ───────────────────────────────────────────────────
  onSave(): void {
    const p = this.profile();
    if (!p || this.saving) return;

    this.saving = true;
    const request: UpdateProfileRequest = {
      firstName: this.firstName.trim(),
      lastName: this.lastName.trim(),
      bio: this.bio.trim() || undefined,
      location: this.location.trim() || undefined,
      websiteUrl: this.websiteUrl.trim() || undefined,
      isPrivate: this.isPrivate,
      gender: this.selectedGender?.value
    };

    this._profileService.updateProfile(p.id, request).subscribe({
      next: () => {
        this.saving = false;
        this.profileUpdated.emit();
        this.close.emit();
      },
      error: () => {
        this.saving = false;
      }
    });
  }

  onClose(): void {
    this.close.emit();
  }

  // ── Expose PassThrough configs for template ──────────────────
  readonly cancelBtnPt = CANCEL_BTN_PT;
  readonly saveBtnPt = SAVE_BTN_PT;
  readonly selectPt = SELECT_PT;
}
