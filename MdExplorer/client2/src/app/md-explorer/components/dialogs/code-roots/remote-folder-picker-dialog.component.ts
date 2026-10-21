import { Component, Inject, OnInit } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { CodeRootsService, ServerBrowse, UploadConsent, uploadConsentOf } from '../../../services/code-roots.service';

export interface RemoteFolderPickerData {
  alias: string;
  /** La posizione già collegata (ssh://host/percorso), per ripartire da lì. */
  start: string | null;
}

/**
 * Sceglie una cartella su un server raggiunto via SSH (sprint 2026-10-21-Cartella-Remota-Via-SSH, F3). L'host è quello
 * che si scriverebbe dopo «ssh» (proposti gli alias di ~/.ssh/config). Se il serverino non c'è ancora su quel server lo
 * dice prima — che cosa, dove, quanto — e lo carica solo con il sì (P13). Se la home non permette di eseguire programmi
 * si sceglie un'altra cartella per il serverino (P11). Chiude con «ssh://host/percorso».
 */
@Component({
  selector: 'app-remote-folder-picker-dialog',
  templateUrl: './remote-folder-picker-dialog.component.html',
  styleUrls: ['./code-roots-dialog.component.scss', './remote-folder-picker-dialog.component.scss']
})
export class RemoteFolderPickerDialogComponent implements OnInit {
  public host = '';
  public hosts: string[] = [];
  public busy = false;
  public errorMessage = '';
  public consent: UploadConsent | null = null;
  public consentMessage = '';
  public browse: ServerBrowse | null = null;
  public pathInput = '';
  public agentFolder = '';
  public defaultAgentFolder = '~/.cache/mdexplorer';
  public showServerSettings = false;
  private startPath: string | null = null;
  /** La cartella chiesta per ultima: dopo il sì o la cartella del serverino si riprova da lì, non dalla home. */
  private requested: string | null = null;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: RemoteFolderPickerData,
    private dialogRef: MatDialogRef<RemoteFolderPickerDialogComponent, string | undefined>,
    private roots$: CodeRootsService,
  ) { }

  ngOnInit(): void {
    this.roots$.sshHosts().subscribe({ next: h => this.hosts = h, error: () => this.hosts = [] });
    const m = /^ssh:\/\/([^/]+)(\/.*)?$/.exec(this.data.start || '');
    if (m) { this.host = m[1]; this.startPath = m[2] || null; this.connect(); }
  }

  get filteredHosts(): string[] {
    const q = this.host.trim().toLowerCase();
    return q ? this.hosts.filter(h => h.toLowerCase().includes(q)) : this.hosts;
  }

  /** Entra nel server: la cartella di partenza o la home. */
  connect(): void {
    if (!this.host.trim()) { return; }
    this.loadServerSettings();
    this.open(this.startPath);
    this.startPath = null;
  }

  open(path: string | null): void {
    this.busy = true; this.errorMessage = ''; this.consent = null; this.requested = path;
    this.roots$.serverBrowse(this.host.trim(), path).subscribe({
      next: b => { this.busy = false; this.browse = b; this.pathInput = b.path; this.showServerSettings = false; },
      error: err => {
        this.busy = false;
        const consent = uploadConsentOf(err);
        if (consent) { this.consent = consent; this.consentMessage = this.messageOf(err); this.showServerSettings = true; return; }
        this.errorMessage = this.messageOf(err);
        this.showServerSettings = true;
      },
    });
  }

  goUp(): void { if (this.browse?.parent) { this.open(this.browse.parent); } }

  goTo(): void { if (this.pathInput.trim()) { this.open(this.pathInput.trim()); } }

  /** Il sì al caricamento (e la cartella del serverino scelta), poi si riprova. */
  allowAndRetry(): void { this.saveSettings(true); }

  /** Solo la cartella del serverino (home noexec), poi si riprova. */
  saveFolderAndRetry(): void { this.saveSettings(false); }

  link(): void { if (this.browse) { this.dialogRef.close(this.browse.location); } }

  close(): void { this.dialogRef.close(); }

  toggleServerSettings(): void { this.showServerSettings = !this.showServerSettings; }

  get sizeMb(): string { return this.consent ? (this.consent.size / 1024 / 1024).toFixed(1) : ''; }

  private saveSettings(allowUpload: boolean): void {
    this.busy = true; this.errorMessage = '';
    this.roots$.saveServerSettings(this.host.trim(), this.agentFolder.trim() || null, allowUpload).subscribe({
      next: () => { this.busy = false; this.open(this.requested ?? this.browse?.path ?? null); },
      error: err => { this.busy = false; this.errorMessage = this.messageOf(err); },
    });
  }

  private loadServerSettings(): void {
    this.roots$.serverSettings(this.host.trim()).subscribe({
      next: s => { this.agentFolder = s.agentFolder || ''; this.defaultAgentFolder = s.defaultAgentFolder; },
      error: () => { },
    });
  }

  private messageOf(err: any): string {
    return err?.error?.error || err?.error?.message || (typeof err?.error === 'string' ? err.error : null) || err?.message || String(err);
  }
}
