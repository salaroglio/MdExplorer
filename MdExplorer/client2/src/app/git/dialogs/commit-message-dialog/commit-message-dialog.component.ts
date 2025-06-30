import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

export interface CommitMessageDialogData {
  defaultMessage: string;
}

@Component({
  selector: 'app-commit-message-dialog',
  templateUrl: './commit-message-dialog.component.html',
  styleUrls: ['./commit-message-dialog.component.scss']
})
export class CommitMessageDialogComponent {
  commitMessage: string;

  constructor(
    public dialogRef: MatDialogRef<CommitMessageDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CommitMessageDialogData
  ) {
    this.commitMessage = data.defaultMessage || 'Update from MdExplorer';
  }

  onCancel(): void {
    this.dialogRef.close(null);
  }

  onConfirm(): void {
    if (this.commitMessage && this.commitMessage.trim()) {
      this.dialogRef.close(this.commitMessage.trim());
    }
  }
}