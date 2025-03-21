import { Component, OnDestroy } from '@angular/core';
import { UserAvatarComponent } from "../../components/user-avatar/user-avatar.component";
import { MatDialogRef } from '@angular/material/dialog';
import { AppCommonComponent } from '../../components/app-common/app-common.component';
import { PostApiService } from '../../common/services/post-api.service';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../common/services/auth.service';
import { Content } from '../../common/models/content.model';
import { ContentType } from '../../common/enums/content-type.enum';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'create-post-dialog',
  standalone: true,
  imports: [UserAvatarComponent, FormsModule, MatIconModule],
  templateUrl: './create-post-dialog.component.html',
  styleUrl: './create-post-dialog.component.scss'
})
export class CreatePostDialogComponent extends AppCommonComponent implements OnDestroy {
  caption: string = '';
  uploadFiles: File[] = [];
  objectURLs: string[] = []

  constructor(
    private dialogRef: MatDialogRef<CreatePostDialogComponent>,
    private postApiSvc: PostApiService,
    authSvc: AuthService,
  ) {
    super(authSvc)
  }

  ngOnDestroy(): void {
    this.objectURLs.forEach(p => URL.revokeObjectURL(p));
  }

  onFileSelected(evt: any): void {
    if (evt.target.files) {
      const files = [...evt.target.files];
      this.objectURLs.push(...files.map((f: File) => URL.createObjectURL(f)));
      this.uploadFiles.push(...files);
    }
  }

  createPost(): void {
    const postRequest = {
      caption: this.caption,
      userId: this.authSvc.getCurrentUserId()
    };

    this.postApiSvc.createPost(postRequest, this.uploadFiles).subscribe(result => {
      this.dialogRef.close(result);
    });
  }

  closeDialog(): void {
    this.dialogRef.close();
  }

  removeImage(index: number): void {
    const removedUrl = this.objectURLs.splice(index, 1);
    URL.revokeObjectURL(removedUrl[0]);
    this.uploadFiles.splice(index, 1);
  }
}
