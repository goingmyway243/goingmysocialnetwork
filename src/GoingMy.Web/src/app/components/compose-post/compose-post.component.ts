import { Component, OnInit, output, inject, signal, computed, ViewChild, ElementRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { TextareaModule } from 'primeng/textarea';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { PostApiService } from '../../services/post-api.service';
import { UploadApiService } from '../../services/upload-api.service';
import { AiApiService } from '../../services/ai-api.service';
import { UserApiService } from '../../services/user-api.service';
import { AuthService } from '../../services/auth.service';
import { MediaPreviewComponent, MediaUploadProgress } from '../media-preview/media-preview.component';
import { Post } from '../../models/post.model';
import { MediaFile } from '../../models/media.model';
import { AiAction } from '../../models/ai-assist.model';

const TONES = ['Casual', 'Professional', 'Funny', 'Inspirational'] as const;

// ── File Validation Configuration ────────────────────────────
const ALLOWED_IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];
const ALLOWED_VIDEO_TYPES = ['video/mp4', 'video/webm'];
const MAX_IMAGE_SIZE = 10 * 1024 * 1024; // 10 MB
const MAX_VIDEO_SIZE = 100 * 1024 * 1024; // 100 MB
const MAX_FILES = 4;

@Component({
  selector: 'app-compose-post',
  standalone: true,
  imports: [FormsModule, CommonModule, ButtonModule, DialogModule, TextareaModule, InputTextModule, SelectModule, TooltipModule, MediaPreviewComponent],
  templateUrl: './compose-post.component.html',
  styleUrl: './compose-post.component.css'
})
export class ComposePostComponent implements OnInit {

  // ── 1. Dependencies ─────────────────────────────────────────
  private readonly _postApi = inject(PostApiService);
  private readonly _uploadApi = inject(UploadApiService);
  private readonly _aiApi = inject(AiApiService);
  private readonly _userApi = inject(UserApiService);
  private readonly _authService = inject(AuthService);
  private readonly _messageService = inject(MessageService);

  // ── 2. Template References ──────────────────────────────────
  @ViewChild('fileInput') fileInput?: ElementRef<HTMLInputElement>;

  // ── 3. Outputs ───────────────────────────────────────────────
  readonly postCreated = output<Post>();

  // ── 4. State ─────────────────────────────────────────────────
  readonly dialogVisible = signal(false);
  readonly currentUserAvatarUrl = signal<string | null>(null);
  readonly content = signal('');
  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);
  readonly waitingDialogVisible = signal(false);
  readonly waitingCountdown = signal(5);
  private _waitingCountdownInterval: ReturnType<typeof setInterval> | null = null;

  // ── Media State ───────────────────────────────────────────────
  readonly pendingMediaFiles = signal<MediaFile[]>([]);
  readonly selectedMediaFiles = signal<MediaFile[]>([]);
  readonly mediaUploading = signal(false);
  readonly mediaValidating = signal(false);
  readonly mediaError = signal<string | null>(null);
  readonly uploadProgress = signal<MediaUploadProgress>({});
  /** Maps MediaFile.id → local object URL created from the original File */
  private readonly _localPreviewMap = new Map<string, string>();
  /** Blob URLs already injected into emitted posts — kept alive until page unload so images stay visible. */
  private readonly _committedPreviewUrls = new Set<string>();

  // ── AI State ──────────────────────────────────────────────────
  readonly aiPanelVisible = signal(false);
  readonly aiLoading = signal(false);
  readonly aiSuggestion = signal<string | null>(null);
  readonly aiError = signal<string | null>(null);
  selectedTone = TONES[0];
  readonly toneOptions = TONES.map(t => ({ label: t, value: t }));

  // ── 5. Derived State ─────────────────────────────────────────
  readonly isValid = computed(() => this.content().trim().length > 0);
  readonly contentLength = computed(() => this.content().length);
  readonly contentTooLong = computed(() => this.contentLength() > 2000);
  readonly hasMedia = computed(() => this.selectedMediaFiles().length > 0);
  readonly previewMediaFiles = computed(() => [...this.pendingMediaFiles(), ...this.selectedMediaFiles()]);
  readonly mediaCount = computed(() => this.selectedMediaFiles().length);
  readonly canAttachMore = computed(() => this.mediaCount() < MAX_FILES);
  readonly canSubmit = computed(() => {
    const hasPostPayload = this.isValid() || this.hasMedia();
    return hasPostPayload && !this.contentTooLong() && !this.submitting() && !this.mediaUploading() && !this.mediaValidating();
  });
  readonly hasAiSuggestion = computed(() => this.aiSuggestion() !== null);

  // ── 5. Actions ───────────────────────────────────────────────
  ngOnInit(): void {
    this._loadCurrentUserAvatar();
  }

  openDialog(): void {
    this.dialogVisible.set(true);
  }

  closeDialog(): void {
    this.dialogVisible.set(false);
    this.content.set('');
    this.error.set(null);
    this.aiPanelVisible.set(false);
    this.aiSuggestion.set(null);
    this.aiError.set(null);
    this.pendingMediaFiles.set([]);
    this.selectedMediaFiles.set([]);
    this.mediaError.set(null);
    this.uploadProgress.set({});
    this.mediaValidating.set(false);
    // Only revoke URLs that were NOT committed to an emitted post (i.e. user cancelled)
    this._localPreviewMap.forEach(url => {
      if (!this._committedPreviewUrls.has(url)) URL.revokeObjectURL(url);
    });
    this._localPreviewMap.clear();
  }

  /** Shows the waiting dialog for video posts */
  private showWaitingDialog(): void {
    this.waitingCountdown.set(5);
    this.waitingDialogVisible.set(true);

    // Start countdown
    this._waitingCountdownInterval = setInterval(() => {
      const current = this.waitingCountdown();
      if (current > 1) {
        this.waitingCountdown.set(current - 1);
      } else {
        this.closeWaitingDialog();
      }
    }, 1000);
  }

  /** Closes the waiting dialog and cleans up resources */
  closeWaitingDialog(): void {
    if (this._waitingCountdownInterval) {
      clearInterval(this._waitingCountdownInterval);
      this._waitingCountdownInterval = null;
    }
    this.waitingDialogVisible.set(false);
    this.closeDialog();
  }

  submit(): void {
    if (!this.canSubmit()) return;

    this.submitting.set(true);
    this.error.set(null);

    const mediaFileIds = this.selectedMediaFiles().map(m => m.id);
    const contentText = this.content().trim();

    // ── Determine if post contains video ──────────────────────
    const hasVideo = this.selectedMediaFiles().some(m => m.contentType.startsWith('video/'));

    // ── Snapshot image attachments with local blob URLs BEFORE the API call ──
    // The server's WithMedia endpoint returns a placeholder with no mediaAttachments,
    // so we must build them client-side for the optimistic post emit.
    const imageAttachmentsSnapshot = this.selectedMediaFiles()
      .filter(m => !m.contentType.startsWith('video/'))
      .map(m => {
        const localUrl = this._localPreviewMap.get(m.id) ?? m.url;
        if (localUrl.startsWith('blob:')) this._committedPreviewUrls.add(localUrl);
        return { fileId: m.id, url: localUrl, contentType: m.contentType, width: m.width, height: m.height };
      });

    // ── Determine which endpoint to use ──────────────────────
    const postRequest = mediaFileIds.length > 0
      ? this._postApi.createPostWithMedia({ content: contentText, mediaFileIds })
      : this._postApi.createPost({ content: contentText });

    postRequest.subscribe({
      next: (response) => {
        this.submitting.set(false);

        // ── If post has video, show waiting dialog ────────────
        if (hasVideo) {
          // this.showWaitingDialog();
          this._messageService.add({
            severity: 'info',
            summary: 'Processing',
            detail: 'Your video post is being processed. We\'ll notify you when it\'s ready.',
            life: 4000
          });
        } else {
          // ── For text/image posts, emit immediately with local preview URLs ──────────
          // Prefer the snapshot over server response since server returns no mediaAttachments for WithMedia posts.
          const postToEmit: Post = {
            ...response.post,
            mediaAttachments: imageAttachmentsSnapshot.length > 0
              ? imageAttachmentsSnapshot
              : response.post.mediaAttachments
          };
          this.postCreated.emit(postToEmit);
          this._messageService.add({
            severity: 'success',
            summary: 'Posted!',
            detail: 'Your post has been published.',
            life: 3000
          });
        }
        this.closeDialog();
      },
      error: () => {
        this.error.set('Failed to create post. Please try again.');
        this.submitting.set(false);
        this._messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to create post',
          life: 3000
        });
      }
    });
  }

  onContentChange(value: string): void {
    this.content.set(value);
  }

  // ── AI Actions ───────────────────────────────────────────────
  toggleAiPanel(): void {
    this.aiPanelVisible.update(v => !v);
    if (!this.aiPanelVisible()) {
      this.aiSuggestion.set(null);
      this.aiError.set(null);
    }
  }

  runAiAction(action: AiAction): void {
    this.aiLoading.set(true);
    this.aiSuggestion.set(null);
    this.aiError.set(null);

    this._aiApi.assist({
      action,
      content: this.content() || undefined,
      tone: action === 'tone' ? this.selectedTone : undefined
    }).subscribe({
      next: (res) => {
        this.aiSuggestion.set(res.suggestion);
        this.aiLoading.set(false);
      },
      error: () => {
        this.aiError.set('AI assistant is unavailable. Please try again.');
        this.aiLoading.set(false);
      }
    });
  }

  applySuggestion(): void {
    const suggestion = this.aiSuggestion();
    if (suggestion) {
      this.content.set(suggestion);
      this.aiSuggestion.set(null);
    }
  }

  dismissSuggestion(): void {
    this.aiSuggestion.set(null);
  }

  // ── Media Actions ────────────────────────────────────────────
  /**
   * Opens the file input dialog
   */
  onAttachClick(): void {
    if (this.canAttachMore()) {
      this.fileInput?.nativeElement.click();
    }
  }

  /**
   * Handles file selection from input
   */
  onMediaFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const files = Array.from(input.files);
      this.validateAndUploadFiles(files);
    }
    input.value = ''; // Reset input
  }

  /**
   * Validates files before upload and creates local preview URLs, then initiates upload
   */
  private validateAndUploadFiles(files: File[]): void {
    // ── Check total file count ──────────────────────────
    const availableSlots = MAX_FILES - this.mediaCount();
    if (files.length > availableSlots) {
      this._messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: `You can only attach ${availableSlots} more file(s). Max is ${MAX_FILES}.`,
        life: 3000
      });
      // Slice to available slots
      files = files.slice(0, availableSlots);
    }

    // ── Validate each file ──────────────────────────────
    const validFiles: File[] = [];
    for (const file of files) {
      const validationError = this.validateFile(file);
      if (validationError) {
        this._messageService.add({
          severity: 'error',
          summary: 'Invalid File',
          detail: `${file.name}: ${validationError}`,
          life: 4000
        });
      } else {
        validFiles.push(file);
      }
    }

    if (validFiles.length > 0) {
      this.uploadMediaFiles(validFiles);
    }
  }

  /**
   * Validates a single file against type and size restrictions
   */
  private validateFile(file: File): string | null {
    const isImage = ALLOWED_IMAGE_TYPES.includes(file.type);
    const isVideo = ALLOWED_VIDEO_TYPES.includes(file.type);

    if (!isImage && !isVideo) {
      return `Unsupported file type. Allowed: JPEG, PNG, WebP, GIF (images) or MP4, WebM (videos).`;
    }

    if (isImage && file.size > MAX_IMAGE_SIZE) {
      const maxMB = MAX_IMAGE_SIZE / (1024 * 1024);
      return `Image exceeds ${maxMB}MB limit. Size: ${(file.size / (1024 * 1024)).toFixed(2)}MB`;
    }

    if (isVideo && file.size > MAX_VIDEO_SIZE) {
      const maxMB = MAX_VIDEO_SIZE / (1024 * 1024);
      return `Video exceeds ${maxMB}MB limit. Size: ${(file.size / (1024 * 1024)).toFixed(2)}MB`;
    }

    return null;
  }

  /**
   * Uploads media files with progress tracking
   */
  private uploadMediaFiles(files: File[]): void {
    const isImageType = (contentType: string) => contentType.startsWith('image/');
    const createPendingId = () => `pending-${crypto.randomUUID()}`;

    const uploadEntries = files.map(file => {
      const pendingId = createPendingId();
      const previewUrl = URL.createObjectURL(file);

      const pendingMedia: MediaFile = {
        id: pendingId,
        url: previewUrl,
        originalFileName: file.name,
        contentType: file.type,
        fileSizeBytes: file.size,
        width: isImageType(file.type) ? 0 : undefined,
        height: isImageType(file.type) ? 0 : undefined,
        purpose: 'PostMedia',
        status: 'Uploading',
        createdAt: new Date().toISOString()
      };

      return { pendingId, file, previewUrl, pendingMedia };
    });

    this.pendingMediaFiles.update(current => [...current, ...uploadEntries.map(entry => entry.pendingMedia)]);

    uploadEntries.forEach(entry => {
      this._localPreviewMap.set(entry.pendingId, entry.previewUrl);
    });

    // ── Initialize progress tracking ────────────────────
    const progressMap: MediaUploadProgress = {};
    uploadEntries.forEach(entry => {
      progressMap[entry.pendingId] = {
        percentage: 0,
        loaded: 0,
        total: entry.file.size,
        status: 'uploading'
      };
    });
    this.uploadProgress.update(current => ({ ...current, ...progressMap }));
    this.mediaUploading.set(true);
    this.mediaError.set(null);

    // ── Progress callback ──────────────────────────────
    const onProgress = (fileIndex: number, loaded: number, total: number) => {
      const entry = uploadEntries[fileIndex];
      if (!entry) return;

      const percentage = Math.round((loaded / total) * 100);
      this.uploadProgress.update(progress => ({
        ...progress,
        [entry.pendingId]: {
          percentage,
          loaded,
          total,
          status: percentage === 100 ? 'validating' : 'uploading'
        }
      }));
    };

    this._uploadApi.uploadFileBatch(files, 'PostMedia', onProgress).subscribe({
      next: (uploadedFiles: MediaFile[]) => {
        // Move local preview URLs from temporary pending IDs to final server media IDs.
        uploadedFiles.forEach((mf, index) => {
          const entry = uploadEntries[index];
          if (!entry) return;

          this._localPreviewMap.set(mf.id, entry.previewUrl);
          this._localPreviewMap.delete(entry.pendingId);
        });

        // ── Update progress to validating ───────────────
        uploadEntries.forEach(entry => {
          this.uploadProgress.update(progress => ({
            ...progress,
            [entry.pendingId]: {
              ...progress[entry.pendingId],
              percentage: 100,
              loaded: entry.file.size,
              total: entry.file.size,
              status: 'validating'
            }
          }));
        });

        // ── Show validating state briefly ────────────
        this.mediaValidating.set(true);
        setTimeout(() => {
          const pendingIds = new Set(uploadEntries.map(entry => entry.pendingId));
          this.pendingMediaFiles.update(current => current.filter(file => !pendingIds.has(file.id)));
          this.selectedMediaFiles.update(current => [...current, ...uploadedFiles]);
          this.mediaUploading.set(false);
          this.mediaValidating.set(false);
          this.uploadProgress.update(progress => {
            const nextProgress: MediaUploadProgress = { ...progress };
            pendingIds.forEach(id => {
              delete nextProgress[id];
            });
            return nextProgress;
          });
        }, 1000);

        this._messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: `${uploadedFiles.length} file(s) uploaded successfully`,
          life: 2000
        });
      },
      error: () => {
        const pendingIds = new Set(uploadEntries.map(entry => entry.pendingId));
        this.pendingMediaFiles.update(current => current.filter(file => !pendingIds.has(file.id)));
        uploadEntries.forEach(entry => {
          const localUrl = this._localPreviewMap.get(entry.pendingId);
          if (localUrl) {
            URL.revokeObjectURL(localUrl);
            this._localPreviewMap.delete(entry.pendingId);
          }
        });
        this.mediaError.set('Failed to upload media. Please try again.');
        this.mediaUploading.set(false);
        this.uploadProgress.update(progress => {
          const nextProgress: MediaUploadProgress = { ...progress };
          pendingIds.forEach(id => {
            delete nextProgress[id];
          });
          return nextProgress;
        });
        this._messageService.add({
          severity: 'error',
          summary: 'Upload Failed',
          detail: 'Failed to upload media files',
          life: 3000
        });
      }
    });
  }

  /**
   * Removes a media file from the selection
   */
  removeMedia(fileId: string): void {
    const url = this._localPreviewMap.get(fileId);
    if (url) {
      URL.revokeObjectURL(url);
      this._localPreviewMap.delete(fileId);
    }
    this.pendingMediaFiles.update(files => files.filter(f => f.id !== fileId));
    this.selectedMediaFiles.update(files => files.filter(f => f.id !== fileId));
  }

  /**
   * Returns a copy of the post where image mediaAttachments use local blob URLs
   * so the feed shows images immediately without waiting for CDN propagation.
   */
  private _applyLocalPreviews(post: Post): Post {
    if (!post.mediaAttachments?.length || this._localPreviewMap.size === 0) return post;
    return {
      ...post,
      mediaAttachments: post.mediaAttachments.map(a => {
        const localUrl = this._localPreviewMap.get(a.fileId);
        return localUrl ? { ...a, url: localUrl } : a;
      })
    };
  }

  private _loadCurrentUserAvatar(): void {
    const currentUserId = this._authService.getCurrentUserId();
    if (!currentUserId) {
      this.currentUserAvatarUrl.set(null);
      return;
    }

    this._userApi.getUserProfile(currentUserId).subscribe({
      next: profile => this.currentUserAvatarUrl.set(profile.avatarUrl ?? null),
      error: () => this.currentUserAvatarUrl.set(null)
    });
  }
}
