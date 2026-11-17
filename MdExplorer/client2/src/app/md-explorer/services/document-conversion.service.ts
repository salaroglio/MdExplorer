import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, timer } from 'rxjs';
import { switchMap, takeWhile } from 'rxjs/operators';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { MdFile } from '../models/md-file';
import { MarkAssistantService } from '../../mark-assistant/mark-assistant.service';

/** Un modello OCR del catalogo, come lo racconta il Service. */
export interface OcrModel {
  id: string;
  name: string;
  description: string;
  downloadBytes: number;
  installed: boolean;
  /** Modello e motore ci sono entrambi: si può convertire adesso. */
  ready: boolean;
}

export interface OcrModels {
  engineInstalled: boolean;
  engineVersion: string;
  models: OcrModel[];
}

/** Il download di motore e modello: `engine` → `model` → `done`, oppure `error`. */
export interface OcrInstallState {
  modelId: string | null;
  phase: 'idle' | 'engine' | 'model' | 'done' | 'error';
  bytesDownloaded: number;
  totalBytes: number;
  error: string | null;
}

/** Una conversione in sottofondo. */
export interface DocumentConversionJob {
  id: string;
  fileName: string;
  status: 'queued' | 'running' | 'done' | 'error';
  page: number;
  pages: number;
  outputRelativePath: string | null;
  truncatedPages: number[];
  warnings: string[];
  error: string | null;
  needsModel: boolean;
}

/**
 * Documenti (pdf, Word) in markdown. La conversione la fa il Service .NET in sottofondo: qui la si
 * avvia e la si segue anche dopo che il dialog è stato chiuso, perché un pdf vuole mezzo minuto a
 * pagina e nessuno resta a guardare una finestra per venti minuti.
 */
@Injectable({ providedIn: 'root' })
export class DocumentConversionService {
  private static readonly POLL_MS = 2000;

  constructor(
    private http: HttpClient,
    private snackBar: MatSnackBar,
    private translate: TranslateService,
    private mark: MarkAssistantService) { }

  /** Le estensioni che il Service sa portare in markdown. */
  static isConvertible(path: string): boolean {
    return /\.(pdf|docx)$/i.test(path || '');
  }

  static isPdf(path: string): boolean {
    return /\.pdf$/i.test(path || '');
  }

  ocrModels(): Observable<OcrModels> {
    return this.http.get<OcrModels>('../api/DocumentConversion/ocr-models');
  }

  install(modelId: string): Observable<OcrInstallState> {
    return this.http.post<OcrInstallState>(`../api/DocumentConversion/ocr-models/${encodeURIComponent(modelId)}/install`, {});
  }

  /** Lo stato del download, riletto finché non finisce (bene o male). L'ultimo valore è quello finale. */
  watchInstall(): Observable<OcrInstallState> {
    return timer(0, 1000).pipe(
      switchMap(() => this.http.get<OcrInstallState>('../api/DocumentConversion/install-state')),
      takeWhile(state => state.phase === 'engine' || state.phase === 'model', true));
  }

  /** Avvia la conversione di un file già copiato nel progetto. */
  convert(mdFile: MdFile, sourceFullPath: string, modelId: string | null): Observable<DocumentConversionJob> {
    return this.http.post<DocumentConversionJob>('../api/mdfiles/ConvertDocumentToMarkdown', {
      mdFile: { fullPath: mdFile.fullPath },
      sourceFullPath,
      modelId
    });
  }

  /** Segue la conversione fino alla fine e dice all'utente com'è andata. */
  follow(job: DocumentConversionJob): void {
    timer(DocumentConversionService.POLL_MS, DocumentConversionService.POLL_MS).pipe(
      switchMap(() => this.http.get<DocumentConversionJob>(`../api/DocumentConversion/jobs/${job.id}`)),
      takeWhile(state => state.status === 'queued' || state.status === 'running', true)
    ).subscribe({
      next: state => {
        if (state.status === 'done') {
          this.announceDone(state);
          // Un Word convertito: Mark guarda le immagini e commenta quello che sistema.
          if (state.outputRelativePath && !DocumentConversionService.isPdf(state.fileName)) {
            this.mark.beginDocumentTidy(state.outputRelativePath);
          }
        } else if (state.status === 'error') {
          this.snackBar.open(
            this.translate.instant('ADD_FILE.CONVERT.FAILED', { file: state.fileName, error: state.error }), 'OK');
        }
      },
      error: error => this.snackBar.open(
        this.translate.instant('ADD_FILE.CONVERT.FAILED', { file: job.fileName, error: error?.error?.error || error.message }), 'OK')
    });
  }

  private announceDone(state: DocumentConversionJob): void {
    if (state.truncatedPages.length) {
      // Pagine incomplete: resta a schermo finché l'utente non lo ha letto.
      this.snackBar.open(this.translate.instant('ADD_FILE.CONVERT.DONE_TRUNCATED', {
        file: state.outputRelativePath, pages: state.truncatedPages.join(', ')
      }), 'OK');
      return;
    }
    this.snackBar.open(
      this.translate.instant('ADD_FILE.CONVERT.DONE', { file: state.outputRelativePath }), null, { duration: 8000 });
  }
}
