import { Injectable } from "@angular/core";
import { MatLegacyDialog as MatDialog, MatLegacyDialogRef as MatDialogRef } from "@angular/material/legacy-dialog";
import { OpeningApplicationComponent } from "./opening-application.component";

@Injectable()
export class OpeningApplicationProvider {

  private _dialogRef: MatDialogRef<OpeningApplicationComponent>;

  constructor(
    private dialog: MatDialog) {
  }

  show(data: any): OpeningApplicationProvider {
    this._dialogRef = this.dialog.open(OpeningApplicationComponent, {
      data: data
    });
    return this;
  }

  hide(data: any): void {
    this._dialogRef.close();
  }

}
