import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { IBranch } from '../models/branch';
import { GitlabSetting } from '../models/gitlab-setting';
import { ITag } from '../models/Tag';




@Injectable({
  providedIn: 'root'
})
export class GITService {
  private _Settings: BehaviorSubject<GitlabSetting[]>;

  constructor(private http: HttpClient) { }

  getCurrentBranch() {
    const url = '../api/gitservice/branches/feat/getcurrentbranch';
    return this.http.get<IBranch>(url);
  }

  getBranchList():Observable<IBranch[]> {
    const url = '../api/gitservice/branches';
    return this.http.get<IBranch[]>(url);
  }

  checkoutSelectedBranch(selected: IBranch) {
    const url = '../api/gitservice/branches/feat/checkoutBranch';
    return this.http.post<IBranch>(url, selected);

  }

  getTagList() {
    const url = '../api/gitservice/tags';
    return this.http.get<ITag[]>(url);
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
