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

  @Output() toggleSidenav = new EventEmitter<void>();
  constructor(
    private sideNavDataService: SideNavDataService,
    public dialog: MatDialog,
    private monitorMDService: MonitorMDService,
    private http: HttpClient,
    private _snackBar: MatSnackBar,
    private router: Router,
    public mdFileService: MdFileService,
    
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
    this.monitorMDService.addMdProcessedListener(this.markdownFileIsProcessed, this);
    this.monitorMDService.addPdfIsReadyListener(this.showPdfIsready, this);    
    this.monitorMDService.addMdRule1Listener(this.showRule1IsBroken, this);
   

    this.mdFileService.selectedMdFileFromSideNav.subscribe(_ => {
      
      if (_ != null) {
        this.mdFileService.navigationArray = [];
        this.absolutePath = _.fullPath;
        this.relativePath = _.relativePath;
      }      
    });

    this.mdFileService.serverSelectedMdFile.subscribe(val => {      
      var current = val[0];
      if (current != undefined) {
        if (current.fullPath == this.mdFileService.navigationArray[0].fullPath) {
          return;
        }
        this.absolutePath = current.fullPath;
        this.relativePath = current.relativePath;
      }
      
    });
  }

  private showRule1IsBroken(data: any, objectThis: ToolbarComponent) {
    objectThis.openRules(data);
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

  private markdownFileIsProcessed(data: MdFile, objectThis: ToolbarComponent) {
    
      objectThis.mdFileService.navigationArray.push(data);
      objectThis.mdFileService.setSelectedMdFileFromServer(data);
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

  backToDocument(doc: MdFile) {
    var thatFile = this.mdFileService.navigationArray.find(_ => _.fullPath == doc.fullPath);
    if (thatFile != null && thatFile != undefined) {
      let i = this.mdFileService.navigationArray.length;
      let toDelete = this.mdFileService.navigationArray[i - 1];
      do  {
        this.mdFileService.navigationArray.pop();
        i = i - 1;
        toDelete = this.mdFileService.navigationArray[i - 1];
      } while (toDelete != undefined && toDelete.fullPath == thatFile.fullPath)
    }
    this.mdFileService.setSelectedMdFileFromToolbar(doc);
  }

}
