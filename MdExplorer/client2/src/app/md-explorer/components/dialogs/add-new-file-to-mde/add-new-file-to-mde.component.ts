import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA, MatLegacyDialog as MatDialog, MatLegacyDialogRef as MatDialogRef } from '@angular/material/legacy-dialog';
import { MdFileService } from '../../../services/md-file.service';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { MdFile } from '../../../models/md-file';
import { MoveMdFileComponent } from '../move-md-file/move-md-file.component';
import { ShowFileSystemComponent } from '../../../../commons/components/show-file-system/show-file-system.component';
import { ShowFileMetadata } from '../../../../commons/components/show-file-system/show-file-metadata';
import { TranslateService } from '@ngx-translate/core';
import { from, Subscription } from 'rxjs';
import { concatMap, toArray } from 'rxjs/operators';
import { DocumentConversionService, OcrInstallState, OcrModel } from '../../../services/document-conversion.service';

@Component({
  selector: 'app-add-new-file-to-mde',
  templateUrl: './add-new-file-to-mde.component.html',
  styleUrls: ['./add-new-file-to-mde.component.scss']
})
export class AddNewFileToMDEComponent implements OnInit, OnDestroy {
  public fullPath: string;

  /** File picked through the (multi-select) file system dialog. */
  public selectedPaths: string[] = [];

  /** Il wizard: `files` sceglie i file, `ocr` sceglie (e scarica) il modello che legge i pdf. */
  public step: 'files' | 'ocr' = 'files';

  /** «Crea anche la versione markdown» dei pdf e dei Word scelti. */
  public alsoMarkdown = false;

  public ocrModels: OcrModel[] = [];
  public ocrModelId: string | null = null;
  public ocrLoadError: string | null = null;
  public install: OcrInstallState | null = null;
  public adding = false;

  private installWatch: Subscription | null = null;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: MdFile,
    private dialogRef: MatDialogRef<AddNewFileToMDEComponent>,
    private mdFileService: MdFileService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
    private translate: TranslateService,
    private conversion: DocumentConversionService) { }

  ngOnInit(): void {
  }

  ngOnDestroy(): void {
    this.installWatch?.unsubscribe();
  }

  /** I file su cui si lavora: quelli scelti dal dialog, o il percorso scritto a mano. */
  get paths(): string[] {
    return this.selectedPaths.length
      ? this.selectedPaths
      : (this.fullPath ? [this.fullPath] : []);
  }

  /** C'è almeno un pdf o un Word: ha senso proporre la versione markdown. */
  get hasConvertible(): boolean {
    return this.paths.some(p => DocumentConversionService.isConvertible(p));
  }

  /** Fra i file c'è un Word: va detto che senza stili conviene passare dal pdf. */
  get hasWord(): boolean {
    return this.paths.some(p => /\.docx$/i.test(p));
  }

  /** I pdf passano dall'OCR, quindi dal passo che sceglie il modello; i Word no. */
  get needsOcrStep(): boolean {
    return this.alsoMarkdown && this.paths.some(p => DocumentConversionService.isPdf(p));
  }

  get selectedOcrModel(): OcrModel | undefined {
    return this.ocrModels.find(m => m.id === this.ocrModelId);
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

  onFileSelected(event: Event) {
    this.fullPath = (event.target as HTMLInputElement).value;

  }

  openFileSystem() {
    let data = new ShowFileMetadata();
    data.title = this.translate.instant('ADD_FILE.SELECT_TITLE');
    data.typeOfSelection = "FoldersAndFiles";
    data.buttonText = this.translate.instant('ADD_FILE.ADD_TO_PROJECT');
    // FILTRO PER ESTENSIONI RIMOSSO - mostra tutti i file
    // data.fileExtensions = ['.md', '.txt', '.doc', '.docx', '.pdf'];
    data.showFileDetails = true; // Mostra dimensione e data
    data.allowMultipleSelection = true; // Abilita la multiselezione solo per questo caso

    const dialogRef = this.dialog.open(ShowFileSystemComponent, {
      width: '800px',
      height: '600px',
      panelClass: 'resizable-dialog-container',
      data: data
    });
    dialogRef.afterClosed().subscribe(result => {
      if (!result) {
        return;
      }
      // In multi-select il dialog ritorna un array di path; restiamo compatibili col singolo
      const picked = result.data;
      this.selectedPaths = Array.isArray(picked) ? picked : (picked ? [picked] : []);
      // Riflette una singola scelta anche nel campo di testo manuale
      this.fullPath = this.selectedPaths.length === 1 ? this.selectedPaths[0] : '';
    });
  }

  removeSelected(index: number) {
    this.selectedPaths.splice(index, 1);
  }

  /** Dal passo dei file a quello del modello OCR. */
  next() {
    if (!this.paths.length) {
      this.snackBar.open(this.translate.instant('ADD_FILE.NO_FILE'), null, { duration: 4000 });
      return;
    }
    this.step = 'ocr';
    this.loadOcrModels();
  }

  back() {
    this.step = 'files';
  }

  private loadOcrModels() {
    this.ocrLoadError = null;
    this.conversion.ocrModels().subscribe({
      next: answer => {
        this.ocrModels = answer.models;
        if (!this.selectedOcrModel) {
          this.ocrModelId = (answer.models.find(m => m.ready) ?? answer.models[0])?.id ?? null;
        }
      },
      error: error => this.ocrLoadError = error?.error?.error || error.message
    });
  }

  /** Scarica il motore e il modello scelto; la barra segue il download finché non finisce. */
  downloadSelectedModel() {
    const model = this.selectedOcrModel;
    if (!model || this.installing) {
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
              this.loadOcrModels();
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

  add() {
    const paths = this.paths;

    if (!paths.length) {
      this.snackBar.open(this.translate.instant('ADD_FILE.NO_FILE'), null, { duration: 4000 });
      return;
    }

    // Più file → link come elenco puntato; il primo inserimento garantisce la riga vuota.
    const asBulletList = paths.length > 1;
    const convert = this.alsoMarkdown;
    const modelId = this.needsOcrStep ? this.ocrModelId : null;
    this.adding = true;

    // Sequenziale (concatMap): ogni file viene copiato e linkato prima del successivo,
    // così le append al markdown non vanno in race tra loro.
    from(paths.map((p, i) => ({ p, i }))).pipe(
      concatMap(({ p, i }) => this.mdFileService.addExistingFileToMDEProject(this.data, p, {
        asBulletList,
        isFirst: i === 0
      })),
      toArray()
    ).subscribe({
      next: (added) => {
        const message = paths.length > 1
          ? this.translate.instant('ADD_FILE.FILES_ADDED', { count: paths.length })
          : this.translate.instant('ADD_FILE.FILE_ADDED');
        this.snackBar.open(message, null, { duration: 5000 });
        if (convert) {
          this.startConversions(added.map(a => a.copiedFullPath), modelId);
        }
        // Reload the file to show the new link(s)
        this.mdFileService.setSelectedMdFileFromSideNav(this.data);
        this.dialogRef.close();
      },
      error: (error) => {
        this.adding = false;
        this.snackBar.open(this.translate.instant('ADD_FILE.ADD_ERROR', { error: error.message }), null, { duration: 5000 });
      }
    });
  }

  /** Le copie nel progetto che sono pdf o Word partono in conversione, e proseguono a dialog chiuso. */
  private startConversions(copiedPaths: string[], modelId: string | null) {
    const sources = copiedPaths.filter(p => DocumentConversionService.isConvertible(p));
    from(sources).pipe(
      concatMap(source => this.conversion.convert(this.data, source, DocumentConversionService.isPdf(source) ? modelId : null))
    ).subscribe({
      next: job => {
        this.snackBar.open(this.translate.instant('ADD_FILE.CONVERT.STARTED', { file: job.fileName }), null, { duration: 6000 });
        this.conversion.follow(job);
      },
      error: error => this.snackBar.open(
        this.translate.instant('ADD_FILE.CONVERT.NOT_STARTED', { error: error?.error?.error || error.message }), 'OK')
    });
  }

  dismiss() {
    this.dialogRef.close();
  }
}
