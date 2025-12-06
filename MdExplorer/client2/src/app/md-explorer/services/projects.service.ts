import { Injectable, Injector } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MdProject } from '../models/md-project';
import { BehaviorSubject, Observable } from 'rxjs';
import { ProjectCreateConfigOptions } from '../../projects/dialogs/project-create-config/project-create-config.model';
import { CompatibilityMode } from '../../models/compatibility-mode.model';

@Injectable({
  providedIn: 'root'
})
export class ProjectsService {

  private _mdProjects: BehaviorSubject<MdProject[]>;
  currentProjects$: BehaviorSubject<MdProject> = new BehaviorSubject<MdProject>(null);

  get mdProjects() {
    return this._mdProjects.asObservable();
  }

  constructor(
    private http: HttpClient,
    private injector: Injector
  ) {
    this.dataStore = { mdProjects: [] };
    this._mdProjects = new BehaviorSubject<MdProject[]>([]);
  }

  private dataStore: {

    mdProjects: MdProject[]
  }

  fetchProjects() {
    const url = '../api/MdProjects/GetProjects';
    this.http.get<MdProject[]>(url)
      .subscribe(data => {
        this.dataStore.mdProjects = data;
        this._mdProjects.next(Object.assign({}, this.dataStore).mdProjects);
      }, error => {
        console.log(error);
      });
  }

  async SetSideNavWidth(mdProject: MdProject) {
    const url = '../api/MdProjects/SetSideNavWidth';
    await this.http.post<any>(url, mdProject).toPromise();
  }

  setNewFolderProject(path: string):void {
    this.http.post<any>('../api/MdProjects/SetFolderProject', { path: path }).subscribe(async response => {
      this.currentProjects$.next(response);

      // Update compatibility mode from response
      if (response.compatibilityMode) {
        const mode = response.compatibilityMode === 'github' ? CompatibilityMode.GitHub :
                     response.compatibilityMode === 'commonmark' ? CompatibilityMode.CommonMark :
                     CompatibilityMode.MdExplorer;
        console.log('Setting compatibility mode from project open response:', mode);

        // Get CompatibilityModeService using dynamic import to avoid circular dependency
        const { CompatibilityModeService } = await import('../../services/compatibility-mode.service');
        const compatibilityService = this.injector.get(CompatibilityModeService);
        compatibilityService.updateMode(mode);
      }
    });

  }

  createProjectWithConfig(config: ProjectCreateConfigOptions): void {
    const request = {
      path: config.projectPath,
      initializeGit: config.initializeGit,
      addCopilotInstructions: config.addCopilotInstructions
    };

    this.http.post<any>('../api/MdProjects/SetFolderProject', request).subscribe(async response => {
      this.currentProjects$.next(response);

      // Update compatibility mode from response
      if (response.compatibilityMode) {
        const mode = response.compatibilityMode === 'github' ? CompatibilityMode.GitHub :
                     response.compatibilityMode === 'commonmark' ? CompatibilityMode.CommonMark :
                     CompatibilityMode.MdExplorer;
        console.log('Setting compatibility mode from project create response:', mode);

        // Get CompatibilityModeService using dynamic import to avoid circular dependency
        const { CompatibilityModeService } = await import('../../services/compatibility-mode.service');
        const compatibilityService = this.injector.get(CompatibilityModeService);
        compatibilityService.updateMode(mode);
      }
    }, error => {
      console.error('Error creating project with config:', error);
    });
  }

  //setNewFolderProject(path: string, callback: (data: any, objectThis: any) => any, objectThis: any) {
  //  const url = '../api/MdProjects/SetFolderProject';
  //  this.http.post<any>(url, { path: path }).subscribe(data => {
  //    callback(data, objectThis);
  //  });
  //}

  deleteProject(project: any, callback: (data: any, objectThis: any) => any, objectThis: any) {
    const url = '../api/MdProjects/DeleteProject';
    this.http.post<any>(url, project).subscribe(data => {
      callback(data, objectThis);
    });
  }

  /**
   * Closes the current project and deallocates backend resources (FileSystemWatcher, database contexts).
   * Should be called when navigating back to the projects list.
   */
  closeCurrentProject(): Observable<any> {
    return this.http.post<any>('../api/MdProjects/CloseProject', {});
  }

}
