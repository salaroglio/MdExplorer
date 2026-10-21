import { Component, HostListener, Inject, OnInit } from '@angular/core';
import { MatLegacyDialog as MatDialog, MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { TranslateService } from '@ngx-translate/core';
import { CodeRootsService } from '../../../services/code-roots.service';
import { CodeRootNameDialogComponent, CodeRootNameDialogData } from './code-root-name-dialog.component';

export interface RemoteFileEditorData {
  projectRoot: string;
  /** Percorso logico @alias/…. */
  path: string;
}

/**
 * Modifica un file di una radice remota (sprint 2026-10-21-Cartella-Remota-Via-SSH, F3): il testo arriva dal serverino
 * con la sua impronta; salvando la si ripresenta, e se nel frattempo qualcun altro l'ha cambiato il serverino rifiuta
 * (P6) — si ricarica, non si sovrascrive. Ctrl+S salva. Chiude con true se ha salvato almeno una volta.
 */
@Component({
  selector: 'app-remote-file-editor-dialog',
  template: `
    <div mat-dialog-title class="dialog-header">
      <mat-icon class="dialog-icon">edit_note</mat-icon>
      <span class="editor-title">{{ data.path }}</span>
      <span class="dirty" *ngIf="dirty">●</span>
    </div>
    <mat-dialog-content class="editor-content">
      <div class="loading" *ngIf="loading"><mat-spinner diameter="24"></mat-spinner></div>
      <textarea *ngIf="!loading && loaded" class="editor" [(ngModel)]="text" (ngModelChange)="dirty = true" spellcheck="false" wrap="off"></textarea>
      <div class="error" *ngIf="errorMessage"><mat-icon>error</mat-icon><span>{{ errorMessage }}</span></div>
      <div class="saved" *ngIf="savedNote && !dirty">{{ 'REMOTE_EDITOR.SAVED' | translate }}</div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button *ngIf="changedOnServer" (click)="load()"><mat-icon>refresh</mat-icon> {{ 'REMOTE_EDITOR.RELOAD' | translate }}</button>
      <button mat-button (click)="close()">{{ 'CODE_ROOTS.CLOSE' | translate }}</button>
      <button mat-flat-button color="primary" (click)="save()" [disabled]="!loaded || saving || !dirty">
        <mat-icon>save</mat-icon> {{ 'REMOTE_EDITOR.SAVE' | translate }}
      </button>
    </mat-dialog-actions>`,
  styleUrls: ['./code-roots-dialog.component.scss'],
  styles: [`
    .editor-content { display: flex; flex-direction: column; gap: 8px; height: 70vh; max-height: 70vh; }
    .editor { flex: 1; width: 100%; min-height: 0; box-sizing: border-box; resize: none; font-family: ui-monospace, Menlo, Consolas, monospace;
      font-size: 13px; line-height: 1.45; tab-size: 4; padding: 10px; border-radius: 6px; border: 1px solid rgba(100,116,139,0.35);
      background: var(--mde-surface-2, rgba(0,0,0,0.03)); color: inherit; }
    .editor-title { font-family: ui-monospace, Menlo, Consolas, monospace; font-size: 14px; word-break: break-all; }
    .dirty { color: var(--mde-accent, #1976d2); }
    .saved { font-size: 12px; opacity: 0.7; }
  `]
})
export class RemoteFileEditorDialogComponent implements OnInit {
  public text = '';
  public loading = true;
  public loaded = false;
  public saving = false;
  public dirty = false;
  public savedNote = false;
  public changedOnServer = false;
  public errorMessage = '';
  private stamp = '';
  private savedOnce = false;

  constructor(@Inject(MAT_DIALOG_DATA) public data: RemoteFileEditorData,
              private dialogRef: MatDialogRef<RemoteFileEditorDialogComponent, boolean>,
              private roots$: CodeRootsService,
              private dialog: MatDialog,
              private translate: TranslateService) {
    dialogRef.disableClose = true;   // una modifica non salvata non si perde con un clic fuori
  }

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true; this.errorMessage = ''; this.changedOnServer = false;
    this.roots$.file(this.data.projectRoot, 'read', this.data.path).subscribe({
      next: r => { this.text = r.text ?? ''; this.stamp = r.stamp ?? ''; this.loading = false; this.loaded = true; this.dirty = false; },
      error: err => { this.loading = false; this.errorMessage = this.messageOf(err); },
    });
  }

  @HostListener('document:keydown', ['$event'])
  onKey(e: KeyboardEvent): void {
    if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 's') { e.preventDefault(); this.save(); }
  }

  save(): void {
    if (!this.loaded || this.saving || !this.dirty) { return; }
    this.saving = true; this.errorMessage = '';
    this.roots$.file(this.data.projectRoot, 'write', this.data.path, { text: this.text, stamp: this.stamp }).subscribe({
      next: r => { this.saving = false; this.stamp = r.stamp ?? this.stamp; this.dirty = false; this.savedNote = true; this.savedOnce = true; },
      error: err => {
        this.saving = false;
        this.errorMessage = this.messageOf(err);
        this.changedOnServer = /changed since read/.test(this.errorMessage);
      },
    });
  }

  close(): void {
    if (!this.dirty) { this.dialogRef.close(this.savedOnce); return; }
    const data: CodeRootNameDialogData = {
      title: this.translate.instant('REMOTE_EDITOR.DISCARD_TITLE'),
      message: this.translate.instant('REMOTE_EDITOR.DISCARD_MESSAGE', { path: this.data.path }),
      okText: this.translate.instant('REMOTE_EDITOR.DISCARD'),
      danger: true,
    };
    this.dialog.open(CodeRootNameDialogComponent, { width: '420px', data })
      .afterClosed().subscribe(ok => { if (ok === true) { this.dialogRef.close(this.savedOnce); } });
  }

  private messageOf(err: any): string {
    return err?.error?.error || err?.error?.message || (typeof err?.error === 'string' ? err.error : null) || err?.message || String(err);
  }
}
