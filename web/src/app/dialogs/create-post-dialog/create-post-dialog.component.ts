import { Component } from '@angular/core';
import { UserAvatarComponent } from "../../components/user-avatar/user-avatar.component";
import {MatDialogRef} from '@angular/material/dialog';

@Component({
  selector: 'create-post-dialog',
  standalone: true,
  imports: [UserAvatarComponent],
  templateUrl: './create-post-dialog.component.html',
  styleUrl: './create-post-dialog.component.scss'
})
export class CreatePostDialogComponent {
  constructor(public dialogRef: MatDialogRef<CreatePostDialogComponent>) {}

  public closeDialog() {
    this.dialogRef.close();
  }
}
