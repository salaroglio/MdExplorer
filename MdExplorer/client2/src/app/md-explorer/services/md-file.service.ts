import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { promise } from 'protractor';
import { BehaviorSubject, Observable } from 'rxjs';
import { MdFile } from '../models/md-file';
import { IChangeFileData } from './md-refactoring.service';

@Injectable({
  providedIn: 'root'
})
export class MdFileService {
  
  private _whatDisplayForToolbar: BehaviorSubject<string>;
  private _mdFiles: BehaviorSubject<MdFile[]>;
  private _mdFoldersDocument: BehaviorSubject<MdFile[]>;
  public _mdDynFolderDocument: BehaviorSubject<MdFile[]>;
  public _serverSelectedMdFile: BehaviorSubject<MdFile[]>;
  private _navigationArray: MdFile[] = [];// deve morire
  private _selectedMdFileFromToolbar: BehaviorSubject<MdFile[]>;
  private _selectedMdFileFromSideNav: BehaviorSubject<MdFile>;

  private dataStore: {
    mdFiles: MdFile[]
    mdFoldersDocument: MdFile[]
    mdDynFolderDocument: MdFile[]
    serverSelectedMdFile: MdFile[]
  }
  constructor(private http: HttpClient) {

    var defaultSelectedMdFile = [];
    this.dataStore = {
      mdFiles: [],
      mdFoldersDocument: [],
      mdDynFolderDocument: [],
      serverSelectedMdFile: defaultSelectedMdFile
    };

    this._mdFiles = new BehaviorSubject<MdFile[]>([]);
    this._mdFoldersDocument = new BehaviorSubject<MdFile[]>([]);
    this._mdDynFolderDocument = new BehaviorSubject<MdFile[]>([]);
    this._serverSelectedMdFile = new BehaviorSubject<MdFile[]>([]);
    this._selectedMdFileFromToolbar = new BehaviorSubject<MdFile[]>([]);
    this._selectedMdFileFromSideNav = new BehaviorSubject<MdFile>(null);
    this._whatDisplayForToolbar = new BehaviorSubject<string>('block');
  }

  get whatDisplayForToolbar(): Observable<string> {
    return this._whatDisplayForToolbar.asObservable();
  }

  setWhatDisplayForToolbar(value: string) {
    this._whatDisplayForToolbar.next(value);
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

  get serverSelectedMdFile(): Observable<MdFile[]> {
    return this._serverSelectedMdFile.asObservable();
  }

  get selectedMdFileFromToolbar(): Observable<MdFile[]> {
    return this._selectedMdFileFromToolbar.asObservable();
  }

  get selectedMdFileFromSideNav(): Observable<MdFile> {
    return this._selectedMdFileFromSideNav.asObservable();
  }

  

  // breadcrumb
  get navigationArray(): MdFile[] {
    return this._navigationArray;
  }

  set navigationArray(mdFile: MdFile[]) {
    this._navigationArray = mdFile;
  }

  addNewFile(data: MdFile[]) {
    // searching directories
    debugger;
      var currentItem = data[0];
      let currentFolder = this.dataStore.mdFiles.find(_ => _.fullPath == currentItem.fullPath);
    if (currentFolder != undefined) {
      this.recursiveSearchFolder(data, 0, currentFolder);
    } else {
      if (currentFolder == undefined) { // the file is in the root
        this.dataStore.mdFiles.push(data[0]);
      } else {
        currentFolder.childrens.push(data[0]); // insert new file in folder
      }
      
      this._mdFiles.next(Object.assign({}, this.dataStore).mdFiles);
    }
        
  }

  recursiveSearchFolder(data: MdFile[], i: number, parentFolder:MdFile) {
    var currentI = i + 1;
    var currentItem = data[currentI];
    let currentFolder = parentFolder.childrens.find(_ => _.fullPath == currentItem.fullPath);
    if (currentFolder != undefined) {
      this.recursiveSearchFolder(data, currentI,currentFolder);
    } else {
      parentFolder.childrens.push(data[currentI]); // insert new file
      this._mdFiles.next(Object.assign({}, this.dataStore).mdFiles);
    }
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

  CreateNewMd(path: string, title: string, directoryLevel:number) {
    const url = '../api/mdfiles/CreateNewMd';
    var newData = {
      directoryPath: path,
      title: title,
      directoryLevel: directoryLevel,
    }
    return this.http.post<MdFile[]>(url, newData);
  }


  fileFoundMd: boolean = false;
 

  /**
   * Funzione di sostituzione di un nodo, con un altro
   * @param oldFile
   * @param newFile
   */
  changeDataStoreMdFiles(oldFile: MdFile, newFile: MdFile) {
    
    var returnFound = this.searchMdFileIntoDataStore(this.dataStore.mdFiles, oldFile);
    var leaf = returnFound[0];
    leaf.name = newFile.name;
    leaf.fullPath = newFile.fullPath;
    leaf.path = newFile.path;
    leaf.relativePath = newFile.relativePath;
    this._mdFiles.next(Object.assign({}, this.dataStore).mdFiles);
    this._serverSelectedMdFile.next(returnFound);
  }

  setSelectedMdFileFromSideNav(selectedFile: MdFile) {
    this._selectedMdFileFromSideNav.next(selectedFile);
  }

  setSelectedMdFileFromToolbar(selectedFile: MdFile) {
    let returnFound = this.searchMdFileIntoDataStore(this.dataStore.mdFiles, selectedFile);
    this._selectedMdFileFromToolbar.next(returnFound);
  }

  setSelectedMdFileFromServer(selectedFile: MdFile) {
    var returnFound = this.searchMdFileIntoDataStore(this.dataStore.mdFiles, selectedFile); 
    this._serverSelectedMdFile.next(returnFound);
  }


  getMdFileFromDataStore(selectedFile: MdFile):MdFile {
    var returnFound = this.searchMdFileIntoDataStore(this.dataStore.mdFiles, selectedFile);
    return returnFound[0];
  }

  searchMdFileIntoDataStore(arrayMd: MdFile[], FileToFind: MdFile): MdFile[] {
    this.fileFoundMd = false;    
    var arrayFound: MdFile[]  = [];
    this.recursiveSearch(arrayMd, FileToFind, arrayFound);
    return arrayFound;
  }


/**
 * Funzione di esplorazione dell'albero per trovare il nodo
 * @param arrayMd
 * @param oldFile
 * @param newFile
 */
  recursiveSearch(arrayMd: MdFile[], fileTofind: MdFile, arrayFound: MdFile[]
    //, newFile: MdFile
  ) {
    if (arrayMd.length == 0) {
      return;
    }
    var thatFile = arrayMd.find(_ => _.fullPath.toLowerCase() == fileTofind.fullPath.toLowerCase());
   
    if (thatFile == undefined) {
      for (var i = 0; i < arrayMd.length; i++) {
        var _ = arrayMd[i];
        if (!this.fileFoundMd) {
          this.recursiveSearch(_.childrens, fileTofind, arrayFound);//, newFile
        } 
        if (this.fileFoundMd) {
          arrayFound.push(_);
          break;
        }
        
      }

    } else {      
      this.fileFoundMd = true;
      arrayFound.push(thatFile);
    }
  }

}

export interface INewFileCreated {  
  newName: string;  
  newPath: string;  
  newLevel: number;
  expandable: boolean;
  relativePath: boolean;
}
