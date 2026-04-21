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

  getStickyScrollSetting(): Observable<{enabled: boolean}> {
    return this.http.get<{enabled: boolean}>('../api/ProjectSettings/GetStickyScrollSetting');
  }

  setStickyScrollSetting(enabled: boolean): Observable<any> {
    return this.http.post('../api/ProjectSettings/SetStickyScrollSetting', { enabled });
  }

  getLinkIndexingSetting(projectPath: string): Observable<any> {
    const url = '../api/ProjectSettings/GetLinkIndexingSetting';
    return this.http.get<any>(url, { params: { projectPath } });
  }

  setLinkIndexingSetting(enabled: boolean, projectPath: string): Observable<any> {
    const url = '../api/ProjectSettings/SetLinkIndexingSetting';
    return this.http.post<any>(url, { enabled, projectPath });
  }

  getPlantUmlKeepOriginalColorsSetting(projectPath: string): Observable<{enabled: boolean}> {
    const url = '../api/ProjectSettings/GetPlantUmlKeepOriginalColorsSetting';
    return this.http.get<{enabled: boolean}>(url, { params: { projectPath } });
  }

  setPlantUmlKeepOriginalColorsSetting(enabled: boolean, projectPath: string): Observable<any> {
    const url = '../api/ProjectSettings/SetPlantUmlKeepOriginalColorsSetting';
    return this.http.post<any>(url, { enabled, projectPath });
  }

  getCopilotCliAutoSelectSetting(projectPath: string): Observable<{enabled: boolean}> {
    const url = '../api/ProjectSettings/GetCopilotCliAutoSelectSetting';
    return this.http.get<{enabled: boolean}>(url, { params: { projectPath } });
  }

  setCopilotCliAutoSelectSetting(enabled: boolean, projectPath: string): Observable<any> {
    const url = '../api/ProjectSettings/SetCopilotCliAutoSelectSetting';
    return this.http.post<any>(url, { enabled, projectPath });
  }

  // RAG Settings
  getRagStatus(): Observable<any> {
    const url = '../api/Rag/status';
    return this.http.get<any>(url);
  }

  enableRag(): Observable<any> {
    const url = '../api/Rag/enable';
    return this.http.post<any>(url, {});
  }

  disableRag(): Observable<any> {
    const url = '../api/Rag/disable';
    return this.http.post<any>(url, {});
  }

  reindexRag(projectPath: string): Observable<any> {
    const url = '../api/Rag/reindex';
    return this.http.post<any>(url, { projectPath });
  }

  clearRagIndex(projectPath: string): Observable<any> {
    const url = '../api/Rag/clear';
    return this.http.post<any>(url, { projectPath });
  }

  indexRagFile(filePath: string, projectPath: string, forceReindex = false): Observable<any> {
    const url = '../api/Rag/index-file';
    return this.http.post<any>(url, { filePath, projectPath, forceReindex });
  }

  indexRagDirectory(directoryPath: string, projectPath: string, forceReindex = false): Observable<any> {
    const url = '../api/Rag/index-directory';
    return this.http.post<any>(url, { directoryPath, projectPath, forceReindex });
  }
}