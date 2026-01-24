import { Component, Inject, OnInit } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { MdFile } from '../../../models/md-file';
import { MdFileService } from '../../../services/md-file.service';

@Component({
  selector: 'app-new-directory',
  templateUrl: './new-directory.component.html',
  styleUrls: ['./new-directory.component.scss']
})
export class NewDirectoryComponent implements OnInit {
  public directoryName: string
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: MdFile,
    private dialogRef: MatDialogRef<NewDirectoryComponent>,
    private mdFileService: MdFileService
  ) { }

  ngOnInit(): void {
  }
  dismiss() {
    this.dialogRef.close();
  }
  save() {
    this.mdFileService.CreateNewDirectory(this.data.fullPath, this.directoryName, this.data.level)
      .subscribe(data => {        
        this.mdFileService.addNewDirectory(data);        
        this.mdFileService.setSelectionMdFile(data.reverse());
      });
    this.dialogRef.close();
  }
}
