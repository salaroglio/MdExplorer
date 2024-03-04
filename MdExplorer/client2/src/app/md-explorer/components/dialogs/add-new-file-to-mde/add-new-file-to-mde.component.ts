import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MdFileService } from '../../../services/md-file.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MdFile } from '../../../models/md-file';
import { MoveMdFileComponent } from '../move-md-file/move-md-file.component';
import { ShowFileSystemComponent } from '../../../../commons/components/show-file-system/show-file-system.component';
import { ShowFileMetadata } from '../../../../commons/components/show-file-system/show-file-metadata';

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
    private dialog: MatDialog) { }
    
  ngOnInit(): void {
  }

  onFileSelected(event: Event) {
    this.fullPath = (event.target as HTMLInputElement).value;
    
  }

  openFileSystem() {
    let data = new ShowFileMetadata();    
    data.title = "Select File";
    data.typeOfSelection = "FoldersAndFiles";

    const dialogRef = this.dialog.open(ShowFileSystemComponent, {
      width: '600px',
      height: '600px',
      data: data 
    });
    dialogRef.afterClosed().subscribe(_ => {
      this.fullPath = _.data;
    });
  }

  add() {
    this.mdFileService.addExistingFileToMDEProject(this.data, this.fullPath).subscribe(data => {
      
      this.snackBar.open("File added", null, { duration: 5000 })
    });
    this.dialogRef.close();
  }

  dismiss() {
    this.dialogRef.close();
  }
}
