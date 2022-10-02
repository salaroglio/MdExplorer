import { Injectable } from "@angular/core";
import { MatDialog, MatDialogRef } from "@angular/material/dialog";
import { ParsingProjectComponent } from "./parsing-project.component";

@Injectable()
export class ParsingProjectProvider {

  private _dialogRef: MatDialogRef<ParsingProjectComponent>;

  constructor(
              private dialog: MatDialog) {
  }

  show(data:any): ParsingProjectProvider {
    this._dialogRef = this.dialog.open(ParsingProjectComponent, {
      data: data
    });
    return this;
  }

  hide(data: any): void {
    this._dialogRef.close();
  }

}
