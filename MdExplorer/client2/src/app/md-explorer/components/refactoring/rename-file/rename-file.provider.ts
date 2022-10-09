import { Injectable } from "@angular/core";
import { MatDialog, MatDialogRef } from "@angular/material/dialog";
import { RenameFileComponent } from "./rename-file.component";

@Injectable()
export class RenameFileProvider {

  private _dialogRef: MatDialogRef<RenameFileComponent>;

  constructor(
    private dialog: MatDialog) {
  }

  show(data: any): RenameFileProvider {
    const dialogRef = this.dialog.open(RenameFileComponent, {
      width: '600px',
    });
    return this;
  }

  hide(data: any): void {
    this._dialogRef.close();
  }

}
