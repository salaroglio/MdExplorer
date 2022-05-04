import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { promise } from 'protractor';
import { BehaviorSubject, Observable } from 'rxjs';
import { MdFile } from '../models/md-file';

@Injectable({
  providedIn: 'root'
})
export class MdFileService {

  private _mdFiles: BehaviorSubject<MdFile[]>;
  private _mdFoldersDocument: BehaviorSubject<MdFile[]>;
  public _mdDynFolderDocument: BehaviorSubject<MdFile[]>;

  private dataStore: {
    mdFiles: MdFile[]
    mdFoldersDocument: MdFile[]
    mdDynFolderDocument:MdFile[]
  }
  constructor(private http: HttpClient) {
    this.dataStore = { mdFiles: [], mdFoldersDocument: [], mdDynFolderDocument:[] };
    this._mdFiles = new BehaviorSubject<MdFile[]>([]);
    this._mdFoldersDocument = new BehaviorSubject<MdFile[]>([]);
    this._mdDynFolderDocument = new BehaviorSubject<MdFile[]>([]);

  }

  get mdFiles(): Observable<MdFile[]> {
    return this._mdFiles.asObservable();
  }
  get mdFoldersDocument(): Observable<MdFile[]> {
    return this._mdFoldersDocument.asObservable();
  }

  get mdDynFolderDocument(): Observable<MdFile[]> {
    return this._mdDynFolderDocument.asObservable();
  }

  loadAll(callback: (data: any, objectThis: any) => any, objectThis: any) {    
    const url = '../api/mdfiles/GetAllMdFiles';
    return this.http.get<MdFile[]>(url)
      .subscribe(data => {
        this.dataStore.mdFiles = data;
        this._mdFiles.next(Object.assign({}, this.dataStore).mdFiles);
        if (callback != null) {
          callback(data, objectThis);
        }        
      },
        error => {
          console.log("failed to fetch mdfile list");
        });
  }

  loadFolders() {
    const url = '../api/mdfiles/GetFoldersDocument';
    return this.http.get<MdFile[]>(url)
      .subscribe(data => {
        this.dataStore.mdFoldersDocument = data;
        this._mdFoldersDocument.next(Object.assign({}, this.dataStore).mdFoldersDocument);
      },
        error => {
          console.log("failed to fetch mdfile list");
        });
  }

  loadDynFolders(path:string, level:number) {
    const url = '../api/mdfiles/GetDynFoldersDocument';    
    var params = new HttpParams().set('path', path).set('level', String(level));
    
    return this.http.get<MdFile[]>(url, {params})
      .subscribe(data => {
        if (this.dataStore.mdDynFolderDocument.length > 0) {
          var test = this.dataStore.mdDynFolderDocument.find(_ => _.path == path);
          test.children = data;          
        } else {
          this.dataStore.mdDynFolderDocument = data;
        }
        this._mdDynFolderDocument.next(Object.assign({}, this.dataStore).mdDynFolderDocument);
      },
        error => {
          console.log("failed to fetch mdfile list");
        });
  }

  loadDocumentFolder(path: string, level: number): Observable<MdFile[]> {
    const url = '../api/mdfiles/GetDynFoldersDocument';
    var params = new HttpParams().set('path', path).set('level', String(level));
    return this.http.get<MdFile[]>(url, { params });
  }


  GetHtml(path: string) { //, currentFile: MdFile
    const url = '../api/mdexplorer/' + path;
    return this.http.get(url, { responseType: 'text' })//, currentFile      
  }

  changeDataStoreMdFiles(oldFile: MdFile, newFile: MdFile) {
    debugger;
    this.exploreMdFiles(this.dataStore.mdFiles, oldFile, newFile);
    this._mdFiles.next(Object.assign({}, this.dataStore).mdFiles);
  }

  foundMd: boolean = false;

  exploreMdFiles(arrayMd: MdFile[], oldFile: MdFile, newFile: MdFile) {
    if (arrayMd.length == 0) {
      return;
    }
    var thatFile = arrayMd.find(_ => _.fullPath == oldFile.fullPath);
    if (thatFile == undefined) {
      arrayMd.map(_ => {
        if (!this.foundMd) {    
          this.exploreMdFiles(_.childrens, oldFile, newFile);
        }        
      });
    } else {
      debugger;
      this.foundMd = true;
      thatFile.name = newFile.name;
      thatFile.path = newFile.path;
    }
  }



}
