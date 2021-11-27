import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { MdFile } from '../models/md-file';

@Injectable({
  providedIn: 'root'
})
export class MdFileService {

  private _mdFiles: BehaviorSubject<MdFile[]>;

  private dataStore: {
    mdFiles: MdFile[]
  }
  constructor(private http: HttpClient) {
    this.dataStore = { mdFiles: [] };
    this._mdFiles = new BehaviorSubject<MdFile[]>([]);

  }

  get mdFiles(): Observable<MdFile[]> {
    return this._mdFiles.asObservable();
  }


  loadAll() {
    const url = '../api/mdfiles';
    return this.http.get<MdFile[]>(url)
      .subscribe(data => {
        this.dataStore.mdFiles = data;
        this._mdFiles.next(Object.assign({}, this.dataStore).mdFiles);
      },
        error => {
          console.log("failed to fetch mdfile list");
        });
  }

  GetHtml(path: string) { //, currentFile: MdFile
    const url = '../api/mdexplorer/' + path;
    return this.http.get(url, { responseType: 'text' })//, currentFile
      
  }

}
