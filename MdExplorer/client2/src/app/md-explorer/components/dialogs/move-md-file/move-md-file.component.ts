import { Component, Inject, OnInit } from '@angular/core';
import { MatLegacyDialog as MatDialog, MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { ShowFileSystemComponent } from '../../../../commons/components/show-file-system/show-file-system.component';
import { MdFile } from '../../../models/md-file';
import { MdFileService } from '../../../services/md-file.service';
import { MdServerMessagesService } from '../../../../signalR/services/server-messages.service';
import { ShowFileMetadata } from '../../../../commons/components/show-file-system/show-file-metadata';

@Component({
  selector: 'app-move-md-file',
  templateUrl: './move-md-file.component.html',
  styleUrls: ['./move-md-file.component.scss']
})
export class MoveMdFileComponent implements OnInit {
  public directoryDestination: string;

  constructor(
    @Inject(MAT_DIALOG_DATA) private dataMdFile: MdFile,
    private dialog: MatDialog,
    private dialogRef: MatDialogRef<MoveMdFileComponent>,
    private mdFileService: MdFileService,
    private mdServerMessages: MdServerMessagesService
  ) { }
  
  ngOnInit(): void { }

  openFileSystem() {
    let data = new ShowFileMetadata();
    data.start = 'project';
    data.title = "Project's folders";
    data.typeOfSelection = "Folders";

    const dialogRef = this.dialog.open(ShowFileSystemComponent, {
      width: '800px',
      height: '600px',
      panelClass: 'resizable-dialog-container',
      data: data,
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result && result.data) {
        this.directoryDestination = result.data;
      }
    });
  }
  
  move(): void {
    if (!this.directoryDestination || this.directoryDestination.trim() === '') {
      // Mostra un messaggio di errore se non c'è una destinazione
      alert('Please select a destination folder');
      return;
    }
    
    const oldFullPath = this.dataMdFile.fullPath;
    const oldParentPath = oldFullPath.substring(0, Math.max(
      oldFullPath.lastIndexOf('\\'), oldFullPath.lastIndexOf('/')
    ));

    this.mdFileService.moveMdFile(this.dataMdFile, this.directoryDestination)
      .subscribe({
        next: (_) => {
          // Incremental update: remove from old parent, add to new parent
          this.mdFileService.recursiveDeleteFileFromDataStore(this.dataMdFile);
          const newFullPath = this.directoryDestination +
            oldFullPath.substring(oldFullPath.lastIndexOf(oldFullPath.includes('\\') ? '\\' : '/'));
          const movedFile = { ...this.dataMdFile, fullPath: newFullPath };
          this.mdFileService.addFileToParent(movedFile, this.directoryDestination);
          this.dialogRef.close();
        },
        error: (error) => {
          console.error('Error moving file:', error);
          alert('Error moving file: ' + (error.message || 'Unknown error'));
        }
      });    
  }
  dismiss(): void {
    this.dialogRef.close();
  }
}
