import { HttpClient } from '@angular/common/http';
import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
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
import { WaitingDialogService } from '../../../commons/waitingdialog/waiting-dialog.service';
import { WaitingDialogInfo } from '../../../commons/waitingdialog/waiting-dialog/models/WaitingDialogInfo';
import { GitAuthComponent } from '../../../git/components/git-auth/git-auth.component';
import { PullInfo } from '../../../git/models/pullInfo';
import { GitMessagesComponent } from '../../../git/components/git-messages/git-messages.component';
import { BookmarksService } from '../../services/bookmarks.service';
import { MdServerMessagesService } from '../../../signalR/services/server-messages.service';
import { Bookmark } from '../../services/Types/Bookmark';
import { MdNavigationService } from '../../services/md-navigation.service';
import { Subscription } from 'rxjs';



@Component({
  selector: 'app-toolbar',
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.scss'],
})
export class ToolbarComponent implements OnInit, OnDestroy {

  public currentBranch: string;
  @ViewChild(MatMenuTrigger) matMenuTrigger: MatMenuTrigger;
  @ViewChild(MatTabGroup) tabGroup: MatTabGroup;
  TitleToShow: string;
  absolutePath: string;
  relativePath: string;
  connectionId: string;
  somethingIsChangedInTheBranch: boolean;
  somethingIsToPull: boolean;
  howManyFilesAreToCommit: number;
  howManyFilesAreToPull: number;
  branches: IBranch[];
  taglist: ITag[];
  currentMdFile: MdFile
  public connectionIsActive: boolean = true;
  public isCheckingConnection: boolean = false;
  subscriptionserverSelectedMdFile: Subscription;

  //@Output() toggleSidenav = new EventEmitter<void>();
  constructor(
    public dialog: MatDialog,    
    private monitorMDService: MdServerMessagesService,
    private http: HttpClient,
    private _snackBar: MatSnackBar,
    public mdFileService: MdFileService,
    private gitservice: GITService,
    private appSettings: AppCurrentMetadataService,
    private projectService: ProjectsService,
    private router: Router,
    private waitingDialogService: WaitingDialogService,
    private bookmarksService: BookmarksService,
    private navService:MdNavigationService,

  ) {
    this.TitleToShow = "MdExplorer";
    this.connectionIsActive = true;
  }

  ngOnInit(): void {
    this.FullScreenToggle();
    this.monitorMDService.addMdProcessedListener(this.markdownFileIsProcessed, this);
    this.monitorMDService.addPdfIsReadyListener(this.showPdfIsready, this); //TODO: da spostare in SignalR
    this.monitorMDService.addMdRule1Listener(this.showRule1IsBroken, this);//TODO: da spostare in SignalR
    // get current branch name and if the branch has something to commit    
    this.gitservice.currentBranch$.subscribe(branch => {
      this.currentBranch = branch.name;
      this.somethingIsChangedInTheBranch = branch.somethingIsChangedInTheBranch;
      this.howManyFilesAreToCommit = branch.howManyFilesAreChanged;
      this.connectionIsActive = true;
    });

    this.gitservice.commmitsToPull$.subscribe(_ => {      
      this.somethingIsToPull = _.somethingIsToPull;
      this.howManyFilesAreToPull = _.howManyFilesAreToPull;
      this.connectionIsActive = _.connectionIsActive;
      this.isCheckingConnection = false;
    });
    this.checkConnection();

    // manage resize fullscreen
    document.onfullscreenchange = (event) => {

      if (document.fullscreenElement) {
        this.screenType = "close_fullscreen";
      } else {
        this.screenType = "fullscreen";
      }
    };

    this.gitservice.getBranchList().subscribe(branches => {
      this.branches = branches;
    });



    // something is selected from treeview/sidenav
    this.mdFileService.selectedMdFileFromSideNav.subscribe(_ => {
      if (_ != null) {
        this.currentMdFile = _;
        this.mdFileService.navigationArray = [];
        this.absolutePath = _.fullPath;
        this.relativePath = _.relativePath;
      }
    });
    // something has changed on filesystem
    this.subscriptionserverSelectedMdFile =this.mdFileService.serverSelectedMdFile.subscribe(val => {
      
      var current = val[0];
      if (current != undefined) {
        let index = this.mdFileService.navigationArray.length;
        if (index > 0) {
          //if (current.fullPath == this.mdFileService.navigationArray[index - 1].fullPath) {
          if (current == this.mdFileService.navigationArray[index - 1]) {
            //return;
          }
        }
        
        this.navService.setNewNavigation(current);
        this.absolutePath = current.fullPath;
        this.relativePath = current.relativePath;
        this.currentMdFile = current;
      }

    });
  }

  ngOnDestroy(): void {
    console.log("ngOnDestroy toolbar");
    this.subscriptionserverSelectedMdFile.unsubscribe();
  }

 

  toggleSidenav() {
    let test = !this.appSettings.showSidenav.value;
    this.appSettings.showSidenav.next(test);
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

  checkConnection(): void {
    this.isCheckingConnection = true;
    this.gitservice.getCurrentBranch();
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
    objectThis.currentMdFile = data;
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


  public screenType = "fullscreen";

  FullScreenToggle(): void {
    if (!document.fullscreenElement) {
      document.documentElement.requestFullscreen();
    } else {
      document.exitFullscreen();
    }

  }

  pull(): void {
    let info = new WaitingDialogInfo();
    info.message = "Please wait... Pulling branch"
    this.waitingDialogService.showMessageBox(info);
    let pullInfo = new PullInfo();
    this.gitservice.pull(pullInfo).subscribe(_ => {
      // manage failed connection
      if (_.isConnectionMissing) {
        const dialogRef = this.dialog.open(GitMessagesComponent, {
          width: '300px',
          data: {
            message: 'Missing connection',
            description: 'Please verify your vpn or network settings'
          }
        });
      }
      if (_.isAuthenticationMissing) {
        const dialogRef = this.dialog.open(GitAuthComponent, {
          width: '300px',
        });
      }
      if (_.thereAreConflicts) {
        const dialogRef = this.dialog.open(GitMessagesComponent, {
          width: '300px',
          data: {
            message: 'Conflicts appear',
            description: _.errorMessage
          }
        });
      }

      if (!_.isAuthenticationMissing && !_.isConnectionMissing && !_.thereAreConflicts) {
        this.mdFileService.loadAll(null, null);
        this.mdFileService.getLandingPage().subscribe(node => {
          if (node != null) {
            this.checkConnection();
            this.mdFileService.setSelectedMdFileFromSideNav(node);
          }
        });
      }
      this.waitingDialogService.closeMessageBox();
      this.matMenuTrigger.closeMenu();
    });
  }

  commit(): void {
    let info = new WaitingDialogInfo();
    info.message = "Please wait... commit and pushing branch";
    this.waitingDialogService.showMessageBox(info);
    let pullInfo = new PullInfo();
    this.gitservice.commit(pullInfo).subscribe(_ => {
      if (_.isConnectionMissing) {
        const dialogRef = this.dialog.open(GitMessagesComponent, {
          width: '300px',
          data: {
            message: 'Missing connection',
            description: 'Please verify your vpn or network settings'
          }
        });
      }
      if (_.isAuthenticationMissing) {
        const dialogRef = this.dialog.open(GitAuthComponent, {
          width: '300px',
        });
      }
      if (_.thereAreConflicts) {
        const dialogRef = this.dialog.open(GitMessagesComponent, {
          width: '300px',
          data: {
            message: 'Conflicts appear',
            description: _.errorMessage,
          }
        });
      }
      this.checkConnection();
      this.waitingDialogService.closeMessageBox();
      this.matMenuTrigger.closeMenu();
    });
  }

  push(): void {
    let info = new WaitingDialogInfo();
    info.message = "Please wait... commit and pushing branch";
    this.waitingDialogService.showMessageBox(info);
    let pullInfo = new PullInfo();
    this.gitservice.push(pullInfo).subscribe(_ => {
      if (_.isConnectionMissing) {
        const dialogRef = this.dialog.open(GitMessagesComponent, {
          width: '300px',
          data: {
            message: 'Missing connection',
            description: 'Please verify your vpn or network settings'
          }
        });
      }
      if (_.isAuthenticationMissing) {
        const dialogRef = this.dialog.open(GitAuthComponent, {
          width: '300px',
        });
      }
      if (_.thereAreConflicts) {
        const dialogRef = this.dialog.open(GitMessagesComponent, {
          width: '300px',
          data: {
            message: 'Conflicts appear',
            description: _.errorMessage,
          }
        });
      }
      this.checkConnection();
      this.waitingDialogService.closeMessageBox();
      this.matMenuTrigger.closeMenu();
    });
  }

  openBranch(branch: IBranch): void {
    this.gitservice.checkoutSelectedBranch(branch).subscribe(_ => {
      this.currentBranch = _.name;
      var mdFile = new MdFile("Welcome to MDExplorer", '/../welcome.html', 0, false);
      mdFile.relativePath = '/../../welcome.html';
      this.mdFileService.setSelectedMdFileFromSideNav(mdFile);
      this.projectService.setNewFolderProject(_.fullPath);
      
    });
    this.matMenuTrigger.closeMenu();
  }

  bookmarkToggle(): void {
    let bookmark: Bookmark = new Bookmark(this.currentMdFile);
    bookmark.projectId = this.projectService.currentProjects$.value.id;   
    this.bookmarksService.toggleBookmark(bookmark);
  }
}
