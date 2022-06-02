import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MdFile } from '../../models/md-file';
import { MdFileService } from '../../services/md-file.service';

@Component({
  selector: 'app-new-markdown',
  templateUrl: './new-markdown.component.html',
  styleUrls: ['./new-markdown.component.scss']
})
export class NewMarkdownComponent implements OnInit {
  public markdownTitle: string
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: MdFile,
    private dialogRef: MatDialogRef<NewMarkdownComponent>,
    private mdFileService: MdFileService
  ) { }

  ngOnInit(): void {
  }

  dismiss() {
    this.dialogRef.close();
  }
  save() {  
    this.mdFileService.CreateNewMd(this.data.fullPath, this.markdownTitle, this.data.level)
      .subscribe(data => {
        debugger;
        this.mdFileService.addNewFile(data);
        this.mdFileService.setSelectedMdFileFromSideNav(data[data.length-1]);
      });
    this.dialogRef.close();
  }
}
