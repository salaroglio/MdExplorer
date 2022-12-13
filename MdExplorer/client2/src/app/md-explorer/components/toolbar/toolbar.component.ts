import { HttpClient } from '@angular/common/http';
import { AfterViewInit, Component, EventEmitter, OnInit, Output, ViewChild } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ServerMessagesService } from '../../../signalr/services/server-messages.service';
import { SettingsComponent } from '../dialogs/settings/settings.component';
import { RenameFileComponent } from '../refactoring/rename-file/rename-file.component';
import { MdFileService } from '../../services/md-file.service';
import { RulesComponent } from '../../../signalR/dialogs/rules/rules.component';
import { MdFile } from '../../models/md-file';
import { GITService } from '../../../git/services/gitservice.service';
import { AppCurrentMetadataService } from '../../../services/app-current-metadata.service';
import { MatMenuTrigger } from '@angular/material/menu';
import { IBranch } from '../../../git/models/branch';
import { MatTabGroup } from '@angular/material/tabs';
import { ITag } from '../../../git/models/Tag';
import { ProjectsService } from '../../services/projects.service';
import { Router } from '@angular/router';
import { NgDialogAnimationService } from "../../../shared/NgDialogAnimationService";
import { INCOMING_ROTATE_OPTION, OUTGOING_ROTATE_OPTION } from '../../../shared/dialogAnimations';



@Component({
  selector: 'app-toolbar',
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.scss'],  
})
export class ToolbarComponent implements OnInit {

  public currentBranch: string;
  @ViewChild(MatMenuTrigger) matMenuTrigger: MatMenuTrigger;
  @ViewChild(MatTabGroup) tabGroup: MatTabGroup;
  TitleToShow: string;
  absolutePath: string;
  relativePath: string;
  connectionId: string;
  somethingIsChangedInTheBranch: boolean;
  howManyFilesAreToCommit: number;
  branches: IBranch[];
  taglist: ITag[];

  //@Output() toggleSidenav = new EventEmitter<void>();
  constructor(
    public dialog: MatDialog,
    private dialogAn: NgDialogAnimationService,
    private monitorMDService: ServerMessagesService,
    private http: HttpClient,
    private _snackBar: MatSnackBar,
    public mdFileService: MdFileService,
    private gitservice: GITService,
    private appSettings: AppCurrentMetadataService,
    private projectService: ProjectsService,
    private router: Router

  ) {
    this.TitleToShow = "MdExplorer";
  }

  ngOnInit(): void {
    this.monitorMDService.addMdProcessedListener(this.markdownFileIsProcessed, this);
    this.monitorMDService.addPdfIsReadyListener(this.showPdfIsready, this); //TODO: da spostare in SignalR
    this.monitorMDService.addMdRule1Listener(this.showRule1IsBroken, this);//TODO: da spostare in SignalR
    // get current branch name and if the branch has something to commit
    this.gitservice.getCurrentBranch().subscribe(branch => {
      this.currentBranch = branch.name;
      this.somethingIsChangedInTheBranch = branch.somethingIsChangedInTheBranch;
      this.howManyFilesAreToCommit = branch.howManyFilesAreChanged;
    });

    this.gitservice.getBranchList().subscribe(branches => {
      this.branches = branches;
    });

    // something is selected from treeview/sidenav
    this.mdFileService.selectedMdFileFromSideNav.subscribe(_ => {
      if (_ != null) {
        this.mdFileService.navigationArray = [];
        this.absolutePath = _.fullPath;
        this.relativePath = _.relativePath;
      }
    });
    // something has changed on filesystem
    this.mdFileService.serverSelectedMdFile.subscribe(val => {
      var current = val[0];

      if (current != undefined) {
        let index = this.mdFileService.navigationArray.length;
        if (index > 0) {
          //if (current.fullPath == this.mdFileService.navigationArray[index - 1].fullPath) {
          if (current == this.mdFileService.navigationArray[index - 1]) {
            //return;
          }
        }
        this.absolutePath = current.fullPath;
        this.relativePath = current.relativePath;
      }

    });
  }


  toggleSidenav() {
    let test = !this.appSettings.showSidenav.value;
    this.appSettings.showSidenav.next(test);
  }

  openDialog(): void {
    const dialogRef = this.dialogAn.open(SettingsComponent, {
      width: '600px',
      animation: {},
      //  incomingOptions: {
      //    keyframes: [
      //      { transform: "rotate(360deg)" },
      //      { transform: "rotate(0)" }
      //    ],
      //    keyframeAnimationOptions: { easing: "ease-in-out", duration: 500 }
      //  },
      //  outgoingOptions: {
      //    keyframes: [
      //      { transform: "rotate(0)" },
      //      { transform: "rotate(360deg)" }
      //    ],
      //    keyframeAnimationOptions: { easing: "ease-in-out", duration: 500 }
      //  }
      //},
    }
    );

    dialogRef.afterClosed().subscribe(result => {
      console.log('The dialog was closed');
    });
  }

  openRules(data: any): void {
    const dialogRef = this.dialog.open(RulesComponent, {
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
    let snackRef = objectThis._snackBar.open("seconds: " + data.executionTimeInSeconds, "Open folder", { duration: 5000, verticalPosition: 'top' });
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
      .subscribe(data => { console.log(data) });
  }

  Export() {
    this._snackBar.open("Export request queued!", null, { duration: 2000, verticalPosition: 'top' });
    this.monitorMDService.getConnectionId(this.sendExportRequest, this);
  }

  backToDocument(doc: MdFile) {
    var thatFile = this.mdFileService.navigationArray.find(_ => _.fullPath == doc.fullPath);
    if (thatFile != null && thatFile != undefined) {
      let i = this.mdFileService.navigationArray.length;
      let toDelete = this.mdFileService.navigationArray[i - 1];
      do {
        this.mdFileService.navigationArray.pop();
        i = i - 1;
        toDelete = this.mdFileService.navigationArray[i - 1];
      } while (toDelete != undefined && toDelete.fullPath == thatFile.fullPath)
    }
    this.mdFileService.setSelectedMdFileFromToolbar(doc);
  }

  openBranch(branch: IBranch): void {
    this.gitservice.checkoutSelectedBranch(branch).subscribe(_ => {
      this.currentBranch = _.name;
      var mdFile = new MdFile("Welcome to MDExplorer", '/../welcome.html', 0, false);
      mdFile.relativePath = '/../../welcome.html';
      this.mdFileService.setSelectedMdFileFromSideNav(mdFile);
      this.projectService.setNewFolderProject(_.fullPath).subscribe(_ => {
        this.mdFileService.loadAll(null, null);
        this.router.navigate(['/main/navigation/document']);
      });
    });
    this.matMenuTrigger.closeMenu();
  }

}
