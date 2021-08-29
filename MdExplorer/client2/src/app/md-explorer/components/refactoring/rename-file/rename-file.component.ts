import { Component, OnInit } from '@angular/core';
import { IRefactoringFilesystemEvent } from '../../../models/irefactoring-filesystem-event';
import { MdRefactoringService } from '../../../services/md-refactoring.service';

@Component({
  selector: 'app-rename-file',
  templateUrl: './rename-file.component.html',
  styleUrls: ['./rename-file.component.scss']
})
export class RenameFileComponent implements OnInit {

  constructor(private mdRefactoringService:MdRefactoringService) { }
  public eventList: IRefactoringFilesystemEvent[];

  ngOnInit(): void {
    this.mdRefactoringService.getFileEventList().subscribe(data => {
      this.eventList = data;
    },
      error => {
        console.log("failed to fetch GetRefactoringFileEvent List");
      });;
  }

}
