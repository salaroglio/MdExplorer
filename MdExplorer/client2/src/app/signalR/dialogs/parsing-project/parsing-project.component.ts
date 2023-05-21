import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'app-parsing-project',
  templateUrl: './parsing-project.component.html',
  styleUrls: ['./parsing-project.component.scss']
})
export class ParsingProjectComponent implements OnInit {

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    private dialogRef: MatDialogRef<ParsingProjectComponent>
  ) {
    dialogRef.disableClose = true;
}

  ngOnInit(): void {
  }

}
