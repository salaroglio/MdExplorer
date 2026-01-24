import { Injectable } from "@angular/core";
import { MatLegacyDialog as MatDialog, MatLegacyDialogRef as MatDialogRef } from "@angular/material/legacy-dialog";
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
