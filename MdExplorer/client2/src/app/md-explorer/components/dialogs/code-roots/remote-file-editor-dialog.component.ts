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
 * Modifica un file di una radice remota (sprint 2026-10-21-Cartella-Remota-Via-SSH, F3): il testo arriva da MDE Remote
 * con la sua impronta; salvando la si ripresenta, e se nel frattempo qualcun altro l'ha cambiato MDE Remote rifiuta
 * (P6) — si ricarica, non si sovrascrive. Ctrl+S salva. Chiude con true se ha salvato almeno una volta.
 */
@Component({
  selector: 'app-remote-file-editor-dialog',
  template: `
    <div mat-dialog-title class="dialog-header">
      <mat-icon class="dialog-icon">edit_note</mat-icon>
      <span class="editor-title">{{ data.path }}</span>
      <span class="spacer"></span>
      <!-- La codifica (l'utente, 21/10: se un LLM rovina la codifica, raddrizzarla da MDE): quella in cui il file è letto e
           sarà salvato; il menu rilegge, ripara un file misto, o converte al salvataggio. -->
      <button mat-stroked-button class="encoding-button" [matMenuTriggerFor]="encodingMenu" [disabled]="loading || saving"
              [matTooltip]="'REMOTE_EDITOR.ENCODING_HINT' | translate: { encoding: labelOf(encoding) }">
        <mat-icon>translate</mat-icon> {{ labelOf(encoding) }}<span *ngIf="converting"> → {{ 'REMOTE_EDITOR.ON_SAVE' | translate }}</span>
      </button>
      <mat-menu #encodingMenu="matMenu">
        <div class="menu-title" mat-menu-item disabled>{{ 'REMOTE_EDITOR.READ_AS' | translate }}</div>
        <button mat-menu-item *ngFor="let e of encodings" (click)="readAs(e)"><mat-icon>visibility</mat-icon>{{ labelOf(e) }}</button>
        <div class="menu-title" mat-menu-item disabled>{{ 'REMOTE_EDITOR.REPAIR_MIXED' | translate }}</div>
        <button mat-menu-item *ngFor="let e of eightBit" (click)="readAs('mixed-' + e)"><mat-icon>healing</mat-icon>UTF-8 + {{ labelOf(e) }}</button>
        <div class="menu-title" mat-menu-item disabled>{{ 'REMOTE_EDITOR.SAVE_IN' | translate }}</div>
        <button mat-menu-item *ngFor="let e of encodings" (click)="convertTo(e)" [disabled]="e === encoding"><mat-icon>save_as</mat-icon>{{ labelOf(e) }}</button>
      </mat-menu>
      <span class="dirty" *ngIf="dirty">●</span>
    </div>
    <mat-dialog-content class="editor-content">
      <div class="loading" *ngIf="loading"><mat-spinner diameter="24"></mat-spinner></div>
      <textarea *ngIf="!loading && loaded" class="editor" [(ngModel)]="text" (ngModelChange)="dirty = true" spellcheck="false" wrap="off"></textarea>
      <div class="error" *ngIf="errorMessage"><mat-icon>error</mat-icon><span>{{ errorMessage }}</span></div>
      <div class="saved" *ngIf="savedNote && !dirty">{{ 'REMOTE_EDITOR.SAVED' | translate }}</div>
      <div class="repair-note" *ngIf="encodingNote">{{ encodingNote }}</div>
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
    .spacer { flex: 1; }
    .encoding-button { font-size: 12px; line-height: 28px; height: 30px; }
    .encoding-button mat-icon { font-size: 16px; width: 16px; height: 16px; margin-right: 4px; }
    .menu-title { font-size: 11px; text-transform: uppercase; letter-spacing: 0.04em; height: 32px; line-height: 32px; }
    .repair-note { font-size: 12px; color: var(--mde-accent, #1976d2); }
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
  /** La codifica in cui il file è letto e sarà salvato (utf-8, ibm850, windows-1252, utf-16…). */
  public encoding = '';
  /** Al salvataggio il file cambia codifica (convertito, o riparato da una lettura mista). */
  public converting = false;
  public encodingNote = '';
  public readonly encodings = ['utf-8', 'ibm850', 'windows-1252'];
  public readonly eightBit = ['ibm850', 'windows-1252'];
  public readonly labelOf = labelOf;
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

  /** @param as una codifica scelta (Rileggi come / Ripara); assente = riconosciuta dal contenuto. */
  load(as?: string): void {
    this.loading = true; this.errorMessage = ''; this.changedOnServer = false;
    this.roots$.file(this.data.projectRoot, 'read', this.data.path, as ? { encoding: as } : {}).subscribe({
      next: r => {
        this.text = r.text ?? ''; this.stamp = r.stamp ?? ''; this.loading = false; this.loaded = true; this.dirty = false;
        this.converting = false; this.encodingNote = '';
        const read = r.encoding ?? 'utf-8';
        if (read.startsWith('mixed-')) {
          // riparato: il testo è ricomposto, si salva in UNA codifica (quella a 8 bit scelta)
          this.encoding = read.substring('mixed-'.length);
          this.converting = true; this.dirty = true;
          this.encodingNote = this.translate.instant('REMOTE_EDITOR.REPAIRED_NOTE', { encoding: labelOf(this.encoding) });
        } else {
          this.encoding = read;
          if (as) { this.encodingNote = this.translate.instant('REMOTE_EDITOR.READ_AS_NOTE', { encoding: labelOf(read) }); }
        }
      },
      error: err => { this.loading = false; this.errorMessage = this.messageOf(err); },
    });
  }

  readAs(encoding: string): void {
    if (!this.dirty || encoding.startsWith('mixed-')) { this.load(encoding); return; }
    this.confirmDiscard(() => this.load(encoding));
  }

  /** Al salvataggio il file si riscrive in questa codifica. */
  convertTo(encoding: string): void {
    this.encoding = encoding;
    this.converting = true; this.dirty = true;
    this.encodingNote = this.translate.instant('REMOTE_EDITOR.CONVERT_NOTE', { encoding: labelOf(encoding) });
  }

  @HostListener('document:keydown', ['$event'])
  onKey(e: KeyboardEvent): void {
    if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 's') { e.preventDefault(); this.save(); }
  }

  save(): void {
    if (!this.loaded || this.saving || !this.dirty) { return; }
    this.saving = true; this.errorMessage = '';
    // la codifica esplicita, quella che si vede: niente stima al salvataggio
    this.roots$.file(this.data.projectRoot, 'write', this.data.path, { text: this.text, stamp: this.stamp, encoding: this.encoding || undefined }).subscribe({
      next: r => {
        this.saving = false; this.stamp = r.stamp ?? this.stamp; this.dirty = false; this.savedNote = true; this.savedOnce = true;
        this.converting = false; this.encodingNote = '';
      },
      error: err => {
        this.saving = false;
        this.errorMessage = this.messageOf(err);
        this.changedOnServer = /changed since read/.test(this.errorMessage);
      },
    });
  }

  close(): void {
    if (!this.dirty) { this.dialogRef.close(this.savedOnce); return; }
    this.confirmDiscard(() => this.dialogRef.close(this.savedOnce));
  }

  private confirmDiscard(then: () => void): void {
    const data: CodeRootNameDialogData = {
      title: this.translate.instant('REMOTE_EDITOR.DISCARD_TITLE'),
      message: this.translate.instant('REMOTE_EDITOR.DISCARD_MESSAGE', { path: this.data.path }),
      okText: this.translate.instant('REMOTE_EDITOR.DISCARD'),
      danger: true,
    };
    this.dialog.open(CodeRootNameDialogComponent, { width: '420px', data })
      .afterClosed().subscribe(ok => { if (ok === true) { then(); } });
  }

  private messageOf(err: any): string {
    return err?.error?.error || err?.error?.message || (typeof err?.error === 'string' ? err.error : null) || err?.message || String(err);
  }
}

/** Il nome da mostrare per una codifica non UTF-8 (come TextDecoding.DisplayName nel backend); vuoto per UTF-8. */
function labelOf(encoding?: string): string {
  if (encoding?.startsWith('mixed-')) { return 'UTF-8 + ' + labelOf(encoding.substring('mixed-'.length)); }
  switch (encoding) {
    case undefined: case null: case '': case 'utf-8': return 'UTF-8';
    case 'ibm850': return 'CP-850';
    case 'windows-1252': return 'CP-1252';
    case 'utf-16': return 'UTF-16 LE';
    case 'utf-16BE': return 'UTF-16 BE';
    default: return encoding;
  }
}
