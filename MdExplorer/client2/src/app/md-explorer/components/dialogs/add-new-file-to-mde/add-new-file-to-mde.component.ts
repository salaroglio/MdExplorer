import { Component, Inject, OnInit } from '@angular/core';
import { MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA, MatLegacyDialog as MatDialog, MatLegacyDialogRef as MatDialogRef } from '@angular/material/legacy-dialog';
import { MdFileService } from '../../../services/md-file.service';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { MdFile } from '../../../models/md-file';
import { MoveMdFileComponent } from '../move-md-file/move-md-file.component';
import { ShowFileSystemComponent } from '../../../../commons/components/show-file-system/show-file-system.component';
import { ShowFileMetadata } from '../../../../commons/components/show-file-system/show-file-metadata';
import { TranslateService } from '@ngx-translate/core';
import { from } from 'rxjs';
import { concatMap, toArray } from 'rxjs/operators';

@Component({
  selector: 'app-add-new-file-to-mde',
  templateUrl: './add-new-file-to-mde.component.html',
  styleUrls: ['./add-new-file-to-mde.component.scss']
})
export class AddNewFileToMDEComponent implements OnInit {
  public fullPath: string;

  /** File picked through the (multi-select) file system dialog. */
  public selectedPaths: string[] = [];


  constructor(
    @Inject(MAT_DIALOG_DATA) public data: MdFile,
    private dialogRef: MatDialogRef<AddNewFileToMDEComponent>,
    private mdFileService: MdFileService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
    private translate: TranslateService) { }
    
  ngOnInit(): void {
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

  add() {
    const paths = this.selectedPaths.length
      ? this.selectedPaths
      : (this.fullPath ? [this.fullPath] : []);

    if (!paths.length) {
      this.snackBar.open(this.translate.instant('ADD_FILE.NO_FILE'), null, { duration: 4000 });
      return;
    }

    // Più file → link come elenco puntato; il primo inserimento garantisce la riga vuota.
    const asBulletList = paths.length > 1;

    // Sequenziale (concatMap): ogni file viene copiato e linkato prima del successivo,
    // così le append al markdown non vanno in race tra loro.
    from(paths.map((p, i) => ({ p, i }))).pipe(
      concatMap(({ p, i }) => this.mdFileService.addExistingFileToMDEProject(this.data, p, {
        asBulletList,
        isFirst: i === 0
      })),
      toArray()
    ).subscribe({
      next: () => {
        const message = paths.length > 1
          ? this.translate.instant('ADD_FILE.FILES_ADDED', { count: paths.length })
          : this.translate.instant('ADD_FILE.FILE_ADDED');
        this.snackBar.open(message, null, { duration: 5000 });
        // Reload the file to show the new link(s)
        this.mdFileService.setSelectedMdFileFromSideNav(this.data);
        this.dialogRef.close();
      },
      error: (error) => {
        this.snackBar.open(this.translate.instant('ADD_FILE.ADD_ERROR', { error: error.message }), null, { duration: 5000 });
      }
    });
  }

  dismiss() {
    this.dialogRef.close();
  }
}
