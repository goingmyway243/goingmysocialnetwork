import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-notification-dialog',
  standalone: true,
  imports: [],
  templateUrl: './notification-dialog.component.html',
  styleUrl: './notification-dialog.component.scss'
})
export class NotificationDialogComponent {
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: { message: string, durationMs: number },
    private dialogRef: MatDialogRef<NotificationDialogComponent>
  ) {
    setTimeout(() => {
      dialogRef.close();
    }, data.durationMs);
  }
}
