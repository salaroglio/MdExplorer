import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { bounceInLeftOnEnterAnimation } from 'angular-animations';
import { Observable } from 'rxjs';
import { MdProject } from '../../md-explorer/models/md-project';
import { MdFileService } from '../../md-explorer/services/md-file.service';
import { ProjectsService } from '../../md-explorer/services/projects.service';

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
    private route: ActivatedRoute) { }

  ngOnInit(): void {
    this.projectService.fetchProjects();
    this.dataSource = this.projectService.mdProjects;
  }
  quickOpenNotes(path: string) {
    this.projectService.setNewFolderProjectQuickNotes(path).subscribe(_ => {      
      this.router.navigate(['/main/navigation/document'])
    });
  }

  openNewProject(path: string) {    
    this.projectService.setNewFolderProject(path).subscribe(_ => {      
      this.router.navigate(['/main/navigation/document']);
    });
  }

  getProjectList(data: any, objectThis: OpenRecentComponent): void {
    objectThis.projectService.fetchProjects();
  };

  deleteProject(project: any) {
    this.projectService.deleteProject(project, this.getProjectList, this);
  }

}
