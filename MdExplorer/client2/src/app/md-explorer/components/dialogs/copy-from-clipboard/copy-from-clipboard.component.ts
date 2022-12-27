import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MdFile } from '../../../models/md-file';
import { MdFileService } from '../../../services/md-file.service';


@Component({
  selector: 'app-copy-from-clipboard',
  templateUrl: './copy-from-clipboard.component.html',
  styleUrls: ['./copy-from-clipboard.component.scss']
})
export class CopyFromClipboardComponent implements OnInit {
  public imageName: string

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: MdFile,
    private dialogRef: MatDialogRef<CopyFromClipboardComponent>,
    private mdFileService: MdFileService,
    private snackBar: MatSnackBar  ) { }

  ngOnInit(): void {

  }

  dismiss() {
    this.dialogRef.close();
  }
  save() {
    debugger;
    let dataToSend = { fileName: this.imageName, fileInfoNode:this.data }
    this.mdFileService.pasteFromClipboard(dataToSend).subscribe(data => {
      this.snackBar.open("Copied", null, { duration: 5000 })
    });    
    this.dialogRef.close();
  }
}
