import { Component, OnInit } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { ShowFileSystemComponent } from '../../../commons/components/show-file-system/show-file-system.component';
import { WaitingDialogService } from '../../../commons/waitingdialog/waiting-dialog.service';
import { WaitingDialogInfo } from '../../../commons/waitingdialog/waiting-dialog/models/WaitingDialogInfo';
import { CloneInfo } from '../../../git/models/cloneRequest';
import { GITService } from '../../../git/services/gitservice.service';
import { MdFileService } from '../../../md-explorer/services/md-file.service';
import { ProjectsService } from '../../../md-explorer/services/projects.service';

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
    private router:Router
  ) { }

  ngOnInit(): void {
    this.mdFileService.getTextFromClipboard().subscribe(_ => {
      this.dataForCloning.urlPath = _.url;
    });
  }

  cloneDirectory(): void {
    let info = new WaitingDialogInfo();
    info.message = "Oh MY GOD... Cloning Repository!"
    this.waitingDialog.showMessageBox(info);
    this.gitService.clone(this.dataForCloning).subscribe(_ => {
      this.waitingDialog.closeMessageBox();
      this.projectService.setNewFolderProject(this.dataForCloning.directoryPath).subscribe(_ => {
        this.router.navigate(['/main/navigation/document']); //main
        this.dialogRef.close();
      });

      this.dialogRef.close(this.dataForCloning);
    });
  }

  openFileSystem() {
    const dialogRef = this.dialog.open(ShowFileSystemComponent, {
      width: '600px',
      height: '600px',
      data: null
    });

    dialogRef.afterClosed().subscribe(_ => {

      this.dataForCloning.directoryPath = _.data;
    });

  }
}
