import { HttpClient } from '@angular/common/http';
import { Injectable, OnDestroy } from '@angular/core';
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
export class GITService implements OnDestroy {
  private _Settings: BehaviorSubject<GitlabSetting[]>;
  private gitPollingInterval: any = null;
  private readonly ACTIVE_POLLING_INTERVAL = 60000; // 60 secondi quando attivo
  private readonly INACTIVE_POLLING_INTERVAL = 300000; // 5 minuti quando inattivo
  
  public currentBranch$: BehaviorSubject<IBranch> = new BehaviorSubject<IBranch>(
    {
      id: "", name: "",
      somethingIsChangedInTheBranch: true,
      howManyFilesAreChanged: 0,            
      fullPath: "",
      howManyCommitAreToPush:0,
    });

  public commmitsToPull$: BehaviorSubject<DataToPull> = new BehaviorSubject<DataToPull>({
    howManyFilesAreToPull: 0,
    howManyCommitAreToPush:0,
    somethingIsToPull: false,
    somethingIsToPush:false,
    connectionIsActive: false,
    whatFilesWillBeChanged: []
  });

  constructor(private http: HttpClient) {
    this.initializeSmartPolling();
  }

  /**
   * Inizializza il polling intelligente che si adatta alla visibilità della finestra
   */
  private initializeSmartPolling(): void {
    // Polling iniziale immediato
    this.getCurrentBranch();
    
    // Avvia polling con intervallo attivo
    this.startPolling(this.ACTIVE_POLLING_INTERVAL);
    
    // Listener per cambio visibilità finestra
    document.addEventListener('visibilitychange', () => {
      this.handleVisibilityChange();
    });
    
    // Listener per focus/blur finestra (backup per browser che non supportano visibilitychange)
    window.addEventListener('focus', () => {
      this.handleWindowFocus();
    });
    
    window.addEventListener('blur', () => {
      this.handleWindowBlur();
    });
    
    console.log('🔄 Smart Git polling initialized');
  }

  /**
   * Gestisce il cambio di visibilità della finestra
   */
  private handleVisibilityChange(): void {
    if (document.visibilityState === 'visible') {
      this.handleWindowFocus();
    } else {
      this.handleWindowBlur();
    }
  }

  /**
   * Quando la finestra diventa attiva: polling più frequente
   */
  private handleWindowFocus(): void {
    console.log('🟢 Window focused - activating frequent Git polling (60s)');
    this.startPolling(this.ACTIVE_POLLING_INTERVAL);
    // Polling immediato quando torna in focus
    this.getCurrentBranch();
  }

  /**
   * Quando la finestra diventa inattiva: polling meno frequente
   */
  private handleWindowBlur(): void {
    console.log('🟡 Window blurred - reducing Git polling frequency (5min)');
    this.startPolling(this.INACTIVE_POLLING_INTERVAL);
  }

  /**
   * Avvia polling con intervallo specificato
   */
  private startPolling(interval: number): void {
    // Ferma polling esistente
    if (this.gitPollingInterval) {
      clearInterval(this.gitPollingInterval);
    }
    
    // Avvia nuovo polling
    this.gitPollingInterval = setInterval(() => {
      this.getCurrentBranch();
    }, interval);
  }

  /**
   * Ferma completamente il polling (per cleanup)
   */
  public stopPolling(): void {
    if (this.gitPollingInterval) {
      clearInterval(this.gitPollingInterval);
      this.gitPollingInterval = null;
      console.log('🔴 Git polling stopped');
    }
  }

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

  /**
   * Cleanup quando il service viene distrutto
   */
  ngOnDestroy(): void {
    this.stopPolling();
    
    // Rimuovi event listeners per evitare memory leak
    document.removeEventListener('visibilitychange', this.handleVisibilityChange);
    window.removeEventListener('focus', this.handleWindowFocus);
    window.removeEventListener('blur', this.handleWindowBlur);
    
    console.log('🧹 Git service cleanup completed');
  }

}
