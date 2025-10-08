import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProjectSettingsService {

  constructor(private http: HttpClient) { }

  getProjectSettings(): Observable<any[]> {
    const url = '../api/ProjectSettings/GetProjectSettings';
    return this.http.get<any[]>(url);
  }

  saveProjectSetting(setting: any): Observable<any> {
    const url = '../api/ProjectSettings/SaveProjectSetting';
    return this.http.post<any>(url, setting);
  }

  getRule1Setting(): Observable<any> {
    const url = '../api/ProjectSettings/GetRule1Setting';
    return this.http.get<any>(url);
  }

  setRule1Setting(enabled: boolean): Observable<any> {
    const url = '../api/ProjectSettings/SetRule1Setting';
    return this.http.post<any>(url, { enabled });
  }
}