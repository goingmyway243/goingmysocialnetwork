import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { MediaFile } from '../models/media.model';

@Injectable({
  providedIn: 'root'
})
export class UploadApiService {
  private readonly _http = inject(HttpClient);
  private readonly _baseUrl = `${environment.apiGatewayUrl}/api/uploads`;

  // ── 1. Single File Upload ────────────────────────────────────

  /** POST /api/uploads — Uploads a single file (multipart/form-data). */
  uploadFile(file: File, purpose: 'Avatar' | 'Cover' | 'PostMedia' = 'PostMedia'): Observable<MediaFile> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('purpose', purpose);

    return this._http.post<MediaFile>(this._baseUrl, formData);
  }

  // ── 2. Batch Upload ──────────────────────────────────────────

  /** POST /api/uploads/batch — Uploads multiple files (up to 4). */
  uploadFileBatch(files: File[], purpose: 'Avatar' | 'Cover' | 'PostMedia' = 'PostMedia'): Observable<MediaFile[]> {
    const formData = new FormData();
    files.forEach(file => formData.append('files', file));
    formData.append('purpose', purpose);

    return this._http.post<MediaFile[]>(`${this._baseUrl}/batch`, formData);
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

  // ── 4. Delete File ───────────────────────────────────────────

  /** DELETE /api/uploads/{id} — Deletes a file. */
  deleteFile(id: string): Observable<void> {
    return this._http.delete<void>(`${this._baseUrl}/${id}`);
  }
}
