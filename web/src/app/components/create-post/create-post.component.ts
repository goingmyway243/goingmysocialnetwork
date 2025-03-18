import { NgIf } from '@angular/common';
import { Component, Input } from '@angular/core';
import { UserAvatarComponent } from "../user-avatar/user-avatar.component";
import { MatDialog } from '@angular/material/dialog';
import { CreatePostDialogComponent } from '../../dialogs/create-post-dialog/create-post-dialog.component';

@Component({
  selector: 'create-post',
  standalone: true,
  imports: [NgIf, UserAvatarComponent],
  templateUrl: './create-post.component.html',
  styleUrl: './create-post.component.scss'
})
export class CreatePostComponent {
  @Input() placeholder: string = '';
  @Input() fullSize: boolean = false;
  @Input() buttonText!: string;

  constructor(public dialog: MatDialog) { }

  public openDialog() {
    this.dialog.open(CreatePostDialogComponent, { panelClass: 'custom-panel-dialog' });
  }
}
