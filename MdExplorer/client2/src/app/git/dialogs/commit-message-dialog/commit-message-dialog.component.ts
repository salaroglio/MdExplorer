import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { HttpClient } from '@angular/common/http';

export interface CommitMessageDialogData {
  defaultMessage: string;
  projectPath?: string;
}

@Component({
  selector: 'app-commit-message-dialog',
  templateUrl: './commit-message-dialog.component.html',
  styleUrls: ['./commit-message-dialog.component.scss']
})
export class CommitMessageDialogComponent {
  commitMessage: string;
  isGeneratingMessage = false;
  aiError: string | null = null;

  constructor(
    public dialogRef: MatDialogRef<CommitMessageDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CommitMessageDialogData,
    private http: HttpClient
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

  generateWithAi(): void {
    if (!this.data.projectPath) {
      this.aiError = 'Project path not available';
      return;
    }

    this.isGeneratingMessage = true;
    this.aiError = null;

    this.http.post<any>('/api/GitAi/generate-commit-message', {
      projectPath: this.data.projectPath
    }).subscribe({
      next: (response) => {
        this.isGeneratingMessage = false;
        if (response.success && response.suggestedMessage) {
          this.commitMessage = response.suggestedMessage;
        } else if (response.error) {
          this.aiError = response.error;
          if (response.suggestedMessage) {
            // Use fallback message if provided
            this.commitMessage = response.suggestedMessage;
          }
        }
      },
      error: (err) => {
        this.isGeneratingMessage = false;
        this.aiError = 'Errore durante la generazione del messaggio';
        console.error('Error generating commit message:', err);
      }
    });
  }
}