import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { MdServerMessagesService } from '../../../../signalR/services/server-messages.service';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { TranslateService } from '@ngx-translate/core';
import { CodeGraphService, CodeFolderAnalysisOutcome, DetectedLanguage } from '../../../services/code-graph.service';

export interface DevEnvironmentDialogData {
  /** Il fullPath della cartella nell'albero. */
  folderPath: string;
  folderName: string;
  projectRoot: string;
  /** true quando il dialogo si apre marcando la cartella (il tag `program` è appena stato messo). */
  justMarked: boolean;
}

export interface DevEnvironmentDialogResult {
  languages: string[];
  analysis?: CodeFolderAnalysisOutcome;
}

/**
 * La finestra «Ambiente di sviluppo» (sprint «Il grafo del codice», D16): si apre quando una cartella viene marcata
 * come ambiente di sviluppo e chiede con quale linguaggio è scritto il programma. Mostra i linguaggi RILEVATI
 * (conteggio dei file per estensione, dopo gli ignore), per ciascuno se l'estrattore è pronto; la scelta va in
 * `.development.yml` (codeGraph.languages) e, confermando, parte l'analisi della cartella.
 */
@Component({
  selector: 'app-dev-environment-dialog',
  templateUrl: './dev-environment-dialog.component.html',
  styleUrls: ['./dev-environment-dialog.component.scss']
})
export class DevEnvironmentDialogComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  public loading = true;
  public saving = false;
  public analyzing = false;
  public errorMessage = '';
  public detected: DetectedLanguage[] = [];
  public selected = new Set<string>();
  public scanMode = '';
  public outcome: CodeFolderAnalysisOutcome | null = null;
  /** La grammatica in download, per lo spinner sulla riga giusta. */
  public downloading: string | null = null;
  /** «Descrivi il codice» (F4): in corso, righe di progresso, esito. */
  public describing = false;
  public describeLog: string[] = [];
  public describeDone: { files: number; described: number; rejected: number } | null = null;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: DevEnvironmentDialogData,
    private dialogRef: MatDialogRef<DevEnvironmentDialogComponent, DevEnvironmentDialogResult | undefined>,
    private codeGraph: CodeGraphService,
    private translate: TranslateService,
    private serverMessages: MdServerMessagesService
  ) { }

  ngOnInit(): void {
    this.loadLanguages();
    this.serverMessages.codeDocProgress$.pipe(takeUntil(this.destroy$)).subscribe(p => this.onDescribeProgress(p));
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  /** La cartella ha già dei linguaggi scelti: si può descrivere senza rifare l'analisi. */
  get canDescribe(): boolean { return (this.outcome != null || this.selected.size > 0) && !this.loading && !this.saving && !this.analyzing; }

  describe(force = false): void {
    const connectionId = this.serverMessages.connectionId;
    if (!connectionId || this.describing) { return; }
    this.describing = true;
    this.describeLog = [];
    this.describeDone = null;
    this.errorMessage = '';
    this.codeGraph.describe(this.data.projectRoot, this.data.folderPath, connectionId, force).subscribe({
      error: (err) => {
        this.describing = false;
        this.errorMessage = err?.error?.error || err?.message || this.translate.instant('COMMON.ERROR');
      }
    });
  }

  cancelDescribe(): void {
    const connectionId = this.serverMessages.connectionId;
    if (connectionId) { this.codeGraph.cancelDescribe(connectionId).subscribe(); }
  }

  private onDescribeProgress(p: any): void {
    if (!p) { return; }
    switch (p.phase) {
      case 'started': this.describeLog.push(this.translate.instant('DEV_ENV.DESCRIBE_STARTED', { provider: p.provider, total: p.fileTotal })); break;
      case 'describing': this.describeLog.push(this.translate.instant('DEV_ENV.DESCRIBE_FILE', { index: p.fileIndex, total: p.fileTotal, file: p.file, symbols: p.symbols })); break;
      case 'file-done': this.describeLog.push(this.translate.instant('DEV_ENV.DESCRIBE_FILE_DONE', { file: p.file, described: p.described, rejected: p.rejected })); break;
      case 'file-failed': case 'file-skipped': this.describeLog.push(`⚠ ${p.file}: ${p.detail}`); break;
      case 'done': this.describing = false; this.describeDone = { files: p.files, described: p.described, rejected: p.rejected }; break;
      case 'cancelled': this.describing = false; this.describeLog.push(this.translate.instant('DEV_ENV.DESCRIBE_CANCELLED')); break;
      case 'error': this.describing = false; this.errorMessage = p.detail || p.message; break;
    }
    if (this.describeLog.length > 200) { this.describeLog.splice(0, this.describeLog.length - 200); }
  }

  private loadLanguages(): void {
    this.loading = true;
    this.codeGraph.getLanguages(this.data.projectRoot, this.data.folderPath).subscribe({
      next: (res) => {
        this.detected = res.detected ?? [];
        this.scanMode = res.scanMode;
        this.selected = new Set(res.selected ?? []);
        // Prima volta: si propongono i linguaggi rilevati che hanno un estrattore pronto.
        if (this.selected.size === 0) {
          this.detected.filter(d => d.availability === 'ready').forEach(d => this.selected.add(d.id));
        }
        this.loading = false;
      },
      error: (err) => {
        this.errorMessage = err?.error?.error || err?.message || this.translate.instant('COMMON.ERROR');
        this.loading = false;
      }
    });
  }

  isSelected(id: string): boolean { return this.selected.has(id); }

  toggle(id: string, checked: boolean): void {
    if (checked) { this.selected.add(id); } else { this.selected.delete(id); }
  }

  isReady(d: DetectedLanguage): boolean { return d.availability === 'ready'; }

  canDownload(d: DetectedLanguage): boolean { return !this.isReady(d) && !!d.grammarId; }

  /** Scarica la grammatica (D8) e rilegge i linguaggi: la riga passa a «pronto» e resta selezionabile. */
  download(d: DetectedLanguage): void {
    if (!d.grammarId || this.downloading) { return; }
    this.downloading = d.grammarId;
    this.errorMessage = '';
    this.codeGraph.downloadGrammar(d.grammarId).subscribe({
      next: () => {
        this.downloading = null;
        this.selected.add(d.id);
        this.loadLanguages();
      },
      error: (err) => {
        this.downloading = null;
        this.errorMessage = err?.error?.error || err?.message || this.translate.instant('COMMON.ERROR');
      }
    });
  }

  get selectedList(): string[] { return Array.from(this.selected); }

  get hasReadySelection(): boolean {
    return this.detected.some(d => this.selected.has(d.id) && this.isReady(d));
  }

  /** Salva la scelta e avvia l'analisi della cartella; il dialogo resta aperto a mostrare l'esito. */
  confirm(): void {
    if (this.saving || this.analyzing) { return; }
    this.saving = true;
    this.errorMessage = '';
    const languages = this.selectedList;
    this.codeGraph.saveLanguages(this.data.projectRoot, this.data.folderPath, languages).subscribe({
      next: () => {
        this.saving = false;
        if (languages.length === 0) { this.dialogRef.close({ languages }); return; }
        this.analyzing = true;
        this.codeGraph.analyze(this.data.projectRoot, this.data.folderPath).subscribe({
          next: (res) => {
            this.analyzing = false;
            this.outcome = res.folder ?? null;
            if (this.outcome?.error) { this.errorMessage = this.outcome.error; }
          },
          error: (err) => {
            this.analyzing = false;
            this.errorMessage = err?.error?.error || err?.message || this.translate.instant('COMMON.ERROR');
          }
        });
      },
      error: (err) => {
        this.saving = false;
        this.errorMessage = err?.error?.error || err?.message || this.translate.instant('COMMON.ERROR');
      }
    });
  }

  close(): void {
    this.dialogRef.close(this.outcome || this.selected.size > 0 ? { languages: this.selectedList, analysis: this.outcome ?? undefined } : undefined);
  }

  /** «TypeScript: 980 file, estrattore non ancora disponibile» — i file senza estrattore si dichiarano, non si saltano. */
  get withoutExtractor(): { language: string; files: number }[] {
    const map = this.outcome?.filesWithoutExtractor ?? {};
    return Object.keys(map).map(k => ({ language: this.detected.find(d => d.id === k)?.displayName ?? k, files: map[k] }));
  }
}
