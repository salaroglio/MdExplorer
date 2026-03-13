import { Injectable } from '@angular/core';
import { MatLegacyDialog as MatDialog, MatLegacyDialogRef as MatDialogRef } from '@angular/material/legacy-dialog';
import { BulkExportProgressDialogComponent } from '../components/dialogs/bulk-export-progress/bulk-export-progress-dialog.component';

@Injectable({
  providedIn: 'root'
})
export class BulkExportProgressService {
  private dialogRef: MatDialogRef<BulkExportProgressDialogComponent> | null = null;

  constructor(private dialog: MatDialog) {}

  showProgress(directoryPath: string): void {
    if (!this.dialogRef) {
      this.dialogRef = this.dialog.open(BulkExportProgressDialogComponent, {
        width: '500px',
        disableClose: true,
        data: {
          directory: directoryPath,
          processed: 0,
          total: 0,
          failed: 0,
          status: 'exporting',
          percentComplete: 0,
          currentFile: ''
        }
      });

      this.dialogRef.afterClosed().subscribe(() => {
        this.dialogRef = null;
      });
    }
  }

  updateProgress(progressData: any): void {
    if (this.dialogRef && this.dialogRef.componentInstance) {
      this.dialogRef.componentInstance.updateProgress(progressData);
    }
  }

  complete(progressData: any): void {
    if (this.dialogRef && this.dialogRef.componentInstance) {
      this.dialogRef.componentInstance.complete(progressData);
    }
  }

  hideProgress(): void {
    if (this.dialogRef) {
      this.dialogRef.close();
      this.dialogRef = null;
    }
  }
}
