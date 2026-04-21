import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserProfile, Gender } from '../../models/user.models';

@Component({
  selector: 'app-profile-about',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './profile-about.component.html',
  styleUrl: './profile-about.component.css'
})
export class ProfileAboutComponent {
  readonly profile = input.required<UserProfile | null>();

  formatJoinDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-US', { year: 'numeric', month: 'long' });
  }

  formatGender(gender: Gender): string {
    switch (gender) {
      case Gender.Male: return 'Male';
      case Gender.Female: return 'Female';
      default: return 'Other';
    }
  }

  hasAboutInfo(p: UserProfile): boolean {
    return !!(p.location || p.websiteUrl || p.dateOfBirth || p.createdAt);
  }
}
