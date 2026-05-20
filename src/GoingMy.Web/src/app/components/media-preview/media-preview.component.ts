import { Component, input, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { ProgressBarModule } from 'primeng/progressbar';
import { MediaFile } from '../../models/media.model';

export interface MediaUploadProgress {
  [fileId: string]: {
    percentage: number;
    loaded: number;
    total: number;
    status: 'uploading' | 'validating' | 'ready' | 'failed';
  };
}

@Component({
  selector: 'app-media-preview',
  standalone: true,
  imports: [CommonModule, ButtonModule, ProgressBarModule],
  templateUrl: './media-preview.component.html',
  styleUrl: './media-preview.component.css'
})
export class MediaPreviewComponent {
  // ── Inputs ────────────────────────────────────────────────
  readonly mediaFiles = input<MediaFile[]>([]);
  readonly uploadProgress = input<MediaUploadProgress>({});
  readonly isValidating = input(false);

  // ── Outputs ───────────────────────────────────────────────
  readonly removeMedia = output<string>(); // fileId

  // ── Derived State ─────────────────────────────────────────
  readonly hasFiles = computed(() => this.mediaFiles().length > 0);
  readonly fileCount = computed(() => this.mediaFiles().length);

  // ── Actions ───────────────────────────────────────────────
  onRemoveClick(fileId: string): void {
    this.removeMedia.emit(fileId);
  }

  /**
   * Get the display label for a media file (file name + size)
   */
  getFileLabel(file: MediaFile): string {
    const fileName = file.originalFileName;
    const sizeMB = (file.fileSizeBytes / (1024 * 1024)).toFixed(2);
    return `${fileName} (${sizeMB} MB)`;
  }

  /**
   * Get readable file size
   */
  getFileSize(fileSizeBytes: number): string {
    const sizeMB = (fileSizeBytes / (1024 * 1024)).toFixed(2);
    return `${sizeMB} MB`;
  }

  /**
   * Determine file type category for display icon
   */
  getFileType(contentType: string): 'image' | 'video' {
    return contentType.startsWith('video') ? 'video' : 'image';
  }

  /**
   * Get progress status for a specific file
   */
  getProgressStatus(fileId: string): MediaUploadProgress[string] | undefined {
    return this.uploadProgress()[fileId];
  }

  /**
   * Check if a file is currently uploading
   */
  isFileUploading(fileId: string): boolean {
    const progress = this.getProgressStatus(fileId);
    return progress ? progress.status === 'uploading' : false;
  }

  /**
   * Check if a file is validating
   */
  isFileValidating(fileId: string): boolean {
    const progress = this.getProgressStatus(fileId);
    return progress ? progress.status === 'validating' : false;
  }

  /**
   * Get progress percentage for a file
   */
  getFileProgress(fileId: string): number {
    const progress = this.getProgressStatus(fileId);
    return progress ? progress.percentage : 0;
  }

  /**
   * Check if a file failed to upload
   */
  isFileFailed(fileId: string): boolean {
    const progress = this.getProgressStatus(fileId);
    return progress ? progress.status === 'failed' : false;
  }
}
