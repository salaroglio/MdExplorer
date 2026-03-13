import { Component, Inject } from '@angular/core';
import { MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA, MatLegacyDialogRef as MatDialogRef } from '@angular/material/legacy-dialog';

export interface BulkExportProgressData {
  directory: string;
  processed: number;
  total: number;
  failed: number;
  status: string;
  percentComplete: number;
  currentFile?: string;
}

@Component({
  selector: 'app-bulk-export-progress-dialog',
  templateUrl: './bulk-export-progress-dialog.component.html',
  styleUrls: ['./bulk-export-progress-dialog.component.scss']
})
export class BulkExportProgressDialogComponent {

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: BulkExportProgressData,
    private dialogRef: MatDialogRef<BulkExportProgressDialogComponent>
  ) { }

  updateProgress(progressData: any): void {
    this.data.processed = progressData.processed || 0;
    this.data.total = progressData.total || 0;
    this.data.failed = progressData.failed || 0;
    this.data.percentComplete = progressData.percentComplete || 0;
    this.data.status = progressData.status || 'exporting';
    this.data.currentFile = progressData.currentFile || '';
  }

  complete(progressData: any): void {
    this.data.processed = progressData.processed || this.data.total;
    this.data.total = progressData.total || this.data.total;
    this.data.failed = progressData.failed || 0;
    this.data.percentComplete = 100;
    this.data.status = 'completed';
    this.data.currentFile = null;
  }

  get isCompleted(): boolean {
    return this.data.status === 'completed';
  }

  close(): void {
    this.dialogRef.close();
  }

  openFolder(): void {
    // Open .mdword folder via Electron shell
    if ((window as any).electronAPI?.showItemInFolder) {
      (window as any).electronAPI.showItemInFolder('.mdword');
    }
    this.dialogRef.close('open-folder');
  }
}
