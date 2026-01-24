import { Component, Inject, OnInit } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { MdFile } from '../../../md-explorer/models/md-file';
import { MdFileService } from '../../../md-explorer/services/md-file.service';
import { NewDirectoryDialogData } from '../show-file-system/show-file-metadata';

@Component({
  selector: 'app-new-directory',
  templateUrl: './new-directory.component.html',
  styleUrls: ['./new-directory.component.scss']
})
export class NewDirectoryComponent implements OnInit {
  public directoryName: string = '';
  public isCreating: boolean = false;
  public errorMessage: string = '';

  // Suggerimenti nomi cartelle comuni
  public suggestions: string[] = [
    'docs',
    'images',
    'assets',
    'templates',
    'backup',
    'archive'
  ];

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: NewDirectoryDialogData | MdFile,
    private dialogRef: MatDialogRef<NewDirectoryComponent>,
    private mdFileService: MdFileService
  ) { }

  ngOnInit(): void {
    // Auto-focus viene gestito tramite autoFocus: true nel dialog config
  }

  /**
   * Preview del path completo
   * Cross-platform: gestisce correttamente sia / che \
   */
  public get fullPathPreview(): string {
    if (!this.directoryName) {
      return this.getParentPath();
    }

    const parentPath = this.getParentPath();
    // Determina il separatore corretto in base al path parent
    const separator = parentPath.includes('\\') ? '\\' : '/';
    return `${parentPath}${separator}${this.directoryName}`;
  }

  /**
   * Ottiene il path parent gestendo entrambi i tipi di data
   */
  private getParentPath(): string {
    if (this.isNewDialogData(this.data)) {
      return this.data.parentPath;
    } else {
      return this.data.fullPath || this.data.path;
    }
  }

  /**
   * Ottiene il nome parent gestendo entrambi i tipi di data
   */
  public getParentName(): string {
    if (this.isNewDialogData(this.data)) {
      return this.data.parentName;
    } else {
      return this.data.name;
    }
  }

  /**
   * Type guard per distinguere NewDirectoryDialogData da MdFile
   */
  private isNewDialogData(data: any): data is NewDirectoryDialogData {
    return data && 'parentPath' in data && 'parentName' in data;
  }

  /**
   * Validazione nome cartella
   * Cross-platform: caratteri non validi per Windows, Linux e Mac
   */
  public isValidName(): boolean {
    if (!this.directoryName || this.directoryName.trim() === '') {
      return false;
    }

    // Caratteri non validi per file system (Windows, Linux, Mac)
    // Windows: < > : " / \ | ? *
    // Linux/Mac: / (e null byte)
    const invalidChars = /[<>:"|?*\\\/\x00]/;
    if (invalidChars.test(this.directoryName)) {
      return false;
    }

    // Nomi riservati Windows
    const windowsReserved = /^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(\..+)?$/i;
    if (windowsReserved.test(this.directoryName.trim())) {
      return false;
    }

    // Non può iniziare o finire con spazi o punti
    if (this.directoryName !== this.directoryName.trim()) {
      return false;
    }

    if (this.directoryName.endsWith('.')) {
      return false;
    }

    return true;
  }

  /**
   * Applica un suggerimento
   */
  public applySuggestion(suggestion: string): void {
    this.directoryName = suggestion;
    this.errorMessage = '';
  }

  /**
   * Salva la nuova cartella
   */
  save(): void {
    if (!this.isValidName()) {
      this.errorMessage = 'Invalid folder name. Avoid special characters and reserved names.';
      return;
    }

    this.isCreating = true;
    this.errorMessage = '';

    const parentPath = this.getParentPath();
    const level = this.isNewDialogData(this.data) ? 0 : this.data.level;

    this.mdFileService
      .CreateNewDirectoryEx(parentPath, this.directoryName, level)
      .subscribe({
        next: (result) => {
          this.isCreating = false;
          this.dialogRef.close(result);
        },
        error: (error) => {
          this.isCreating = false;
          const errorMsg = error?.error?.message || error?.message || 'Failed to create folder';

          // Messaggi di errore specifici
          if (errorMsg.toLowerCase().includes('already exists')) {
            this.errorMessage = 'A folder with this name already exists';
          } else if (errorMsg.toLowerCase().includes('permission')) {
            this.errorMessage = 'Permission denied. Check folder permissions.';
          } else {
            this.errorMessage = errorMsg;
          }
        }
      });
  }

  dismiss(): void {
    this.dialogRef.close();
  }
}
