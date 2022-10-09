import { Injectable } from "@angular/core";
import { MatDialog, MatDialogRef } from "@angular/material/dialog";
import { RenameFileComponent } from "../../../md-explorer/components/refactoring/rename-file/rename-file.component";
import { GoodRulesComponent } from "./good-rules.component";

@Injectable()
export class GoodRulesProvider {

  private _dialogRef: MatDialogRef<GoodRulesComponent>;

  constructor(
    private dialog: MatDialog) {
  }

  show(data: any): GoodRulesProvider {
    const dialogRef = this.dialog.open(GoodRulesComponent, {
      width: '600px',
      data: data
    });
    dialogRef.afterClosed().subscribe(_ => {
      if (_.refactoringSourceActionId != undefined) {
        this.dialog.open(RenameFileComponent, {
          width: '600px',
          data: _
        });
      }
    });

    //this._dialogRef = this.dialog.open(RulesComponent, {
    //  data: data
    //});
    return this;
  }

  hide(data: any): void {
    this._dialogRef.close();
  }

}
