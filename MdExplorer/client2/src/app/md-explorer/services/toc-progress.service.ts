import { Injectable } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { TocProgressDialogComponent } from '../components/dialogs/toc-progress-dialog/toc-progress-dialog.component';

@Injectable({
  providedIn: 'root'
})
export class TocProgressService {
  private dialogRef: MatDialogRef<TocProgressDialogComponent> | null = null;

  constructor(private dialog: MatDialog) {}

  showProgress(directoryPath: string): void {
    if (!this.dialogRef) {
      this.dialogRef = this.dialog.open(TocProgressDialogComponent, {
        width: '500px',
        disableClose: true,
        data: {
          directory: directoryPath,
          processed: 0,
          total: 0,
          status: 'Inizializzazione...',
          percentComplete: 0,
          currentFile: ''
        }
      });
    }
  }

  updateProgress(progressData: any): void {
    if (this.dialogRef && this.dialogRef.componentInstance) {
      this.dialogRef.componentInstance.updateProgress(progressData);
    }
  }

  hideProgress(): void {
    if (this.dialogRef) {
      this.dialogRef.close();
      this.dialogRef = null;
    }
  }
}