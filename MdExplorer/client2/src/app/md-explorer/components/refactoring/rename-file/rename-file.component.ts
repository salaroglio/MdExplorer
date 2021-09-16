import { Component, OnInit } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { IRefactoringFilesystemEvent } from '../../../models/irefactoring-filesystem-event';
import { IRefactoringSourceAction } from '../../../models/irefactoring-source-action';
import { MdRefactoringService } from '../../../services/md-refactoring.service';

@Component({
  selector: 'app-rename-file',
  templateUrl: './rename-file.component.html',
  styleUrls: ['./rename-file.component.scss']
})
export class RenameFileComponent implements OnInit {

  constructor(private mdRefactoringService: MdRefactoringService,
    private dialogRef: MatDialogRef<RenameFileComponent>,  ) { }
  public actionList: IRefactoringSourceAction[];

  ngOnInit(): void {
    this.mdRefactoringService.getRefactoringSourceActionList().subscribe(data => {
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
