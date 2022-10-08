import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
//import { MdFile } from '../../../models/md-file';
import { ServerMessagesService } from '../../services/server-messages.service'; //../../../compoments/services/monitor-md.service

@Component({
  selector: 'app-connection-lost',
  templateUrl: './connection-lost.component.html',
  styleUrls: ['./connection-lost.component.scss']
})
export class ConnectionLostComponent implements OnInit {
  //private _this: any;
  public _HideImg = true;
  public consoleIsClosed = false;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    //private monitorMDService: MonitorMDService,    
    private dialogRef: MatDialogRef<ConnectionLostComponent>){
    console.log('MAT_DIALOG_DATA = ' + data);
    if (data ==='serverIsDown') {
      this.consoleIsClosed = true;
    }
    dialogRef.disableClose = true;
  }

  ngOnInit(): void {
  }

  refresh() {
    //this.monitorMDService.startConnection();
    this.dialogRef.close();
  }
    


}
