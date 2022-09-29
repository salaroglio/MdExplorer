import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MdFile } from '../../../models/md-file';
import { MonitorMDService } from '../../../services/monitor-md.service';

@Component({
  selector: 'app-connection-lost',
  templateUrl: './connection-lost.component.html',
  styleUrls: ['./connection-lost.component.scss']
})
export class ConnectionLostComponent implements OnInit {
  //private _this: any;
  public _HideImg = true;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: MdFile,
    private monitorMDService: MonitorMDService,    
    private dialogRef: MatDialogRef<ConnectionLostComponent>,
     
  ) {
    
  }

  ngOnInit(): void {
  }

  refresh() {
    this.monitorMDService.startConnection();
    this.dialogRef.close();
  }
    


}
