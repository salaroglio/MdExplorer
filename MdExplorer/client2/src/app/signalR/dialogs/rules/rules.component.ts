import { Component, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Inject } from '@angular/core';
import { MdRefactoringService } from '../../../md-explorer/services/md-refactoring.service';
import { MdFileService } from '../../../md-explorer/services/md-file.service';
import { MdFile } from '../../../md-explorer/models/md-file';


@Component({
  selector: 'app-rules',
  templateUrl: './rules.component.html',
  styleUrls: ['./rules.component.scss']
})
export class RulesComponent implements OnInit {

  constructor(
    private dialogRef: MatDialogRef<RulesComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private refactoringService: MdRefactoringService,
    private mdFileService: MdFileService,
  ) { }


  public message: string;
  public fromFileName: string;
  public toFileName: string;

  ngOnInit(): void {    
    this.message = this.data.message;
    this.fromFileName = this.data.fromFileName;
    this.toFileName = this.data.toFileName;
   
  }

  changeName(): void {    
    this.refactoringService.renameFileName(this.data)
      .subscribe(data => {
        var oldPath = data.relativePath + '\\' + data.oldName;
        var newPath = data.relativePath + '\\' + data.newName;        
        var oldFile = new MdFile(data.oldName, oldPath, data.oldLevel, false);
        oldFile.relativePath = oldPath;
        oldFile.fullPath = data.oldPath;
        var newFile = new MdFile(data.newName, newPath, data.newLevel, false);
        newFile.fullPath = data.newPath;
        newFile.relativePath = newPath;
        this.mdFileService.changeDataStoreMdFiles(oldFile, newFile);
        debugger;
        this.dialogRef.close(data);
      });
  }

  closeDialog(data: any) {
    this.dialogRef.close(null);
  }

  dismiss() {
    this.dialogRef.close(null);
  }

}
