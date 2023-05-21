import { Injectable } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { WaitingDialogInfo } from './waiting-dialog/models/WaitingDialogInfo';
import { WaitingDialogComponent } from './waiting-dialog/waiting-dialog.component';

@Injectable({
  providedIn: 'root'
})
export class WaitingDialogService {

  private dialogRef!: MatDialogRef<WaitingDialogComponent>;
  constructor(private dialog: MatDialog,

  ) { }

  showMessageBox(info: WaitingDialogInfo): void {
    this.dialogRef = this.dialog.open(WaitingDialogComponent, {
      width: '300px',
      height: '300px',
      data: info    
    });
  }

  closeMessageBox(): void {
    this.dialogRef.close();
  }
}
