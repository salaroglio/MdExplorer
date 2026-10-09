import { Component, Inject, OnInit } from '@angular/core';
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
export class DevEnvironmentDialogComponent implements OnInit {
  public loading = true;
  public saving = false;
  public analyzing = false;
  public errorMessage = '';
  public detected: DetectedLanguage[] = [];
  public selected = new Set<string>();
  public scanMode = '';
  public outcome: CodeFolderAnalysisOutcome | null = null;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: DevEnvironmentDialogData,
    private dialogRef: MatDialogRef<DevEnvironmentDialogComponent, DevEnvironmentDialogResult | undefined>,
    private codeGraph: CodeGraphService,
    private translate: TranslateService
  ) { }

  ngOnInit(): void {
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
