import { Component, Inject, OnInit } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ShowFileSystemComponent } from '../../../../commons/components/show-file-system/show-file-system.component';
import { MdFile } from '../../../models/md-file';
import { MdFileService } from '../../../services/md-file.service';

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
    private mdFileService: MdFileService
  ) { }
  
  ngOnInit(): void { }

  openFileSystem() {
    const dialogRef = this.dialog.open(ShowFileSystemComponent, {
      width: '600px',
      height: '600px',
      data: 'project'
    });
    dialogRef.afterClosed().subscribe(_ => {
      this.directoryDestination = _.data;
    });
  }
  
  move(): void {
    this.mdFileService.moveMdFile(this.dataMdFile, this.directoryDestination)
      .subscribe(_ => {
        this.mdFileService.loadAll(null, null);
        this.dialogRef.close();
      });    
  }
  dismiss(): void {
    this.dialogRef.close();
  }
}
