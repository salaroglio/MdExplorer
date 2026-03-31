import { Component, OnInit } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { Inject } from '@angular/core';
import { MdRefactoringService } from '../../../md-explorer/services/md-refactoring.service';
import { MdFileService } from '../../../md-explorer/services/md-file.service';
import { MdFile } from '../../../md-explorer/models/md-file';
import { TranslateService } from '@ngx-translate/core';


@Component({
  selector: 'app-rules',
  templateUrl: './rules.component.html',
  styleUrls: ['./rules.component.scss']
})
export class RulesComponent implements OnInit {

  public message: string;
  public fromFileName: string;
  public toFileName: string;
  public isProcessing: boolean = false;
  public errorMessage: string = '';
  public hasError: boolean = false;

  constructor(
    private dialogRef: MatDialogRef<RulesComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private refactoringService: MdRefactoringService,
    private mdFileService: MdFileService,
    private translate: TranslateService,
  ) { }

  ngOnInit(): void {    
    this.message = this.data.message;
    this.fromFileName = this.data.fromFileName;
    this.toFileName = this.data.toFileName;
   
  }

  changeName(): void {
    this.clearError();
    this.isProcessing = true;
    
    this.refactoringService.renameFileName(this.data)
      .subscribe({
        next: (data) => {
          // Normalizza il relativePath rimuovendo doppi backslash
          let normalizedRelativePath = data.relativePath || '';
          if (normalizedRelativePath.startsWith('\\\\')) {
            normalizedRelativePath = normalizedRelativePath.substring(1);
          }
          
          // Evita doppi backslash nei path
          const separator = normalizedRelativePath.endsWith('\\') ? '' : '\\';
          const oldPath = normalizedRelativePath + separator + data.oldName;
          const newPath = normalizedRelativePath + separator + data.newName;        
          const oldFile = new MdFile(data.oldName, oldPath, data.oldLevel, false);
          oldFile.relativePath = oldPath;
          oldFile.fullPath = data.oldPath;
          const newFile = new MdFile(data.newName, newPath, data.newLevel, false);
          newFile.fullPath = data.newPath;
          newFile.relativePath = newPath;
          
          this.mdFileService.changeDataStoreMdFiles(oldFile, newFile);
          
          // Forza l'indicizzazione con un delay per assicurare la consistenza
          setTimeout(() => {
            this.mdFileService.forceFileAsIndexed(newFile.fullPath);
            this.isProcessing = false;
            this.dialogRef.close(data);
          }, 150);
        },
        error: (error) => {
          console.error('Errore durante rinominazione:', error);
          this.isProcessing = false;
          
          // Mostra messaggio di errore user-friendly
          let errorMessage = this.translate.instant('SIGNALR.RENAME_FAILED');
          if (error.status === 500) {
            errorMessage = this.translate.instant('SIGNALR.RENAME_SPECIAL_CHARS');
          } else if (error.status === 400) {
            errorMessage = this.translate.instant('SIGNALR.RENAME_INVALID');
          } else if (error.status === 0) {
            errorMessage = this.translate.instant('SIGNALR.RENAME_NETWORK_ERROR');
          }
          
          // Invece di chiudere il dialog, mostra l'errore
          this.showError(errorMessage);
        }
      });
  }

  closeDialog(data: any) {
    this.dialogRef.close(null);
  }

  dismiss() {
    this.dialogRef.close(null);
  }

  showError(message: string): void {
    this.errorMessage = message;
    this.hasError = true;
  }

  clearError(): void {
    this.hasError = false;
    this.errorMessage = '';
  }

  retryRename(): void {
    this.changeName();
  }

}
