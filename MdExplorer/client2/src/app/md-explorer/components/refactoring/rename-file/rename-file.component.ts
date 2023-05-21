import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { IRefactoringSourceAction } from '../../../models/irefactoring-source-action';
import { MdRefactoringService } from '../../../services/md-refactoring.service';

@Component({
  selector: 'app-rename-file',
  templateUrl: './rename-file.component.html',
  styleUrls: ['./rename-file.component.scss']
})
export class RenameFileComponent implements OnInit {

  constructor(private mdRefactoringService: MdRefactoringService,
    private dialogRef: MatDialogRef<RenameFileComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,  ) {
  }
  public actionList: IRefactoringSourceAction[];
  public displayedColumns = ['suggestedAction', 'fileName', 'oldLinkStored', 'newLinkToReplace'];

  ngOnInit(): void {    
    this.mdRefactoringService.getRefactoringSourceActionList(this.data.refactoringSourceActionId).subscribe(data => {      
      this.actionList = data;
    },
      error => {
        console.log("failed to fetch GetRefactoringFileEvent List");
      });;
  }

  doRefactoring() {

    this.dialogRef.close(null);
  }
}
