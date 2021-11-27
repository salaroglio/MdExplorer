import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Observable } from 'rxjs';
import { MdProject } from '../../models/md-project';
import { MdFileService } from '../../services/md-file.service';
import { ProjectsService } from '../../services/projects.service';

@Component({
  selector: 'app-projects',
  templateUrl: './projects.component.html',
  styleUrls: ['./projects.component.scss']
})
export class ProjectsComponent implements OnInit {

  constructor(private projectService: ProjectsService,
    private mdFileService: MdFileService,   
  ) { }

  public dataSource: Observable<MdProject[]>
  public dataSource1 = [{ name: 'Nome progetto', path: 'c:\folder\folder\folder' }]

  ngOnInit(): void {   
    this.projectService.fetchProjects();
    this.dataSource = this.projectService.mdProjects;
  }

  openNewProject(path: string) {    
    this.projectService.setNewFolderProject(path, this.loadNewProject,this);
  }

  loadNewProject(data: any, objectThis: ProjectsComponent) {
    objectThis.mdFileService.loadAll();
  }
  
}
