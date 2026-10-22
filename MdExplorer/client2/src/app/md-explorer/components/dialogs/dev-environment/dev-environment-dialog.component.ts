import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { MdServerMessagesService } from '../../../../signalR/services/server-messages.service';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { TranslateService } from '@ngx-translate/core';
import { CodeGraphService, CodeFolderAnalysisOutcome, DescribePreview, DescriptionRun, DetectedLanguage } from '../../../services/code-graph.service';

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
  /** Che cosa aprire chiudendo: la pagina del grafo o il report (F6). */
  open?: 'graph' | 'report';
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
  /** La cartella aveva già dei linguaggi salvati: la finestra si è aperta dal menu, non dal segno appena messo. */
  public configured = false;
  public scanMode = '';
  public outcome: CodeFolderAnalysisOutcome | null = null;
  /** La grammatica in download, per lo spinner sulla riga giusta. */
  public downloading: string | null = null;
  /** «Descrivi il codice» (F4): in corso, righe di progresso, esito. */
  public describing = false;
  public describeLog: string[] = [];
  public describeDone: { files: number; described: number; rejected: number } | null = null;
  /** L'anteprima del prossimo giro (F12b): campi semplici, niente getter che allocano nel template. */
  public preview: DescribePreview | null = null;
  public previewTodo = 0;
  public previewReview = 0;

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
    // una descrizione già in corso su questa cartella (lanciata prima, o da un'altra finestra): la finestra lo dice e non
    // offre di lanciarne un'altra; si aggiorna a ogni avanzamento e si riattiva quando finisce (22/10/2026)
    this.loadRunning();
    this.serverMessages.codeDescriptionStatus$.pipe(takeUntil(this.destroy$)).subscribe(s => this.onRunStatus(s));
  }

  /** La descrizione in corso su QUESTA cartella, se c'è (null altrimenti). */
  public runningRun: DescriptionRun | null = null;

  /** La cartella come la chiama il servizio: relativa alla radice del progetto, con «/» (o «@alias»). */
  private get folderRelative(): string {
    const norm = (x: string) => (x || '').replace(/\\/g, '/').replace(/\/+$/, '');
    const root = norm(this.data.projectRoot), folder = norm(this.data.folderPath);
    const rel = folder.toLowerCase().startsWith(root.toLowerCase() + '/') ? folder.substring(root.length + 1) : folder;
    return rel.replace(/^\/+/, '');
  }

  private loadRunning(): void {
    this.codeGraph.background(this.data.projectRoot).subscribe({
      next: b => {
        const live = (b.descriptions ?? []).find(d => d.state === 'running' && d.run.programFolder === this.folderRelative);
        this.runningRun = live ? live.run : null;
      },
      error: () => { this.runningRun = null; }
    });
  }

  private onRunStatus(s: any): void {
    const run: DescriptionRun | undefined = s?.run;
    if (!run || run.programFolder !== this.folderRelative) { return; }
    const wasRunning = this.runningRun != null;
    this.runningRun = run.status === 'running' ? run : null;
    if (wasRunning && !this.runningRun) { this.loadPreview(); }
  }

  /** «Ferma» la descrizione in corso, anche se l'ha lanciata un'altra finestra. */
  stopRunning(): void {
    this.codeGraph.stopDescribe(this.data.projectRoot, this.folderRelative).subscribe({
      error: (err) => { this.errorMessage = err?.error?.error || err?.message || this.translate.instant('COMMON.ERROR'); }
    });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  /** La cartella ha già dei linguaggi scelti: si può descrivere senza rifare l'analisi. */
  get canDescribe(): boolean { return (this.outcome != null || this.selected.size > 0) && !this.loading && !this.saving && !this.analyzing && this.runningRun == null; }

  /** Il database del grafo su disco (tutti i progetti): dimensione e generazioni del dataset del codice, testo pronto. */
  public storageMb: number | null = null;
  public storageGenerations = 0;

  private loadStorage(): void {
    this.codeGraph.storage().subscribe({
      next: st => {
        const code = (st.datasets ?? []).find(d => d.name === 'code');
        this.storageMb = st.totalMb;
        this.storageGenerations = code ? code.generations.length : 0;
      },
      error: () => { this.storageMb = null; }
    });
  }

  /** Quante da descrivere e quante da rivedere: si chiede quando la cartella è già configurata e dopo ogni giro. */
  private loadPreview(): void {
    this.loadStorage();
    this.codeGraph.describePreview(this.data.projectRoot, this.data.folderPath).subscribe({
      next: (p) => {
        this.preview = p;
        this.previewTodo = p.missing + p.signatureChanged + p.bodyChanged + (p.incomplete ?? 0);
        this.previewReview = p.reviewContext + p.reviewPrompt + p.reviewNoFingerprint;
        this.updateEstimate();
      },
      // niente script ancora (mai analizzata): l'anteprima non c'è, la finestra funziona lo stesso
      error: () => { this.preview = null; this.previewTodo = 0; this.previewReview = 0; this.updateEstimate(); }
    });
  }

  describe(force = false, review = false): void {
    const connectionId = this.serverMessages.connectionId;
    if (!connectionId || this.describing) { return; }
    this.describing = true;
    this.describeLog = [];
    this.describeDone = null;
    this.errorMessage = '';
    this.codeGraph.describe(this.data.projectRoot, this.data.folderPath, connectionId, force, review).subscribe({
      error: (err) => {
        this.describing = false;
        this.errorMessage = err?.error?.error || err?.message || this.translate.instant('COMMON.ERROR');
      }
    });
  }

  openGraph(): void { this.dialogRef.close({ languages: this.selectedList, analysis: this.outcome ?? undefined, open: 'graph' }); }
  openReport(): void { this.dialogRef.close({ languages: this.selectedList, analysis: this.outcome ?? undefined, open: 'report' }); }

  cancelDescribe(): void {
    const connectionId = this.serverMessages.connectionId;
    if (connectionId) { this.codeGraph.cancelDescribe(connectionId).subscribe(); }
  }

  private onDescribeProgress(p: any): void {
    if (!p) { return; }
    switch (p.phase) {
      case 'started': this.describeLog.push(this.translate.instant('DEV_ENV.DESCRIBE_STARTED', { provider: p.provider, total: p.fileTotal, levels: p.levels, symbols: p.symbols, trivial: p.trivial })); break;
      case 'describing': this.describeLog.push(this.translate.instant('DEV_ENV.DESCRIBE_FILE', { index: p.fileIndex, total: p.fileTotal, level: p.level, file: p.file, symbols: p.symbols })); break;
      case 'file-done': this.describeLog.push(this.translate.instant('DEV_ENV.DESCRIBE_FILE_DONE', { file: p.file, described: p.described, rejected: p.rejected })); break;
      case 'file-failed': case 'file-skipped': this.describeLog.push(`⚠ ${p.file}: ${p.detail}`); break;
      case 'done': this.describing = false; this.describeDone = { files: p.files, described: p.described, rejected: p.rejected }; this.loadPreview(); break;
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
        this.configured = (res.selected?.length ?? 0) > 0;
        this.descriptionMode = this.configured ? res.describe : null;
        if (this.configured) { this.loadPreview(); }
        this.descriptionParallel = res.descriptionParallel || 3;
        // Prima volta: si propongono i linguaggi rilevati che hanno un estrattore pronto.
        if (this.selected.size === 0) {
          this.detected.filter(d => d.availability === 'ready').forEach(d => this.selected.add(d.id));
        }
        this.updateEstimate();
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
    this.updateEstimate();
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

  /**
   * «Descrivi tutto» o «Solo su richiesta» (punto 2b del 14/10/2026). Alla prima configurazione non c'è una scelta
   * preimpostata: la descrizione costa ore e quota del motore, la decide l'utente.
   */
  descriptionMode: 'all' | 'onDemand' | null = null;
  descriptionParallel = 3;
  /**
   * La stima per «descrivi tutto»: le DOMANDE all'LLM dell'anteprima (20-60 s l'una, N insieme). Dal basso (F12a) una
   * domanda non è più un file: in PlantUML 13.429 domande per 3.545 file, e la stima per file era 4 volte troppo bassa.
   * Prima della prima scansione le domande non si sanno: si mostrano i file e lo si dice. Calcolata al cambio, non in un getter.
   */
  estimate = { files: 0, questions: null as number | null, low: '', high: '' };

  private updateEstimate(): void {
    const files = this.detected.filter(d => this.selected.has(d.id)).reduce((n, d) => n + d.files, 0);
    const questions = this.preview ? this.preview.questions : null;
    const hours = (secondsPerQuestion: number) => (questions ?? 0) * secondsPerQuestion / 3600 / Math.max(1, this.descriptionParallel);
    const fmt = (h: number) => h < 1 ? `${Math.max(1, Math.round(h * 60))} min` : `${Math.round(h)} h`;
    this.estimate = { files, questions, low: fmt(hours(20)), high: fmt(hours(60)) };
  }

  get needsDescriptionChoice(): boolean { return this.hasReadySelection && this.descriptionMode == null; }

  get hasReadySelection(): boolean {
    return this.detected.some(d => this.selected.has(d.id) && this.isReady(d));
  }

  /** Salva la scelta e avvia l'analisi della cartella; il dialogo resta aperto a mostrare l'esito. */
  confirm(): void {
    if (this.saving || this.analyzing) { return; }
    this.saving = true;
    this.errorMessage = '';
    const languages = this.selectedList;
    this.codeGraph.saveLanguages(this.data.projectRoot, this.data.folderPath, languages, this.descriptionMode).subscribe({
      next: () => {
        this.saving = false;
        if (languages.length === 0) { this.dialogRef.close({ languages }); return; }
        this.analyzing = true;
        this.codeGraph.analyze(this.data.projectRoot, this.data.folderPath).subscribe({
          next: (res) => {
            this.analyzing = false;
            this.outcome = res.folder ?? null;
            if (this.outcome?.error) { this.errorMessage = this.outcome.error; } else { this.loadPreview(); }
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
