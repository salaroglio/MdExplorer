import { Component, Inject, OnInit } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Observable } from 'rxjs';
import { MdProject } from '../../models/md-project';
import { MdFileService } from '../../services/md-file.service';
import { ProjectsService } from '../../services/projects.service';
import { OpenNewFolderComponent } from './open-new-folder/open-new-folder.component';

@Component({
  selector: 'app-projects',
  templateUrl: './projects.component.html',
  styleUrls: ['./projects.component.scss']
})
export class ProjectsComponent implements OnInit {

  constructor(private projectService: ProjectsService,
    private mdFileService: MdFileService,
    public dialog: MatDialog,
  ) { }

  public dataSource: Observable<MdProject[]>
  public dataSource1 = [{ name: 'Nome progetto', path: 'c:\folder\folder\folder' }]

  ngOnInit(): void {
    this.projectService.fetchProjects();
    this.dataSource = this.projectService.mdProjects;    
  }

  getProjectList(data: any, objectThis: ProjectsComponent): void {
    objectThis.projectService.fetchProjects();
  };

  quickOpenNotes(path: string) {
    this.projectService.setNewFolderProjectQuickNotes(path, this.loadNewProject, this);
  }


  openNewProject(path: string) {
    this.projectService.setNewFolderProject(path, this.loadNewProject, this);
  }

  deleteProject(project: any) {
    this.projectService.deleteProject(project, this.getProjectList, this);
  }

  loadNewProject(data: any, objectThis: ProjectsComponent) {
    objectThis.mdFileService.loadAll(null,null);
    this.dialog.closeAll();
  }

  openNewFolder(): void {
    const dialogRef = this.dialog.open(OpenNewFolderComponent, {
      width: '600px',
      data: { name: 'test' }
    });

    dialogRef.afterClosed().subscribe(selectedPath => {
      this.projectService.setNewFolderProject(selectedPath, this.loadNewProject, this);

    });
  }

}
