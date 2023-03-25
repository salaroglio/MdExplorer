import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MdFile } from '../../../md-explorer/models/md-file';
import { MdFileService } from '../../../md-explorer/services/md-file.service';

@Component({
  selector: 'app-new-directory',
  templateUrl: './new-directory.component.html',
  styleUrls: ['./new-directory.component.scss']
})
export class NewDirectoryComponent implements OnInit {

  constructor(@Inject(MAT_DIALOG_DATA) public data: MdFile,
    private dialogRef: MatDialogRef<NewDirectoryComponent>,
    private mdFileService: MdFileService) { }
  public directoryName: string
  ngOnInit(): void {
  }

  save() {
    this.mdFileService.CreateNewDirectoryEx(this.data.fullPath, this.directoryName, this.data.level)
      .subscribe(data => {
        this.dialogRef.close(data);
      });

  }
  dismiss() {
    this.dialogRef.close();
  }

}
