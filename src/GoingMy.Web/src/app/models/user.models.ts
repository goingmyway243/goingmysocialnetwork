// ── Enums ─────────────────────────────────────────────────────
export enum Gender {
  Male = 0,
  Female = 1,
  Other = 2
}

// ── Core model ────────────────────────────────────────────────
/** Full social profile served by UserService. */
export interface UserProfile {
  id: string;
  username: string;
  firstName: string;
  lastName: string;
  bio?: string;
  avatarUrl?: string;
  coverUrl?: string;
  dateOfBirth?: string;
  gender: Gender;
  location?: string;
  websiteUrl?: string;
  followersCount: number;
  followingCount: number;
  postsCount: number;
  isVerified: boolean;
  isPrivate: boolean;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
  interests: string[];
}

// ── Request DTOs ─────────────────────────────────────────────
export interface UpdateProfileRequest {
  firstName?: string;
  lastName?: string;
  bio?: string;
  dateOfBirth?: string; // ISO date string
  gender?: Gender;
  location?: string;
  websiteUrl?: string;
  isPrivate?: boolean;
  interests?: string[];
}

export interface UpdateAvatarRequest {
  avatarUrl: string;
}

export interface UpdateCoverRequest {
  coverUrl: string;
}
