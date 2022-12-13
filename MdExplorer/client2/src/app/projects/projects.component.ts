import { Component, OnDestroy, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { MdProject } from '../md-explorer/models/md-project';
import { MdFileService } from '../md-explorer/services/md-file.service';
import { ServerMessagesService } from '../signalr/services/server-messages.service';
import { ProjectsService } from '../md-explorer/services/projects.service';
import { NewProjectComponent } from './new-project/new-project.component';

@Component({
  selector: 'app-projects',
  templateUrl: './projects.component.html',
  styleUrls: ['./projects.component.scss']
})
export class ProjectsComponent implements OnInit, OnDestroy {

  constructor(private projectService: ProjectsService,
    public dialog: MatDialog,
    private router: Router,
    private signalRService: ServerMessagesService
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

  openNewFolder(): void {
    const dialogRef = this.dialog.open(NewProjectComponent, {
      width: '600px',
      data: null
    });
  }
}
