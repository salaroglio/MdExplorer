import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AppCurrentFolderService {

  private _folderName: BehaviorSubject<string>;

  private dataStore: {
    folderName: string
  }

  constructor(private http: HttpClient) {
    this.dataStore = { folderName: 'cucu' }
    this._folderName = new BehaviorSubject<string>('test');
  }

  get folderName(): Observable<string> {
    return this._folderName.asObservable();
  }

  loadFolderName() {
    const url = '../api/AppCurrentFolder';
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

}
