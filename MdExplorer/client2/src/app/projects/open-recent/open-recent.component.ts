import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { bounceInLeftOnEnterAnimation } from 'angular-animations';
import { Observable } from 'rxjs';
import { MdProject } from '../../md-explorer/models/md-project';
import { MdFileService } from '../../md-explorer/services/md-file.service';
import { ProjectsService } from '../../md-explorer/services/projects.service';
import { ProjectSettingsComponent } from '../project-settings/project-settings.component';

@Component({
  selector: 'app-open-recent',
  templateUrl: './open-recent.component.html',
  styleUrls: ['./open-recent.component.scss'],
  animations: [
    bounceInLeftOnEnterAnimation({ translate: '500px' }),
  ]
})
export class OpenRecentComponent implements OnInit {

  public dataSource: Observable<MdProject[]>

  constructor(private projectService: ProjectsService,
    private mdFileService: MdFileService,
    private router: Router,
    private route: ActivatedRoute,
    private dialog: MatDialog) { }

  ngOnInit(): void {
    this.projectService.fetchProjects();
    this.dataSource = this.projectService.mdProjects;
    // when the project is loaded, then switch to navigation environment
    this.projectService.currentProjects$.subscribe(_ => {
      if (_ != null && _ != undefined) {
        this.router.navigate(['/main/navigation/document']);
      }      
    });
  }
  openNewProject(path: string) {    
    this.projectService.setNewFolderProject(path);
  }

  getProjectList(data: any, objectThis: OpenRecentComponent): void {
    objectThis.projectService.fetchProjects();
  };

  deleteProject(project: any) {
    this.projectService.deleteProject(project, this.getProjectList, this);
  }

  openProjectSettings(project: any) {
    const dialogRef = this.dialog.open(ProjectSettingsComponent, {
      width: '600px',
      data: { 
        projectId: project.id,
        projectName: project.name 
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log('Settings dialog closed');
    });
  }

}
