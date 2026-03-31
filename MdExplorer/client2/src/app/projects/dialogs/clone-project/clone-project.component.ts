import { Component, OnInit } from '@angular/core';
import { MatLegacyDialog as MatDialog, MatLegacyDialogRef as MatDialogRef } from '@angular/material/legacy-dialog';
import { Router } from '@angular/router';
import { ShowFileSystemComponent } from '../../../commons/components/show-file-system/show-file-system.component';
import { WaitingDialogService } from '../../../commons/waitingdialog/waiting-dialog.service';
import { WaitingDialogInfo } from '../../../commons/waitingdialog/waiting-dialog/models/WaitingDialogInfo';
import { GitMessagesComponent } from '../../../git/components/git-messages/git-messages.component';
import { CloneInfo } from '../../../git/models/cloneRequest';
import { GITService } from '../../../git/services/gitservice.service';
import { MdFileService } from '../../../md-explorer/services/md-file.service';
import { ProjectsService } from '../../../md-explorer/services/projects.service';
import { ShowFileMetadata } from '../../../commons/components/show-file-system/show-file-metadata';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-clone-project',
  templateUrl: './clone-project.component.html',
  styleUrls: ['./clone-project.component.scss']
})
export class CloneProjectComponent implements OnInit {
  public hide = true;
  public urlpath: string;
  

  public dataForCloning: CloneInfo = {
    urlPath: null,
    directoryPath: null,
    username: null,
    password: null,
    email: null,
    storeCredentials: true
  };


  constructor(private dialog: MatDialog,
    private mdFileService: MdFileService,
    private gitService: GITService,
    private dialogRef: MatDialogRef<CloneProjectComponent>,    
    private waitingDialog: WaitingDialogService,
    private projectService: ProjectsService,
    private router:Router,
    private translate: TranslateService
  ) { }

  ngOnInit(): void {
    this.mdFileService.getTextFromClipboard().subscribe(_ => {
      this.dataForCloning.urlPath = _.url;
    });
    // when the project change, then switch to navigation environment
    this.projectService.currentProjects$.subscribe(_ => {
      if (_!=null && _!=undefined) {
        this.router.navigate(['/main/navigation/document']); //main
        this.dialogRef.close();
      }
    });
  }

  cloneDirectory(): void {
    let info = new WaitingDialogInfo();
    info.message = this.translate.instant('CLONE.CLONING_MSG');
    this.waitingDialog.showMessageBox(info);
    this.gitService.clone(this.dataForCloning).subscribe(_ => {
      if (_.areCredentialsCorrect) {
        this.projectService.setNewFolderProject(this.dataForCloning.directoryPath);
        
      } else {
        const dialogRef = this.dialog.open(GitMessagesComponent, {
          width: '300px',          
          data: {message: this.translate.instant('CLONE.CREDENTIALS_WRONG')}
        });
      }
      this.waitingDialog.closeMessageBox();
      

      this.dialogRef.close(this.dataForCloning);
    });
  }

  openFileSystem() {
    let data = new ShowFileMetadata();
    data.start = null;
    data.title = this.translate.instant('CLONE.FOLDER_TITLE');
    data.typeOfSelection = "Folders";

    const dialogRef = this.dialog.open(ShowFileSystemComponent, {
      width: '800px',
      height: '600px',
      panelClass: 'resizable-dialog-container',
      data: data
    });

    dialogRef.afterClosed().subscribe(_ => {

      this.dataForCloning.directoryPath = _.data;
    });

  }
}
