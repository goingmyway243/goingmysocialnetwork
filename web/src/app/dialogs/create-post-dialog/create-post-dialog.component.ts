import { Component, model } from '@angular/core';
import { UserAvatarComponent } from "../../components/user-avatar/user-avatar.component";
import { MatDialogRef } from '@angular/material/dialog';
import { AppCommonComponent } from '../../components/app-common/app-common.component';
import { IdentityService } from '../../common/services/identity.service';
import { PostApiService } from '../../common/services/post-api.service';
import { FormsModule } from '@angular/forms';

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
    identitySvc: IdentityService,
  ) {
    super(identitySvc)
  }

  onPost() {
    this.postApiSvc.createPost({
      caption: this.caption,
      userId: this.identitySvc.getCurrentUserId(),
    }).subscribe(result => {
      console.log('success: ' + result);
    })
  }

  closeDialog() {
    this.dialogRef.close();
  }
}
