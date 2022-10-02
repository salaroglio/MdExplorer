import { Injectable } from "@angular/core";
import { MatDialog, MatDialogRef } from "@angular/material/dialog";
import { ConnectionLostComponent } from "./connection-lost.component";


@Injectable()
export class ConnectionLostProvider {

  private _dialogRef: MatDialogRef<ConnectionLostComponent>;

  constructor(
    private dialog: MatDialog) {
  }

  show(data: any): ConnectionLostProvider {
    this._dialogRef = this.dialog.open(ConnectionLostComponent, {
      data: data
    });
    return this;
  }

  hide(data: any): void {
    this._dialogRef.close();
  }

}
