import { HttpClient } from '@angular/common/http';
import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { MonitorMDService } from '../../services/monitor-md.service';
import { SideNavDataService } from '../../services/side-nav-data.service';
import { SettingsComponent } from '../settings/settings.component';
import { RenameFileComponent } from '../refactoring/rename-file/rename-file.component';

@Component({
  selector: 'app-toolbar',
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.scss']
})
export class ToolbarComponent implements OnInit {
  TitleToShow: string;
  absolutePath: string;
  relativePath: string;
  connectionId: string;

  @Output() toggleSidenav = new EventEmitter<void>();
  constructor(
    private sideNavDataService: SideNavDataService,
    public dialog: MatDialog,
    private monitorMDService: MonitorMDService,
    private http: HttpClient,
    private _snackBar: MatSnackBar,
    private router: Router,
  ) {
    this.TitleToShow = "M↓Explorer";
  }
  private plantumlServer: string;

  openRefactoring(): void {
    const dialogRef = this.dialog.open(RenameFileComponent, {
      width: '300px',
      data: { name: this.plantumlServer }
    });
  }

  openDialog(): void {
    const dialogRef = this.dialog.open(SettingsComponent, {
      width: '300px',
      data: { name: this.plantumlServer }
    });
    

    dialogRef.afterClosed().subscribe(result => {
      console.log('The dialog was closed');
      this.plantumlServer = result;
    });
  }
  

  ngOnInit(): void {
    this.monitorMDService.addMdProcessedListener(this.updateModifiedMarkDown, this);
    this.monitorMDService.addPdfIsReadyListener(this.showPdfIsready, this);
    this.monitorMDService.addRefactoringFileEvent(this.openDialogRefactoringFileEvent, this);
  }

  private openDialogRefactoringFileEvent(data, objectThis: ToolbarComponent) {
    objectThis.openRefactoring();    
  }

  private sendExportRequest(data, objectThis: ToolbarComponent) {
    const url = '../api/mdexport/' + objectThis.relativePath + '?connectionId=' + data;    
    return objectThis.http.get(url)
      .subscribe(data => { console.log(data) });
  }

  private showPdfIsready(data: any, objectThis: ToolbarComponent) {
    let snackRef = objectThis._snackBar.open("seconds: " + data.executionTimeInSeconds , "Open folder", { duration: 5000, verticalPosition: 'top' });
    snackRef.onAction().subscribe(() => {
      const url = '../api/AppSettings/OpenChromePdf?path=' + data.path;
      return objectThis.http.get(url)
        .subscribe(data => { console.log(data) });
    });
  }

  private updateModifiedMarkDown(data: any, objectThis: any) {
    console.log(data);    
    objectThis.TitleToShow = data.name;
    objectThis.absolutePath = data.path;
    objectThis.relativePath = data.relativePath;
  }

  OpenEditor() {    
    const url = '../api/AppSettings/OpenFile?path=' + this.absolutePath;
    return this.http.get(url)
      .subscribe(data => { console.log(data)});
  }

  Export() {
    this._snackBar.open("Export request queued!",null ,{ duration: 2000, verticalPosition: 'top' });
    this.monitorMDService.getConnectionId(this.sendExportRequest,this);   
  }
   
}
