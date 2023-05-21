import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { IBranch } from '../models/branch';
import { DataToPull } from '../models/DataToPull'
import { CloneInfo } from '../models/cloneRequest';
import { GitlabSetting } from '../models/gitlab-setting';
import { PullInfo } from '../models/pullInfo';
import { ResponsePull } from '../models/responsePull';
import { ITag } from '../models/Tag';
import { ResposneClone } from './responses/ResponseClone';


@Injectable({
  providedIn: 'root'
})
export class GITService {
  private _Settings: BehaviorSubject<GitlabSetting[]>;
  public currentBranch$: BehaviorSubject<IBranch> = new BehaviorSubject<IBranch>(
    {
      id: "", name: "",
      somethingIsChangedInTheBranch: true,
      howManyFilesAreChanged: 0,            
      fullPath : ""
    });

  public commmitsToPull$: BehaviorSubject<DataToPull> = new BehaviorSubject<DataToPull>({
    howManyFilesAreToPull: 0,
    somethingIsToPull: false,
    connectionIsActive:false,
  });

  constructor(private http: HttpClient) { }

  clone(request: CloneInfo): Observable<ResposneClone> {
    const url = '../api/gitfeatures/cloneRepository';
    return this.http.post<ResposneClone>(url, request);
  }

  getCurrentBranch():Observable<IBranch> {
    const url = '../api/gitservice/branches/feat/getcurrentbranch';
    let data$ = this.http.get<IBranch>(url);
    data$.subscribe(_ => {      
      this.currentBranch$.next(_);
    });
    debugger;
    const url2 = '../api/gitservice/branches/feat/getdatatopull';
    let data2$ = this.http.get<DataToPull>(url2);
    data2$.subscribe(_ => {
      this.commmitsToPull$.next(_);
    });
    return data$;

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

  getGitlabSettings(): Observable<GitlabSetting[]> {
    const url = '../api/gitservice/gitlabsettings';
    return this.http.get<GitlabSetting[]>(url);
  }

  pull(request:PullInfo): Observable<ResponsePull>  {
    const url = '../api/gitfeatures/pull';
    return this.http.post<ResponsePull>(url, request);
    //return this.http.get<any>(url);
  }

  commitAndPush(request: PullInfo): Observable<ResponsePull> {
    const url = '../api/gitfeatures/commitandpush';
    return this.http.post<ResponsePull>(url, request);
  }

  commit(request: PullInfo): Observable<ResponsePull> {
    const url = '../api/gitfeatures/commit';
    return this.http.post<ResponsePull>(url, request);
  }

  push(request: PullInfo): Observable<ResponsePull> {
    const url = '../api/gitfeatures/push';
    return this.http.post<ResponsePull>(url, request);
  }

}
