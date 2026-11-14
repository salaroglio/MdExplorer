import { Component, Inject, OnInit } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { CodeRootsService } from '../../../services/code-roots.service';

export interface CodeRootEncodingsData { projectRoot: string; alias: string; }

interface Row { extensions: string; encoding: string; }

/**
 * Le codifiche dei file di una radice, per estensione (l'utente, 21/10: «una configurazione in cui, in base all'estensione
 * del file, viene scelto il formato»). Un'estensione elencata si legge e si scrive in quella codifica, senza stima; una
 * non elencata è UTF-8, e se non lo è parte la stima. Si salva nel .development.yml, su Git.
 */
@Component({
  selector: 'app-code-root-encodings-dialog',
  template: `
    <div mat-dialog-title class="dialog-header">
      <mat-icon class="dialog-icon">translate</mat-icon>
      <span>{{ 'CODE_ROOT_ENCODINGS.TITLE' | translate: { alias: data.alias } }}</span>
    </div>
    <mat-dialog-content>
      <div class="dialog-container">
        <p class="intro">{{ 'CODE_ROOT_ENCODINGS.INTRO' | translate }}</p>
        <div class="loading" *ngIf="loading"><mat-spinner diameter="24"></mat-spinner></div>
        <ng-container *ngIf="!loading">
          <div class="rule" *ngFor="let row of rows; let i = index">
            <div class="rule-field">
              <label class="field-label" [for]="'enc-ext-' + i">{{ 'CODE_ROOT_ENCODINGS.EXTENSIONS' | translate }}</label>
              <mat-form-field appearance="outline" class="alias-field">
                <input matInput [id]="'enc-ext-' + i" [(ngModel)]="row.extensions" placeholder=".cbl, .cpy, .cob" autocomplete="off">
              </mat-form-field>
            </div>
            <div class="rule-field encoding">
              <label class="field-label" [for]="'enc-cod-' + i">{{ 'CODE_ROOT_ENCODINGS.ENCODING' | translate }}</label>
              <mat-form-field appearance="outline" class="alias-field">
                <mat-select [id]="'enc-cod-' + i" [(ngModel)]="row.encoding">
                  <mat-option *ngFor="let e of encodings" [value]="e.id">{{ e.label }}</mat-option>
                </mat-select>
              </mat-form-field>
            </div>
            <button mat-icon-button (click)="remove(i)" [matTooltip]="'CODE_ROOT_ENCODINGS.REMOVE' | translate"><mat-icon>delete</mat-icon></button>
          </div>
          <div class="empty" *ngIf="rows.length === 0">{{ 'CODE_ROOT_ENCODINGS.NONE' | translate }}</div>
          <div class="actions">
            <button mat-stroked-button (click)="add()"><mat-icon>add</mat-icon> {{ 'CODE_ROOT_ENCODINGS.ADD' | translate }}</button>
          </div>
          <p class="note">{{ 'CODE_ROOT_ENCODINGS.NOTE' | translate }}</p>
        </ng-container>
        <div class="error" *ngIf="errorMessage"><mat-icon>error</mat-icon><span>{{ errorMessage }}</span></div>
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="dialogRef.close()">{{ 'CODE_ROOTS.CANCEL' | translate }}</button>
      <button mat-flat-button color="primary" (click)="save()" [disabled]="loading || saving">
        <mat-icon>save</mat-icon> {{ 'CODE_ROOT_ENCODINGS.SAVE' | translate }}
      </button>
    </mat-dialog-actions>`,
  styleUrls: ['./code-roots-dialog.component.scss'],
  styles: [`
    .rule { display: flex; align-items: flex-end; gap: 10px; }
    .rule-field { display: flex; flex-direction: column; flex: 2; }
    .rule-field.encoding { flex: 1; min-width: 150px; }
    /* allineato ai campi, non al loro spazio per i suggerimenti sotto (visto a schermo il 21/10) */
    .rule > button { margin-bottom: 36px; }
  `]
})
export class CodeRootEncodingsDialogComponent implements OnInit {
  public rows: Row[] = [];
  public loading = true;
  public saving = false;
  public errorMessage = '';
  public readonly encodings = [
    { id: 'ibm850', label: 'CP-850' }, { id: 'windows-1252', label: 'CP-1252' }, { id: 'utf-8', label: 'UTF-8' },
    { id: 'utf-16', label: 'UTF-16 LE' }, { id: 'utf-16BE', label: 'UTF-16 BE' },
  ];

  constructor(@Inject(MAT_DIALOG_DATA) public data: CodeRootEncodingsData,
              public dialogRef: MatDialogRef<CodeRootEncodingsDialogComponent, boolean>,
              private roots$: CodeRootsService) { }

  ngOnInit(): void {
    this.roots$.list(this.data.projectRoot).subscribe({
      next: roots => {
        const root = roots.find(r => r.alias === this.data.alias);
        this.rows = (root?.encodings ?? []).map(r => ({ extensions: r.extensions.join(', '), encoding: r.encoding }));
        if (this.rows.length === 0) { this.add(); }
        this.loading = false;
      },
      error: err => { this.loading = false; this.errorMessage = this.messageOf(err); },
    });
  }

  add(): void { this.rows.push({ extensions: '', encoding: 'ibm850' }); }

  remove(i: number): void { this.rows.splice(i, 1); }

  save(): void {
    this.saving = true; this.errorMessage = '';
    const rules = this.rows
      .map(r => ({ encoding: r.encoding, extensions: r.extensions.split(/[,;\s]+/).map(e => e.trim()).filter(e => e) }))
      .filter(r => r.extensions.length > 0);
    this.roots$.setEncodings(this.data.projectRoot, this.data.alias, rules).subscribe({
      next: () => { this.saving = false; this.dialogRef.close(true); },
      error: err => { this.saving = false; this.errorMessage = this.messageOf(err); },
    });
  }

  private messageOf(err: any): string {
    return err?.error?.error || err?.error?.message || (typeof err?.error === 'string' ? err.error : null) || err?.message || String(err);
  }
}
