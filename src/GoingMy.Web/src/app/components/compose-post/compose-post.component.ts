import { Component, output, inject, signal, computed, ViewChild, ElementRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { TextareaModule } from 'primeng/textarea';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { PostApiService } from '../../services/post-api.service';
import { UploadApiService } from '../../services/upload-api.service';
import { AiApiService } from '../../services/ai-api.service';
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
  imports: [FormsModule, CommonModule, ButtonModule, DialogModule, TextareaModule, InputTextModule, SelectModule, ToastModule, TooltipModule, MediaPreviewComponent],
  providers: [MessageService],
  templateUrl: './compose-post.component.html',
  styleUrl: './compose-post.component.css'
})
export class ComposePostComponent {

  // ── 1. Dependencies ─────────────────────────────────────────
  private readonly _postApi = inject(PostApiService);
  private readonly _uploadApi = inject(UploadApiService);
  private readonly _aiApi = inject(AiApiService);
  private readonly _messageService = inject(MessageService);

  // ── 2. Template References ──────────────────────────────────
  @ViewChild('fileInput') fileInput?: ElementRef<HTMLInputElement>;

  // ── 3. Outputs ───────────────────────────────────────────────
  readonly postCreated = output<Post>();

  // ── 4. State ─────────────────────────────────────────────────
  readonly dialogVisible = signal(false);
  readonly content = signal('');
  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);

  // ── Media State ───────────────────────────────────────────────
  readonly selectedMediaFiles = signal<MediaFile[]>([]);
  readonly mediaUploading = signal(false);
  readonly mediaValidating = signal(false);
  readonly mediaError = signal<string | null>(null);
  readonly uploadProgress = signal<MediaUploadProgress>({});

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
  readonly mediaCount = computed(() => this.selectedMediaFiles().length);
  readonly canAttachMore = computed(() => this.mediaCount() < MAX_FILES);
  readonly canSubmit = computed(() => {
    const hasPostPayload = this.isValid() || this.hasMedia();
    return hasPostPayload && !this.contentTooLong() && !this.submitting() && !this.mediaUploading() && !this.mediaValidating();
  });
  readonly hasAiSuggestion = computed(() => this.aiSuggestion() !== null);

  // ── 5. Actions ───────────────────────────────────────────────
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
    this.selectedMediaFiles.set([]);
    this.mediaError.set(null);
    this.uploadProgress.set({});
    this.mediaValidating.set(false);
  }

  submit(): void {
    if (!this.canSubmit()) return;

    this.submitting.set(true);
    this.error.set(null);

    const mediaFileIds = this.selectedMediaFiles().map(m => m.id);
    const contentText = this.content().trim();

    // ── Determine which endpoint to use ──────────────────────
    const postRequest = mediaFileIds.length > 0
      ? this._postApi.createPostWithMedia({ content: contentText, mediaFileIds })
      : this._postApi.createPost({ content: contentText });

    postRequest.subscribe({
      next: (response) => {
        this.postCreated.emit(response.post);
        this.submitting.set(false);
        this._messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Post created successfully',
          life: 3000
        });
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
   * Validates files before upload and initiates upload
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
    // ── Initialize progress tracking ────────────────────
    const progressMap: MediaUploadProgress = {};
    files.forEach(file => {
      progressMap[file.name] = {
        percentage: 0,
        loaded: 0,
        total: file.size,
        status: 'uploading'
      };
    });
    this.uploadProgress.set(progressMap);
    this.mediaUploading.set(true);
    this.mediaError.set(null);

    // ── Progress callback ──────────────────────────────
    const onProgress = (fileName: string, loaded: number, total: number) => {
      const percentage = Math.round((loaded / total) * 100);
      this.uploadProgress.update(progress => ({
        ...progress,
        [fileName]: {
          percentage,
          loaded,
          total,
          status: percentage === 100 ? 'validating' : 'uploading'
        }
      }));
    };

    // ── Perform upload ──────────────────────────────────
    this._uploadApi.uploadFileBatch(files, 'PostMedia', onProgress).subscribe({
      next: (uploadedFiles: MediaFile[]) => {
        // ── Update progress to validating ───────────────
        uploadedFiles.forEach(file => {
          this.uploadProgress.update(progress => ({
            ...progress,
            [file.originalFileName]: {
              ...progress[file.originalFileName],
              status: 'validating'
            }
          }));
        });

        // ── Show validating state briefly ────────────
        this.mediaValidating.set(true);
        setTimeout(() => {
          this.selectedMediaFiles.update(current => [...current, ...uploadedFiles]);
          this.mediaUploading.set(false);
          this.mediaValidating.set(false);
          this.uploadProgress.set({});
        }, 1000);

        this._messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: `${uploadedFiles.length} file(s) uploaded successfully`,
          life: 2000
        });
      },
      error: () => {
        this.mediaError.set('Failed to upload media. Please try again.');
        this.mediaUploading.set(false);
        this.uploadProgress.set({});
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
    this.selectedMediaFiles.update(files => files.filter(f => f.id !== fileId));
  }
}
