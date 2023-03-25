import { Component, OnDestroy, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { MdProject } from '../md-explorer/models/md-project';
import { MdFileService } from '../md-explorer/services/md-file.service';
import { ServerMessagesService } from '../signalr/services/server-messages.service';
import { ProjectsService } from '../md-explorer/services/projects.service';
import { NewProjectComponent } from './new-project/new-project.component';
import { ShowFileSystemComponent } from '../commons/components/show-file-system/show-file-system.component';
import { CloneProjectComponent } from './dialogs/clone-project/clone-project.component';
import { NgDialogAnimationService } from '../shared/NgDialogAnimationService';
import { SettingsComponent } from '../md-explorer/components/dialogs/settings/settings.component';

@Component({
  selector: 'app-projects',
  templateUrl: './projects.component.html',
  styleUrls: ['./projects.component.scss']
})
export class ProjectsComponent implements OnInit, OnDestroy {

  constructor(private projectService: ProjectsService,
    public dialog: MatDialog,
    private router: Router,
    private signalRService: ServerMessagesService,
    private dialogAn: NgDialogAnimationService,
  ) { }

    ngOnDestroy(): void {
      console.log('ProjectsComponent destroyed!');
    }

  public dataSource: Observable<MdProject[]>
  public dataSource1 = [{ name: 'Nome progetto', path: 'c:\folder\folder\folder' }]

  ngOnInit(): void {
  }


  openRecent(): void {
    this.router.navigate(['/projects/openrecent']);
  }

  prepareToClone(): void {
    const dialogRef = this.dialog.open(CloneProjectComponent, {
      width: '600px',
      height: '600px',
      data: null
    });
  }

  openNewFolder(): void {
    const dialogRef = this.dialog.open(ShowFileSystemComponent, {
      width: '600px',
      height: '600px',
      data: null
    });

    dialogRef.afterClosed().subscribe(folderPath => {      
      this.projectService.setNewFolderProject(folderPath.data).subscribe(_ => {        
        this.router.navigate(['/main/navigation/document']); //main        
      });
    });

  }

  openSettings(): void {
    const dialogRef = this.dialogAn.open(SettingsComponent, {
      width: '600px',
      animation: {},
    });
  }
}
