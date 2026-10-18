import { Component, Inject } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { TranslateService } from '@ngx-translate/core';
import { CodeRoot, CodeRootsService } from '../../../services/code-roots.service';

export interface CodeRootsReminderData { projectRoot: string; roots: CodeRoot[]; }

/**
 * All'apertura del progetto: le radici di codice esterne che qui non si possono usare (da collegare, non trovate,
 * repository diverso). Una volta per sessione; «Non ricordarmelo più» vale per questi alias, uno nuovo si ricorda.
 * Chiude con 'open' per aprire la finestra «Radici di codice esterne».
 */
@Component({
  selector: 'app-code-roots-reminder-dialog',
  template: `
    <div mat-dialog-title class="dialog-header">
      <mat-icon class="dialog-icon">device_hub</mat-icon>
      <span>{{ 'CODE_ROOTS.REMINDER_TITLE' | translate }}</span>
    </div>
    <mat-dialog-content>
      <p class="intro">{{ 'CODE_ROOTS.REMINDER_INTRO' | translate }}</p>
      <div class="root" *ngFor="let r of data.roots">
        <div><strong>@{{ r.alias }}</strong> <span class="status" [ngClass]="r.status.toLowerCase()">{{ statusLabel(r) }}</span></div>
        <div class="reason" *ngIf="r.reason">{{ r.reason }}</div>
      </div>
      <p class="note">{{ 'CODE_ROOTS.REMINDER_NOTE' | translate }}</p>
      <mat-checkbox [(ngModel)]="dontRemind">{{ 'CODE_ROOTS.REMINDER_DONT_REMIND' | translate }}</mat-checkbox>
      <div class="error" *ngIf="errorMessage">{{ errorMessage }}</div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="close(false)" [disabled]="busy">{{ 'CODE_ROOTS.REMINDER_LATER' | translate }}</button>
      <button mat-flat-button color="primary" (click)="close(true)" [disabled]="busy">{{ 'CODE_ROOTS.REMINDER_OPEN' | translate }}</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .dialog-header { display: flex; align-items: center; gap: 8px; }
    .dialog-icon { color: var(--mde-accent, #1976d2); }
    .intro { margin: 0 0 10px; }
    .root { padding: 6px 10px; margin-bottom: 6px; border-radius: 6px; background: var(--mde-surface-2, rgba(0,0,0,0.04)); }
    .status { font-size: 12px; padding: 1px 8px; border-radius: 999px; border: 1px solid currentColor; margin-left: 6px; }
    .status.notlinked { color: #ef6c00; }
    .status.notfound, .status.mismatch { color: #c62828; }
    .reason { font-size: 12px; opacity: 0.8; margin-top: 2px; }
    .note { font-size: 12px; opacity: 0.75; margin: 10px 0; }
    .error { color: #c62828; margin-top: 8px; }
  `]
})
export class CodeRootsReminderDialogComponent {
  public dontRemind = false;
  public busy = false;
  public errorMessage = '';

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: CodeRootsReminderData,
    private dialogRef: MatDialogRef<CodeRootsReminderDialogComponent, 'open' | undefined>,
    private roots$: CodeRootsService,
    private translate: TranslateService,
  ) { }

  statusLabel(r: CodeRoot): string { return this.translate.instant('CODE_ROOTS.STATUS_' + r.status.toUpperCase()); }

  close(open: boolean): void {
    const result = open ? 'open' : undefined;
    if (!this.dontRemind) { this.dialogRef.close(result); return; }
    this.busy = true;
    this.roots$.dismissReminder(this.data.projectRoot, this.data.roots.map(r => r.alias)).subscribe({
      next: () => this.dialogRef.close(result),
      error: err => { this.busy = false; this.errorMessage = err?.error?.error || err?.message || String(err); },
    });
  }
}
