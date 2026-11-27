import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA, MatLegacyDialogModule as MatDialogModule, MatLegacyDialogRef as MatDialogRef } from '@angular/material/legacy-dialog';
import { MatLegacyButtonModule as MatButtonModule } from '@angular/material/legacy-button';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';

export type RootReviewGuardReason = 'project' | 'chat';
export type RootReviewGuardChoice = 'review' | 'keep' | 'undo' | 'later';

/**
 * L'avviso dove il contesto si spezza (sprint 2026-11-27-Modifiche-Da-Approvare-Sulle-Radici, D11): ci sono file delle radici
 * cambiati da MarkAgent e non rivisti. Standalone: lo aprono la chat, la cronologia e il cambio di progetto.
 */
@Component({
  selector: 'app-root-review-guard-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule, TranslateModule],
  template: `
    <h2 mat-dialog-title><mat-icon class="rr-icon">rate_review</mat-icon>{{ 'ROOT_REVIEW.GUARD_TITLE' | translate:{ count: data.files.length } }}</h2>
    <mat-dialog-content>
      <p>{{ ('ROOT_REVIEW.GUARD_' + data.reason.toUpperCase()) | translate }}</p>
      <ul class="rr-files">
        <li *ngFor="let f of data.files.slice(0, 8)"><code>{{ f.logicalPath }}</code> <span class="rr-count">+{{ f.added }} −{{ f.removed }}</span></li>
        <li *ngIf="data.files.length > 8">{{ 'ROOT_REVIEW.GUARD_MORE' | translate:{ count: data.files.length - 8 } }}</li>
      </ul>
      <p class="rr-hint">{{ 'ROOT_REVIEW.GUARD_HINT' | translate }}</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="close('later')">{{ 'ROOT_REVIEW.LATER' | translate }}</button>
      <button mat-button color="warn" (click)="close('undo')">{{ 'ROOT_REVIEW.UNDO_ALL' | translate }}</button>
      <button mat-button (click)="close('keep')">{{ 'ROOT_REVIEW.KEEP_ALL' | translate }}</button>
      <button mat-raised-button color="primary" (click)="close('review')">{{ 'ROOT_REVIEW.REVIEW_NOW' | translate }}</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .rr-icon { vertical-align: middle; margin-right: 8px; }
    .rr-files { margin: 8px 0; padding-left: 20px; max-height: 200px; overflow: auto; }
    .rr-files code { font-size: 12px; }
    .rr-count { font-size: 12px; color: var(--mde-text-secondary); margin-left: 6px; }
    .rr-hint { font-size: 12px; color: var(--mde-text-secondary); }
  `],
})
export class RootReviewGuardDialogComponent {
  constructor(@Inject(MAT_DIALOG_DATA) public data: { reason: RootReviewGuardReason; files: { logicalPath: string; added: number; removed: number }[] },
              private ref: MatDialogRef<RootReviewGuardDialogComponent, RootReviewGuardChoice>) {}

  close(choice: RootReviewGuardChoice): void {
    this.ref.close(choice);
  }
}
