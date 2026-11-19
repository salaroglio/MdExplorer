import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA, MatLegacyDialogRef as MatDialogRef } from '@angular/material/legacy-dialog';
import { Subscription } from 'rxjs';
import { DocumentConversionJob, DocumentConversionService, OcrInstallState, OcrModel } from '../../../services/document-conversion.service';

/** Il Word appena convertito e quanti dei suoi pdf allegati aspettano il modello OCR. */
export interface OcrForAttachmentsData {
  jobId: string;
  fileName: string;
  pdfs: number;
}

/**
 * Un Word convertito aveva dei pdf allegati, e il modello OCR che li legge non è scaricato: qui lo si
 * propone. Scaricato il modello, i pdf partono in coda e la finestra si chiude restituendo le conversioni
 * avviate; con «Non ora» restano allegati, ognuno con la sua pagina che dice che cosa manca.
 */
@Component({
  selector: 'app-ocr-for-attachments',
  templateUrl: './ocr-for-attachments.component.html',
  styleUrls: ['./ocr-for-attachments.component.scss']
})
export class OcrForAttachmentsComponent implements OnInit, OnDestroy {
  models: OcrModel[] = [];
  modelId: string | null = null;
  loadError: string | null = null;
  startError: string | null = null;
  install: OcrInstallState | null = null;
  starting = false;
  private installWatch: Subscription | null = null;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: OcrForAttachmentsData,
    private dialogRef: MatDialogRef<OcrForAttachmentsComponent, DocumentConversionJob[]>,
    private conversion: DocumentConversionService) { }

  ngOnInit(): void {
    this.loadModels();
  }

  ngOnDestroy(): void {
    this.installWatch?.unsubscribe();
  }

  get selected(): OcrModel | undefined {
    return this.models.find(m => m.id === this.modelId);
  }

  get installing(): boolean {
    return this.install?.phase === 'engine' || this.install?.phase === 'model';
  }

  get installPercent(): number {
    return this.install && this.install.totalBytes > 0
      ? Math.round(this.install.bytesDownloaded / this.install.totalBytes * 100)
      : 0;
  }

  megabytes(bytes: number): number {
    return Math.round(bytes / 1_000_000);
  }

  private loadModels(thenStart = false): void {
    this.loadError = null;
    this.conversion.ocrModels().subscribe({
      next: answer => {
        this.models = answer.models;
        if (!this.selected) {
          this.modelId = (answer.models.find(m => m.ready) ?? answer.models[0])?.id ?? null;
        }
        if (thenStart && this.selected?.ready) {
          this.start();
        }
      },
      error: error => this.loadError = error?.error?.error || error.message
    });
  }

  /** Scarica motore e modello, poi fa partire i pdf: un clic solo. */
  downloadAndConvert(): void {
    const model = this.selected;
    if (!model || this.installing || this.starting) {
      return;
    }
    if (model.ready) {
      this.start();
      return;
    }
    this.conversion.install(model.id).subscribe({
      next: state => {
        this.install = state;
        this.installWatch?.unsubscribe();
        this.installWatch = this.conversion.watchInstall().subscribe({
          next: latest => {
            this.install = latest;
            if (latest.phase === 'done') {
              this.loadModels(true);
            }
          },
          error: error => this.install = { ...this.install, phase: 'error', error: error?.error?.error || error.message }
        });
      },
      error: error => this.install = {
        modelId: model.id, phase: 'error', bytesDownloaded: 0, totalBytes: 0, error: error?.error?.error || error.message
      }
    });
  }

  private start(): void {
    this.starting = true;
    this.startError = null;
    this.conversion.startAttachedPdfs(this.data.jobId, this.modelId).subscribe({
      next: jobs => this.dialogRef.close(jobs),
      error: error => {
        this.starting = false;
        this.startError = error?.error?.error || error.message;
      }
    });
  }

  notNow(): void {
    this.dialogRef.close();
  }
}
