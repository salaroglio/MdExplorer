import { Component, OnDestroy, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { MdProject } from '../md-explorer/models/md-project';
import { MdFileService } from '../md-explorer/services/md-file.service';
import { MdServerMessagesService } from '../signalR/services/server-messages.service';
import { ProjectsService } from '../md-explorer/services/projects.service';
import { NewProjectComponent } from './new-project/new-project.component';
import { ShowFileSystemComponent } from '../commons/components/show-file-system/show-file-system.component';
import { CloneProjectComponent } from './dialogs/clone-project/clone-project.component';
import { NgDialogAnimationService } from '../shared/NgDialogAnimationService';
import { SettingsComponent } from '../md-explorer/components/dialogs/settings/settings.component';
import { ShowFileMetadata } from '../commons/components/show-file-system/show-file-metadata';
import { versionInfo } from '../../environments/version'; // Importa la versione

@Component({
  selector: 'app-projects',
  templateUrl: './projects.component.html',
  styleUrls: ['./projects.component.scss']
})
export class ProjectsComponent implements OnInit, OnDestroy {

  public appVersion = versionInfo.version; // Rendi la versione disponibile nel template
  public buildTime = versionInfo.buildTime; // Rendi il timestamp di build disponibile nel template

  constructor(private projectService: ProjectsService,
    public dialog: MatDialog,
    private router: Router,
    private signalRService: MdServerMessagesService,
    private dialogAn: NgDialogAnimationService,
  ) { }

    ngOnDestroy(): void {
      console.log('ProjectsComponent destroyed!');
    }

  public dataSource: Observable<MdProject[]>
  public dataSource1 = [{ name: 'Nome progetto', path: 'c:\folder\folder\folder' }]

  ngOnInit(): void {
    this.projectService.currentProjects$.subscribe(_ => {
      if (_ != null && _!= undefined) {
        this.router.navigate(['/main/navigation/document']); //main
      }
    });
  }


  openRecent(): void {
    this.router.navigate(['/projects/openrecent']);
  }

  prepareToClone(): void {
    const dialogRef = this.dialog.open(CloneProjectComponent, {
      width: '600px',
      maxHeight: '600px',
      data: null
    });
  }

  openNewFolder(): void {
    let data = new ShowFileMetadata();
    data.start = null;
    data.title = "Select project folder";
    data.typeOfSelection = "Folders";
    data.buttonText = "Create project here"; // Testo personalizzato

    const dialogRef = this.dialog.open(ShowFileSystemComponent, {
      width: '800px',
      height: '600px',
      panelClass: 'resizable-dialog-container',
      data: data
    });

    dialogRef.afterClosed().subscribe(folderPath => {
      this.projectService.setNewFolderProject(folderPath.data)
      // when the project change, then switch to navigation environment

    });

  }

  openSettings(): void {
    const dialogRef = this.dialogAn.open(SettingsComponent, {
      width: '600px',
      animation: {},
    });
  }
}
