import { Component, Inject, OnInit } from '@angular/core';
import { MatLegacyDialog as MatDialog, MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { CodeRoot, CodeRootChanges, CodeRootsService } from '../../../services/code-roots.service';
import { ShowFileSystemComponent } from '../../../../commons/components/show-file-system/show-file-system.component';
import { ShowFileMetadata } from '../../../../commons/components/show-file-system/show-file-metadata';
import { TranslateService } from '@ngx-translate/core';
import { DevEnvironmentDialogComponent, DevEnvironmentDialogData, DevEnvironmentDialogResult } from '../dev-environment/dev-environment-dialog.component';
import { RemoteFolderPickerDialogComponent, RemoteFolderPickerData } from './remote-folder-picker-dialog.component';

export interface CodeRootsDialogData { projectRoot: string; }

/** Che cosa aprire chiudendo: il grafo (o il report) di una radice, «@alias». */
export interface CodeRootsDialogResult { open: 'graph' | 'report'; folder: string; }

/**
 * «Radici di codice esterne» (sprint 2026-10-12-Radici-Di-Codice-Esterne, F5): il codice che sta FUORI dalla cartella
 * del progetto. Per ogni radice dichiarata nel .development.yml: lo stato su QUESTA macchina col motivo, collegarla
 * (selettore di cartelle del backend: vale anche per l'edizione web), scollegarla, linguaggi e analisi (la finestra
 * «Ambiente di sviluppo» su «@alias»), il grafo, e i file modificati del suo repository (che cosa ha toccato MarkAgent).
 */
@Component({
  selector: 'app-code-roots-dialog',
  templateUrl: './code-roots-dialog.component.html',
  styleUrls: ['./code-roots-dialog.component.scss']
})
export class CodeRootsDialogComponent implements OnInit {
  public loading = true;
  public busy: string | null = null;
  public errorMessage = '';
  public roots: CodeRoot[] = [];
  public changes: { [alias: string]: CodeRootChanges | 'loading' | undefined } = {};
  public newAlias = '';
  /** Le stesse regole del backend (CodeRootPaths.IsValidAlias). */
  private readonly aliasRule = /^[a-z0-9][a-z0-9._-]{0,62}$/;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: CodeRootsDialogData,
    private dialogRef: MatDialogRef<CodeRootsDialogComponent, CodeRootsDialogResult | undefined>,
    private dialog: MatDialog,
    private roots$: CodeRootsService,
    private translate: TranslateService,
  ) { }

  ngOnInit(): void { this.reload(); }

  reload(): void {
    this.loading = true;
    this.roots$.list(this.data.projectRoot).subscribe({
      next: roots => { this.roots = roots; this.loading = false; },
      error: err => { this.errorMessage = this.messageOf(err); this.loading = false; },
    });
  }

  statusLabel(r: CodeRoot): string { return this.translate.instant('CODE_ROOTS.STATUS_' + r.status.toUpperCase()); }

  get newAliasValid(): boolean { return this.aliasRule.test(this.newAlias.trim()); }

  /** Collega (o ricollega) una radice: si sceglie la cartella sul file system del servizio. */
  pickAndLink(alias: string, start: string | null): void {
    if (!this.aliasRule.test(alias)) { this.errorMessage = this.translate.instant('CODE_ROOTS.ALIAS_RULE'); return; }
    const picker = new ShowFileMetadata();
    picker.title = this.translate.instant('CODE_ROOTS.PICK_TITLE', { alias });
    picker.typeOfSelection = 'Folders';
    picker.buttonText = this.translate.instant('CODE_ROOTS.PICK_BUTTON');
    picker.start = start && !start.startsWith('ssh://') ? start : 'root';
    this.dialog.open(ShowFileSystemComponent, { width: '900px', height: '700px', panelClass: 'resizable-dialog-container', data: picker })
      .afterClosed().subscribe(result => {
        const folder = typeof result?.data === 'string' ? result.data : null;
        if (!folder) return;
        this.busy = alias; this.errorMessage = '';
        this.roots$.link(this.data.projectRoot, alias, folder).subscribe({
          next: () => { this.busy = null; this.newAlias = ''; this.reload(); },
          error: err => { this.busy = null; this.errorMessage = this.messageOf(err); },
        });
      });
  }

  /**
   * Collega (o ricollega) una radice a una cartella su un server via SSH (sprint 2026-10-21-Cartella-Remota-Via-SSH, F3):
   * la finestra sceglie host e cartella, e chiede il sì prima di caricare il serverino.
   */
  pickRemoteAndLink(alias: string, start: string | null): void {
    if (!this.aliasRule.test(alias)) { this.errorMessage = this.translate.instant('CODE_ROOTS.ALIAS_RULE'); return; }
    const data: RemoteFolderPickerData = { alias, start: start && start.startsWith('ssh://') ? start : null };
    this.dialog.open(RemoteFolderPickerDialogComponent, { width: '720px', data })
      .afterClosed().subscribe((location?: string) => {
        if (!location) return;
        this.busy = alias; this.errorMessage = '';
        this.roots$.link(this.data.projectRoot, alias, location).subscribe({
          next: () => { this.busy = null; this.newAlias = ''; this.reload(); },
          error: err => { this.busy = null; this.errorMessage = this.messageOf(err); },
        });
      });
  }

  unlink(r: CodeRoot): void {
    this.busy = r.alias; this.errorMessage = '';
    this.roots$.unlink(this.data.projectRoot, r.alias).subscribe({
      next: () => { this.busy = null; delete this.changes[r.alias]; this.reload(); },
      error: err => { this.busy = null; this.errorMessage = this.messageOf(err); },
    });
  }

  /** Linguaggi, analisi e descrizioni: la finestra «Ambiente di sviluppo» sulla radice («@alias»). */
  openLanguages(r: CodeRoot): void {
    const data: DevEnvironmentDialogData = { folderPath: '@' + r.alias, folderName: '@' + r.alias, projectRoot: this.data.projectRoot, justMarked: false };
    this.dialog.open(DevEnvironmentDialogComponent, { width: '560px', data })
      .afterClosed().subscribe((result?: DevEnvironmentDialogResult) => {
        if (result?.open) { this.dialogRef.close({ open: result.open, folder: '@' + r.alias }); return; }
        this.reload();
      });
  }

  openGraph(r: CodeRoot): void { this.dialogRef.close({ open: 'graph', folder: '@' + r.alias }); }

  toggleChanges(r: CodeRoot): void {
    if (this.changes[r.alias]) { delete this.changes[r.alias]; return; }
    this.changes[r.alias] = 'loading';
    this.roots$.changes(this.data.projectRoot, r.alias).subscribe({
      next: c => this.changes[r.alias] = c,
      error: err => { delete this.changes[r.alias]; this.errorMessage = this.messageOf(err); },
    });
  }

  changesOf(r: CodeRoot): CodeRootChanges | null {
    const c = this.changes[r.alias];
    return c && c !== 'loading' ? c : null;
  }

  close(): void { this.dialogRef.close(); }

  private messageOf(err: any): string {
    return err?.error?.error || err?.error?.message || (typeof err?.error === 'string' ? err.error : null) || err?.message || String(err);
  }
}
