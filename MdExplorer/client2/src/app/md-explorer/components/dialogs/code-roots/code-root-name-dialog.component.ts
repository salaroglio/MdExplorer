import { Component, Inject } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';

export interface CodeRootNameDialogData {
  title: string;
  /** Con un'etichetta si chiede un nome; senza, è solo una conferma (cancellare). */
  label?: string;
  value?: string;
  message?: string;
  okText: string;
  danger?: boolean;
}

/**
 * Un nome (nuovo file, nuova cartella, rinomina) o una conferma (elimina) per un elemento di una radice remota (sprint
 * 2026-10-21-Cartella-Remota-Via-SSH, F3, D7). Chiude con il nome, o con true per una conferma.
 */
@Component({
  selector: 'app-code-root-name-dialog',
  template: `
    <div mat-dialog-title class="dialog-header">
      <mat-icon class="dialog-icon">{{ data.danger ? 'delete' : 'edit' }}</mat-icon>
      <span>{{ data.title }}</span>
    </div>
    <mat-dialog-content>
      <div class="dialog-container">
        <p class="intro" *ngIf="data.message">{{ data.message }}</p>
        <ng-container *ngIf="data.label">
          <label class="field-label" for="code-root-name">{{ data.label }}</label>
          <mat-form-field appearance="outline" class="alias-field">
            <input matInput id="code-root-name" [(ngModel)]="value" (keydown.enter)="ok()" autocomplete="off" cdkFocusInitial>
          </mat-form-field>
        </ng-container>
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="dialogRef.close()">{{ 'CODE_ROOTS.CANCEL' | translate }}</button>
      <button mat-flat-button [color]="data.danger ? 'warn' : 'primary'" (click)="ok()" [disabled]="!!data.label && !valid">{{ data.okText }}</button>
    </mat-dialog-actions>`,
  styleUrls: ['./code-roots-dialog.component.scss'],
  // il contenitore condiviso vuole 460px: in una finestra più stretta usciva la barra orizzontale e l'etichetta tagliata
  styles: ['.dialog-container { min-width: 0; }']
})
export class CodeRootNameDialogComponent {
  public value: string;

  constructor(@Inject(MAT_DIALOG_DATA) public data: CodeRootNameDialogData,
              public dialogRef: MatDialogRef<CodeRootNameDialogComponent, string | true | undefined>) {
    this.value = data.value || '';
  }

  /** Un nome solo, senza «/» né «..»: si crea o si rinomina dentro la cartella in cui si è. */
  get valid(): boolean {
    const v = this.value.trim();
    return v.length > 0 && v !== '.' && v !== '..' && !v.includes('/') && !v.includes('\\');
  }

  ok(): void {
    if (!this.data.label) { this.dialogRef.close(true); return; }
    if (this.valid) { this.dialogRef.close(this.value.trim()); }
  }
}
