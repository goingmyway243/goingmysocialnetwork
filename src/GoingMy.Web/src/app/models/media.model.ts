export interface MediaFile {
  id: string;
  url: string;
  originalFileName: string;
  contentType: string;
  fileSizeBytes: number;
  width?: number;
  height?: number;
  durationSeconds?: number;
  purpose: 'Avatar' | 'Cover' | 'PostMedia';
  status: 'Uploading' | 'Ready' | 'Failed' | 'Orphaned' | 'Deleted';
  createdAt: string;
}

export interface MediaAttachment {
  fileId: string;
  url: string;
  contentType: string;
  width?: number;
  height?: number;
}
