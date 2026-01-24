import { Injectable } from "@angular/core";
import { MatLegacyDialog as MatDialog, MatLegacyDialogRef as MatDialogRef } from "@angular/material/legacy-dialog";
import { MdServerMessagesService } from "../../services/server-messages.service";
import { ConnectionLostComponent } from "./connection-lost.component";


@Injectable()
export class ConnectionLostProvider {

  private _dialogRef: MatDialogRef<ConnectionLostComponent>;

  constructor(
    private dialog: MatDialog) {
  }

  show(hub: MdServerMessagesService): ConnectionLostProvider {
    this._dialogRef = this.dialog.open(ConnectionLostComponent, {
      data: null
    });
    this._dialogRef.afterClosed().subscribe(_ => {
      hub.startConnection();
    });
    return this;
  }
  showConsoleClosed(): void {
    console.log('showConsoleClosed')
    this._dialogRef = this.dialog.open(ConnectionLostComponent, {
      data: 'serverIsDown'
    });
  }

  hide(data: any): void {
    this._dialogRef.close();
  }

}
