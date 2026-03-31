import { Component, Inject, OnInit } from '@angular/core';
import { MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA, MatLegacyDialog as MatDialog, MatLegacyDialogRef as MatDialogRef } from '@angular/material/legacy-dialog';
import { MdFileService } from '../../../services/md-file.service';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { MdFile } from '../../../models/md-file';
import { MoveMdFileComponent } from '../move-md-file/move-md-file.component';
import { ShowFileSystemComponent } from '../../../../commons/components/show-file-system/show-file-system.component';
import { ShowFileMetadata } from '../../../../commons/components/show-file-system/show-file-metadata';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-add-new-file-to-mde',
  templateUrl: './add-new-file-to-mde.component.html',
  styleUrls: ['./add-new-file-to-mde.component.scss']
})
export class AddNewFileToMDEComponent implements OnInit {
  public fullPath: string;


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

    const dialogRef = this.dialog.open(ShowFileSystemComponent, {
      width: '800px',
      height: '600px',
      panelClass: 'resizable-dialog-container',
      data: data
    });
    dialogRef.afterClosed().subscribe(_ => {
      this.fullPath = _.data;
    });
  }

  add() {
    this.mdFileService.addExistingFileToMDEProject(this.data, this.fullPath).subscribe(
      data => {
        this.snackBar.open(this.translate.instant('ADD_FILE.FILE_ADDED'), null, { duration: 5000 });
        // Reload the file to show the new link
        this.mdFileService.setSelectedMdFileFromSideNav(this.data);
        this.dialogRef.close();
      },
      error => {
        this.snackBar.open(this.translate.instant('ADD_FILE.ADD_ERROR', { error: error.message }), null, { duration: 5000 });
      }
    );
  }

  dismiss() {
    this.dialogRef.close();
  }
}
