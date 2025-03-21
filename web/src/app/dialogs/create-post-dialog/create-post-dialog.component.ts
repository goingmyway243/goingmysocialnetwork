import { Component } from '@angular/core';
import { UserAvatarComponent } from "../../components/user-avatar/user-avatar.component";
import { MatDialogRef } from '@angular/material/dialog';
import { AppCommonComponent } from '../../components/app-common/app-common.component';
import { PostApiService } from '../../common/services/post-api.service';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../common/services/auth.service';

@Component({
  selector: 'create-post-dialog',
  standalone: true,
  imports: [UserAvatarComponent, FormsModule],
  templateUrl: './create-post-dialog.component.html',
  styleUrl: './create-post-dialog.component.scss'
})
export class CreatePostDialogComponent extends AppCommonComponent {
  caption: string = '';

  constructor(
    private dialogRef: MatDialogRef<CreatePostDialogComponent>,
    private postApiSvc: PostApiService,
    authSvc: AuthService,
  ) {
    super(authSvc)
  }

  createPost() {
    this.postApiSvc.createPost({
      caption: this.caption,
      userId: this.authSvc.getCurrentUserId(),
    }).subscribe(result => {
      this.dialogRef.close(result);
    })
  }

  closeDialog() {
    this.dialogRef.close();
  }
}
