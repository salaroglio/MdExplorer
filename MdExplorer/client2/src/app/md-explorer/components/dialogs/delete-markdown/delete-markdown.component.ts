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
    
    this.mdFileService.deleteFile(this.data)
      .subscribe(_ => {
        debugger;
        
        let filePath = this.mdFileService.recursiveSearchForShowData(this.data);
        filePath.reverse().pop(); // i'm ripping away the file element
        let folderContainer = filePath.reverse(); // i'm putting the array in the shape tha like to structure setSelectionMdFile
        this.mdFileService.recursiveDeleteFileFromDataStore(this.data);
        this.mdFileService.setSelectionMdFile(folderContainer);
        this.dialogRef.close();
      });
    
  }
}
