import { Component, Inject } from '@angular/core';
import { MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA, MatLegacyDialogRef as MatDialogRef } from '@angular/material/legacy-dialog';
import { TranslateService } from '@ngx-translate/core';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
}

@Component({
  selector: 'app-confirm-dialog',
  template: `
    <h2 mat-dialog-title>{{ data.title }}</h2>
    <mat-dialog-content>
      <p>{{ data.message }}</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">{{ data.cancelText || defaultCancel }}</button>
      <button mat-raised-button color="warn" (click)="onConfirm()">{{ data.confirmText || defaultConfirm }}</button>
    </mat-dialog-actions>
  `,
  styles: [`
    mat-dialog-content p {
      margin: 0;
      font-size: 14px;
    }
    mat-dialog-actions {
      padding: 16px 0 0 0;
    }
  `]
})
export class ConfirmDialogComponent {
  defaultCancel: string;
  defaultConfirm: string;

  constructor(
    public dialogRef: MatDialogRef<ConfirmDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogData,
    private translate: TranslateService
  ) {
    this.defaultCancel = this.translate.instant('CONFIRM_DIALOG.DEFAULT_CANCEL');
    this.defaultConfirm = this.translate.instant('CONFIRM_DIALOG.DEFAULT_CONFIRM');
  }

  onConfirm(): void {
    this.dialogRef.close(true);
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}
