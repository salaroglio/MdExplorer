import { Injectable } from "@angular/core";
import { MatDialog, MatDialogRef } from "@angular/material/dialog";
import { PlantumlWorkingComponent } from "./plantuml-working.component";

@Injectable()
export class PlantumlWorkingProvider {

  private _dialogRef: MatDialogRef<PlantumlWorkingComponent>;

  constructor(
    private dialog: MatDialog) {
  }

  show(data: any): PlantumlWorkingProvider {
    this._dialogRef = this.dialog.open(PlantumlWorkingComponent, {
      data: data
    });
    return this;
  }

  hide(data: any): void {
    this._dialogRef.close();
  }

}
