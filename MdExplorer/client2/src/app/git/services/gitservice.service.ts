import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Branch } from '../models/branch';
import { GitlabSetting } from '../models/gitlab-setting';




@Injectable({
  providedIn: 'root'
})
export class GITService {
  private _Settings: BehaviorSubject<GitlabSetting[]>;

  constructor(private http: HttpClient) { }

  getCurrentBranch() {
    const url = '../api/gitservice/branches/feat/getcurrentbranch';
    return this.http.get<Branch>(url);
  }

  storeGitlabSettings(user: string, password: string, gitlabLink: string) {
    const url = '../api/gitservice/gitlabsettings';
    let setting = new GitlabSetting();    
    return this.http.post<GitlabSetting>(url, setting);
  }

  getGitlabSettings() {
    const url = '../api/gitservice/gitlabsettings'
    return this.http.get<GitlabSetting[]>(url);
  }
}
