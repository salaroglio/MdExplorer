import { Injectable } from "@angular/core";
import { MatDialog, MatDialogRef } from "@angular/material/dialog";
import { ParsingProjectComponent } from "./parsing-project.component";
import { BehaviorSubject } from "rxjs";

@Injectable()
export class ParsingProjectProvider {

  private _dialogRef: MatDialogRef<ParsingProjectComponent>;
  public folder$ = new BehaviorSubject<string>("Processing");

  constructor(
              private dialog: MatDialog) {
  }

  show(data:any): ParsingProjectProvider {
    this._dialogRef = this.dialog.open(ParsingProjectComponent, {
      data: { data : data, folder$:this.folder$ }
    });
    return this;
  }

  hide(data: any): void {
    this._dialogRef.close();
  }

}
