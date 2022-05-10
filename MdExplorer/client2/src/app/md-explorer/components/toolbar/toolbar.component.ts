import { HttpClient } from '@angular/common/http';
import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { MonitorMDService } from '../../services/monitor-md.service';
import { SideNavDataService } from '../../services/side-nav-data.service';
import { SettingsComponent } from '../settings/settings.component';
import { RenameFileComponent } from '../refactoring/rename-file/rename-file.component';
import { MdFileService } from '../../services/md-file.service';
import { RulesComponent } from '../rules/rules.component';
import { MdRefactoringService } from '../../services/md-refactoring.service';
import { MdFile } from '../../models/md-file';

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
  navigationArray: MdFile[] = [];

  @Output() toggleSidenav = new EventEmitter<void>();
  constructor(
    private sideNavDataService: SideNavDataService,
    public dialog: MatDialog,
    private monitorMDService: MonitorMDService,
    private http: HttpClient,
    private _snackBar: MatSnackBar,
    private router: Router,
    private mdFileService: MdFileService,
    
  ) {
    this.TitleToShow = "MdExplorer";
  }
  private plantumlServer: string;

  openRefactoring(): void {
    const dialogRef = this.dialog.open(RenameFileComponent, {
      width: '600px',
    });
  }

  openDialog(): void {
    const dialogRef = this.dialog.open(SettingsComponent, {
      width: '300px',
      
    });
    
    dialogRef.afterClosed().subscribe(result => {
      console.log('The dialog was closed');
      
    });
  }

  openRules(data:any): void {
    const dialogRef = this.dialog.open(RulesComponent, {
      width: '600px',
      data: data
    });    
  }


  ngOnInit(): void {    
    this.monitorMDService.addMdProcessedListener(this.updateModifiedMarkDown, this);
    //this.monitorMDService.addMarkdownFileListener(this.markDownIsChanged, this);
    this.monitorMDService.addPdfIsReadyListener(this.showPdfIsready, this);
    this.monitorMDService.addRefactoringFileEvent(this.openDialogRefactoringFileEvent, this);
    this.monitorMDService.addMdRule1Listener(this.showRule1IsBroken, this);

    this.mdFileService.serverSelectedMdFile.subscribe(val => {
      debugger;
      var current = val[0];
      if (current != undefined) {
        if (this.sideNavDataService.currentPath == current.path) {
          this.navigationArray = [];
          this.navigationArray.push(current);
        } else {
          this.navigationArray.push(current);
        }
        
        this.absolutePath = current.fullPath;
        this.relativePath = current.relativePath;
      }
      
    });
  }



  private showRule1IsBroken(data: any, objectThis: ToolbarComponent) {
    objectThis.openRules(data);
  }

  private openDialogRefactoringFileEvent(data, objectThis: ToolbarComponent) {
    objectThis.mdFileService.loadAll(null,null);
    objectThis.openRefactoring();    
  }

  private sendExportRequest(data, objectThis: ToolbarComponent) {
    const url = '../api/mdexport/' + objectThis.relativePath + '?connectionId=' + data;    
    return objectThis.http.get(url)
      .subscribe(data => { console.log(data) });
  }

  //private sendExporEmailRequest(data, objectThis: ToolbarComponent) {    
  //  const url = '../api/mdexportemail/' + objectThis.relativePath + '?connectionId=' + data;
  //  return objectThis.http.get(url)
  //    .subscribe(data => { console.log(data) });
  //}

  private showPdfIsready(data: any, objectThis: ToolbarComponent) {
    let snackRef = objectThis._snackBar.open("seconds: " + data.executionTimeInSeconds , "Open folder", { duration: 5000, verticalPosition: 'top' });
    snackRef.onAction().subscribe(() => {
      const url = '../api/AppSettings/OpenChromePdf?path=' + data.path;
      return objectThis.http.get(url)
        .subscribe(data => { console.log(data) });
    });
  }

  private updateModifiedMarkDown(data: any, objectThis: ToolbarComponent) {
    debugger;
    objectThis.mdFileService.setNewSelectedMdFile(data);
    //objectThis.TitleToShow = data.name;
    //objectThis.absolutePath = data.path;
    //objectThis.relativePath = data.relativePath;
  }

  //private markDownIsChanged(data: any, objectThis: any) {
    
  //  objectThis.TitleToShow = data.name;
  //  objectThis.absolutePath = data.path;
  //  objectThis.relativePath = data.relativePath;
  //}

  OpenEditor() {    
    const url = '../api/AppSettings/OpenFile?path=' + this.absolutePath;
    return this.http.get(url)
      .subscribe(data => { console.log(data)});
  }

  Export() {
    this._snackBar.open("Export request queued!",null ,{ duration: 2000, verticalPosition: 'top' });
    this.monitorMDService.getConnectionId(this.sendExportRequest,this);   
  }

  backToDocument(doc: MdFile) {

    this.mdFileService.setNewSelectedMdFile(doc);
  }

  //ExportEmail() {
  //  this._snackBar.open("Email queued!", null, { duration: 2000, verticalPosition: 'top' });
  //  this.monitorMDService.getConnectionId(this.sendExporEmailRequest, this);
  //}
   
}
