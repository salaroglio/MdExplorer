import { Component, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Inject } from '@angular/core';
import { MdRefactoringService } from '../../services/md-refactoring.service';
import { MdFileService } from '../../services/md-file.service';
import { MdFile } from '../../models/md-file';


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
    debugger;
    this.refactoringService.renameFileName(this.data)
      .subscribe(data => {
        var oldPath = data.relativePath + '/' + data.oldName;
        var newPath = data.relativePath + '/' + data.newName;
        var oldFile = new MdFile(data.oldName, data.oldPath,data.oldLevel,false);
        var newFile = new MdFile(data.newName, data.newPath, data.newLevel, false);
        this.mdFileService.changeDataStoreMdFiles(oldFile, newFile);
        this.dialogRef.close(null);
      });
  }

  closeDialog(data: any) {
    this.dialogRef.close(null);
  }

  dismiss() {
    this.dialogRef.close(null);
  }

}
