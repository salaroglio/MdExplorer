import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { WaitingDialogInfo } from './models/WaitingDialogInfo';

@Component({
  selector: 'app-waiting-dialog',
  templateUrl: './waiting-dialog.component.html',
  styleUrls: ['./waiting-dialog.component.scss']
})
export class WaitingDialogComponent implements OnInit {

  public message: string = "<todo>";
 

  constructor(private dialogRef: MatDialogRef<WaitingDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: WaitingDialogInfo,) {
    dialogRef.disableClose = true;
    this.message = this.data.message;
  }

  ngOnInit(): void {
  }

}
