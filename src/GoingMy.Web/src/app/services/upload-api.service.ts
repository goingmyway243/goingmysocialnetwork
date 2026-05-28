import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';
import { MediaFile } from '../models/media.model';

export type ProgressCallback = (fileIndex: number, loaded: number, total: number) => void;

@Injectable({
  providedIn: 'root'
})
export class UploadApiService {
  private readonly _http = inject(HttpClient);
  private readonly _authService = inject(AuthService);
  private readonly _baseUrl = `${environment.apiGatewayUrl}/api/uploads`;

  // ── 1. Single File Upload ────────────────────────────────────

  /** POST /api/uploads — Uploads a single file (multipart/form-data). */
  uploadFile(file: File, purpose: 'Avatar' | 'Cover' | 'PostMedia' = 'PostMedia'): Observable<MediaFile> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('purpose', purpose);

    return this._http.post<MediaFile>(this._baseUrl, formData);
  }

  // ── 2. Batch Upload with Progress ────────────────────────────

  /**
   * POST /api/uploads/batch — Uploads multiple files (up to 4) with progress tracking.
   * Uses XMLHttpRequest for per-file progress events.
   *
   * @param files - Array of files to upload (max 4)
   * @param purpose - Upload purpose (Avatar | Cover | PostMedia)
   * @param onProgress - Optional callback for progress: (fileName, loaded, total) => void
   * @returns Observable<MediaFile[]> - Array of uploaded MediaFile objects in same order as input
   */
  uploadFileBatch(
    files: File[],
    purpose: 'Avatar' | 'Cover' | 'PostMedia' = 'PostMedia',
    onProgress?: ProgressCallback
  ): Observable<MediaFile[]> {
    return new Observable(subscriber => {
      // ── Setup FormData ──────────────────────────────────
      const formData = new FormData();
      files.forEach(file => formData.append('files', file));
      formData.append('purpose', purpose);

      // ── Setup XMLHttpRequest ────────────────────────────
      const xhr = new XMLHttpRequest();
      const endpointUrl = `${this._baseUrl}/batch`;

      // ── Track progress for entire upload ────────────────
      let lastProgressTime = 0;
      const progressThrottle = 100; // ms - prevent excessive callbacks

      xhr.upload.addEventListener('progress', (event: ProgressEvent) => {
        if (event.lengthComputable) {
          const now = Date.now();
          // ── Throttle progress callbacks ─────────────
          if (now - lastProgressTime > progressThrottle) {
            if (onProgress) {
              // Translate aggregate request progress into per-file progress by file order.
              let loadedAcrossBatch = event.loaded;
              files.forEach((file, fileIndex) => {
                const loadedForFile = Math.max(0, Math.min(file.size, loadedAcrossBatch));
                onProgress(fileIndex, loadedForFile, file.size);
                loadedAcrossBatch = Math.max(0, loadedAcrossBatch - file.size);
              });
            }
            lastProgressTime = now;
          }
        }
      });

      // ── Handle completion ───────────────────────────────
      xhr.addEventListener('load', () => {
        if (xhr.status >= 200 && xhr.status < 300) {
          try {
            const response = JSON.parse(xhr.responseText) as MediaFile[];
            subscriber.next(response);
            subscriber.complete();
          } catch (error) {
            subscriber.error(new Error('Failed to parse upload response'));
          }
        } else {
          subscriber.error(new Error(`Upload failed with status ${xhr.status}`));
        }
      });

      // ── Handle errors ──────────────────────────────────
      xhr.addEventListener('error', () => {
        subscriber.error(new Error('Upload request failed (network error)'));
      });

      xhr.addEventListener('abort', () => {
        subscriber.error(new Error('Upload was cancelled'));
      });

      // ── Set request headers and send ────────────────────
      xhr.open('POST', endpointUrl);

      // ── Add authorization header ────────────────────────
      const token = this.getAuthToken();
      if (token) {
        xhr.setRequestHeader('Authorization', `Bearer ${token}`);
      }

      // ── Send the request ────────────────────────────────
      xhr.send(formData);

      // ── Return unsubscribe function ─────────────────────
      return () => {
        if (xhr.readyState !== XMLHttpRequest.DONE) {
          xhr.abort();
        }
      };
    });
  }

  // ── 3. Get File by ID ────────────────────────────────────────

  /** GET /api/uploads/{id} — Retrieves file metadata. */
  getFile(id: string): Observable<MediaFile> {
    return this._http.get<MediaFile>(`${this._baseUrl}/${id}`);
  }

  // ── 4. Get File by Key ───────────────────────────────────────

  /**
   * GET /api/uploads/by-filekey/{fileKey} — Looks up a file by its storage key.
   * Returns null if the file is not found (e.g., old avatar was not an uploaded file).
   */
  getFileByKey(fileKey: string): Observable<MediaFile | null> {
    return this._http.get<MediaFile>(`${this._baseUrl}/by-filekey/${fileKey}`).pipe(
      catchError(() => of(null))
    );
  }

  // ── 5. Delete File ───────────────────────────────────────────

  /** DELETE /api/uploads/{id} — Deletes a file. */
  deleteFile(id: string): Observable<void> {
    return this._http.delete<void>(`${this._baseUrl}/${id}`);
  }

  // ── Helper: Get Auth Token ──────────────────────────────────

  /**
   * Retrieves the current access token from AuthService (OAuth2/OIDC).
   * The token is automatically managed and refreshed by the auth service.
   */
  private getAuthToken(): string | null {
    const token = this._authService.getAccessToken();
    return token || null;
  }
}
