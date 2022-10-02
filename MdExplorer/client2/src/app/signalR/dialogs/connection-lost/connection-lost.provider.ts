import { Injectable } from "@angular/core";
import { MatDialog, MatDialogRef } from "@angular/material/dialog";
import { MonitorMDService } from "../../../md-explorer/services/monitor-md.service";
import { ConnectionLostComponent } from "./connection-lost.component";


@Injectable()
export class ConnectionLostProvider {

  private _dialogRef: MatDialogRef<ConnectionLostComponent>;

  constructor(
    private dialog: MatDialog) {
  }

  show(hub: MonitorMDService): ConnectionLostProvider {
    this._dialogRef = this.dialog.open(ConnectionLostComponent, {
      data: null
    });
    this._dialogRef.afterClosed().subscribe(_ => {
      hub.startConnection();
    });
    return this;
  }

  hide(data: any): void {
    this._dialogRef.close();
  }

}
