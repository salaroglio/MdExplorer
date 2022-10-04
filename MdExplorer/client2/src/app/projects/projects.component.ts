import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { MdProject } from '../md-explorer/models/md-project';
import { MdFileService } from '../md-explorer/services/md-file.service';
import { MonitorMDService } from '../md-explorer/services/monitor-md.service';
import { ProjectsService } from '../md-explorer/services/projects.service';

@Component({
  selector: 'app-projects',
  templateUrl: './projects.component.html',
  styleUrls: ['./projects.component.scss']
})
export class ProjectsComponent implements OnInit {

  constructor(private projectService: ProjectsService,
    public dialog: MatDialog,
    private router: Router,
    private signalRService:MonitorMDService
  ) { }

  public dataSource: Observable<MdProject[]>
  public dataSource1 = [{ name: 'Nome progetto', path: 'c:\folder\folder\folder' }]

  ngOnInit(): void {
    //this.projectService.fetchProjects();
    //this.dataSource = this.projectService.mdProjects;
  }


  openRecent(): void {
    this.router.navigate(['/projects/openrecent']);
  }

  openNewFolder(): void {
    this.router.navigate(['/projects/newproject']);
  }

}
