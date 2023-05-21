import { Component, Inject, OnInit } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MdFile } from '../../../models/md-file';
import { MdFileService } from '../../../services/md-file.service';

@Component({
  selector: 'app-delete-markdown',
  templateUrl: './delete-markdown.component.html',
  styleUrls: ['./delete-markdown.component.scss']
})
export class DeleteMarkdownComponent implements OnInit {

  constructor(
    private dialogRef: MatDialogRef<DeleteMarkdownComponent>,
    private mdFileService: MdFileService,
    @Inject(MAT_DIALOG_DATA) public data: MdFile,
  ) { }

  ngOnInit(): void {
  }

  dismiss() {
    this.dialogRef.close();
  }

  delete() {
    this.mdFileService.deleteFile(this.data);
    this.dialogRef.close();
  }
}
