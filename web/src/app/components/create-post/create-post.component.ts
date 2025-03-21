import { NgIf } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { UserAvatarComponent } from "../user-avatar/user-avatar.component";
import { MatDialog } from '@angular/material/dialog';
import { CreatePostDialogComponent } from '../../dialogs/create-post-dialog/create-post-dialog.component';
import { AppCommonComponent } from '../app-common/app-common.component';
import { AuthService } from '../../common/services/auth.service';
import { Post } from '../../common/models/post.model';

@Component({
  selector: 'create-post',
  standalone: true,
  imports: [NgIf, UserAvatarComponent],
  templateUrl: './create-post.component.html',
  styleUrl: './create-post.component.scss'
})
export class CreatePostComponent extends AppCommonComponent {
  @Input() placeholder: string = '';
  @Input() fullSize: boolean = false;
  @Input() buttonText: string = '';

  @Output() onCreatePost = new EventEmitter<Post>();


  constructor(public dialog: MatDialog, authSvc: AuthService) {
    super(authSvc);

    this.currentUser = authSvc.getCurrentUser();
  }

  public openDialog() {
    this.dialog.open(CreatePostDialogComponent, { panelClass: 'custom-panel-dialog' })
      .afterClosed().subscribe(data => {
        if (data) {
          this.onCreatePost.emit(data);
        }
      });
  }
}
