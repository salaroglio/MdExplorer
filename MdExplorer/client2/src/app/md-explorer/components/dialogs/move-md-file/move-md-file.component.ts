import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ShowFileSystemComponent } from '../../../../commons/components/show-file-system/show-file-system.component';

@Component({
  selector: 'app-move-md-file',
  templateUrl: './move-md-file.component.html',
  styleUrls: ['./move-md-file.component.scss']
})
export class MoveMdFileComponent implements OnInit {

  constructor(private dialog: MatDialog,) { }
  public directoryDestination: string;
  ngOnInit(): void {
  }
  openFileSystem() {
    const dialogRef = this.dialog.open(ShowFileSystemComponent, {
      width: '600px',
      height: '600px',
      data: null
    });

    dialogRef.afterClosed().subscribe(_ => {

      this.directoryDestination = _.data;
    });

  }
}
