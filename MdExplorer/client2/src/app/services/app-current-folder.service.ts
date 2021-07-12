import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { IMdSetting } from '../Models/IMdSetting';
import { MdSetting } from '../Models/MdSettings';

@Injectable({
  providedIn: 'root'
})
export class AppCurrentFolderService {

  private _folderName: BehaviorSubject<string>;
  private _Settings: BehaviorSubject<IMdSetting[]>;


  private dataStore: {
    folderName: string
    settings: IMdSetting[]
  }

  constructor(private http: HttpClient) {
    
    this.dataStore = {
      folderName: 'cucu', settings: [new MdSetting({ id:'test', name: 'PlantumlServer' })]}
    this._folderName = new BehaviorSubject<string>('test');
    this._Settings = new BehaviorSubject<IMdSetting[]>([]);
  }

  get folderName(): Observable<string> {
    return this._folderName.asObservable();
  }

  get settings(): Observable<IMdSetting[]> {
    return this._Settings.asObservable();
  }

  loadFolderName() {
    const url = '../api/AppSettings/GetCurrentFolder';
    return this.http.get<string>(url)
      .subscribe(data => {
        this.dataStore.folderName = data;
        this._folderName.next(Object.assign({},this.dataStore).folderName);
      },
        error => {
          console.log("failed to fetch working folder name");
        }
      );
  }

  loadSettings() {
    const url = '../api/AppSettings/GetSettings';
    return this.http.get<IMdSetting[]>(url)
      .subscribe(data => {
        this.dataStore.settings = data;
        this._Settings.next(Object.assign({}, this.dataStore).settings);
      }
        , error => {

        });
  }

  saveSettings() {
    const url = '../api/AppSettings/SetSettings';    
    var test = { settings: this.dataStore.settings };
    return this.http.post<IMdSetting[]>(url, this.dataStore.settings);
  }

  killServer() {
    const url = '../api/AppSettings/KillServer';
    return this.http.get(url).subscribe(data => {
      debugger;
    });
  }

}
