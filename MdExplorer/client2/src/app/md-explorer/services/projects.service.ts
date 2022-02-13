import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MdProject } from '../models/md-project';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProjectsService {

  private _mdProjects: BehaviorSubject<MdProject[]>;
  get mdProjects() {
    return this._mdProjects.asObservable();
  }

  constructor(private http: HttpClient) {
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

  setNewFolderProjectQuickNotes(path: string, callback: (data: any, objectThis: any) => any, objectThis: any) {
    const url = '../api/MdProjects/SetFolderProjectQuickNotes';
    this.http.post<any>(url, { path: path }).subscribe(data => {
      callback(data, objectThis);
    });
  }

  setNewFolderProject(path: string, callback: (data: any, objectThis: any) => any, objectThis: any) {
    const url = '../api/MdProjects/SetFolderProject';
    this.http.post<any>(url, { path: path }).subscribe(data => {
      callback(data, objectThis);
    });
  }

  deleteProject(project: any, callback: (data: any, objectThis: any) => any, objectThis: any) {
    const url = '../api/MdProjects/DeleteProject';
    this.http.post<any>(url, project).subscribe(data => {
      callback(data, objectThis);
    });
  }

}
