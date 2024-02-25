import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { MdFile } from '../models/md-file';
import { IDocumentSettings } from './Types/IDocumentSettings';
import { MdServerMessagesService } from '../../signalR/services/server-messages.service';

@Injectable({
  providedIn: 'root'
})
export class MdFileService {

  private _whatDisplayForToolbar: BehaviorSubject<string>;
  public _mdFiles: BehaviorSubject<MdFile[]>;
  public _mdDynFolderDocument: BehaviorSubject<MdFile[]>;
  public _serverSelectedMdFile: BehaviorSubject<MdFile[]>;
  private _navigationArray: MdFile[] = [];// deve morire
  private _selectedMdFileFromToolbar: BehaviorSubject<MdFile[]>;
  private _selectedMdFileFromSideNav: BehaviorSubject<MdFile>;
  private _selectedDirectoryFromNewDirectory: BehaviorSubject<MdFile>;

  private dataStore: {
    mdFiles: MdFile[]
    mdFoldersDocument: MdFile[]
    mdDynFolderDocument: MdFile[]
    serverSelectedMdFile: MdFile[]
  }
  constructor(private http: HttpClient,
    private mdServerMessages: MdServerMessagesService) {

    var defaultSelectedMdFile = [];
    this.dataStore = {
      mdFiles: [],
      mdFoldersDocument: [],
      mdDynFolderDocument: [],
      serverSelectedMdFile: defaultSelectedMdFile
    };

    this._mdFiles = new BehaviorSubject<MdFile[]>([]);
    this._mdDynFolderDocument = new BehaviorSubject<MdFile[]>([]);
    this._serverSelectedMdFile = new BehaviorSubject<MdFile[]>([]);
    this._selectedMdFileFromToolbar = new BehaviorSubject<MdFile[]>([]);
    this._selectedMdFileFromSideNav = new BehaviorSubject<MdFile>(null);
    this._selectedDirectoryFromNewDirectory = new BehaviorSubject<MdFile>(null);
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

  get selectedDirectoryFromNewDirectory(): Observable<MdFile> {
    return this._selectedDirectoryFromNewDirectory.asObservable();
  }


  // breadcrumb
  get navigationArray(): MdFile[] {
    return this._navigationArray;
  }

  set navigationArray(mdFile: MdFile[]) {
    this._navigationArray = mdFile;
  }

  moveMdFile(mdFile: MdFile, pathDestination: string) {
    const url = '../api/mdFiles/moveMdFile';
    return this.http.post<any>(url, { mdFile: mdFile, destinationPath:pathDestination });
  }

  openInheritingTemplateWord(InheringTemplate: string) {
    const url = '../api/mdFiles/openinheritingtemplateWord';
    return this.http.post<any>(url, { templateName: InheringTemplate });
  }

  opencustomwordtemplate(mdFile: MdFile) {
    const url = '../api/mdFiles/opencustomwordtemplate';
    return this.http.post<any>(url, mdFile);
  }

  setDocumentSettings(documentDescriptor: IDocumentSettings , mdFile:MdFile) {
    const url = '../api/mdFiles/setdocumentsettings';
    return this.http.post<any>(url, { documentDescriptor, mdFile });
  }

  getDocumentSettings(mdFile: MdFile): Observable<IDocumentSettings> {
    const url = '../api/mdFiles/getdocumentsettings';
    var params = new HttpParams().set('fullPath', mdFile.fullPath)
    return this.http.get<IDocumentSettings>(url, { params });
  }

  addNewFile(data: MdFile[]) {
    // searching directories    
    var currentItem = data[0];
    let currentFolder = this.dataStore.mdFiles.find(_ => _.fullPath == currentItem.fullPath);
    if (currentFolder != undefined) {
      this.recursiveSearchFolder(data, 0, currentFolder);
    } else {
      if (currentFolder == undefined) { // the file is in the root
        // first find the dummy (transparent)
        // the remove-it
        // then add folder
        // then add the dummy again
        let dummyItem = this.dataStore.mdFiles.pop();
        this.dataStore.mdFiles.push(data[0]);
        this.dataStore.mdFiles.push(dummyItem);
      } else {
        currentFolder.childrens.push(data[0]); // insert new file in folder
      }
      this._mdFiles.next(Object.assign({}, this.dataStore).mdFiles);
    }
  }


  addNewDirectory(data: MdFile[]) {
    // searching directories
    
    var currentItem = data[0];
    currentItem.expandable = true;
    let currentFolder = this.dataStore.mdFiles.find(_ => _.fullPath == currentItem.fullPath);
    if (currentFolder != undefined) {
      this.recursiveSearchFolder(data, 0, currentFolder);
    } else {
      if (currentFolder == undefined) { // the directory is in the root
        let dummyItem = this.dataStore.mdFiles.pop();
        this.dataStore.mdFiles.push(currentItem);
        this.dataStore.mdFiles.push(dummyItem);
      } else {
        currentFolder.childrens.push(currentItem); // insert new directory in folder
      }
      this._mdFiles.next(Object.assign({}, this.dataStore).mdFiles);
    }
  }

  recursiveSearchFolder(data: MdFile[], i: number, parentFolder: MdFile) {
    var currentI = i + 1;
    var currentItem = data[currentI];
    let currentFolder = parentFolder.childrens.find(_ => _.fullPath == currentItem.fullPath);
    if (currentFolder != undefined) {
      this.recursiveSearchFolder(data, currentI, currentFolder);
    } else {
      parentFolder.childrens.push(data[currentI]); // insert new file
      this._mdFiles.next(Object.assign({}, this.dataStore).mdFiles);
    }
  }




  loadAll(callback: (data: any, objectThis: any) => any, objectThis: any) {
    const url = '../api/mdfiles/GetAllMdFiles?connectionId=' + this.mdServerMessages.connectionId;    
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


  loadDynFolders(path: string, level: number) {
    const url = '../api/mdfiles/GetDynFoldersDocument';
    var params = new HttpParams().set('path', path).set('level', String(level));

    return this.http.get<MdFile[]>(url, { params })
      .subscribe(data => {
        if (this.dataStore.mdDynFolderDocument.length > 0) {          
          //var test = this.dataStore.mdDynFolderDocument.find(_ => _.path == path);
          //test.children = data;
        } else {
          this.dataStore.mdDynFolderDocument = data;
        }
        this._mdDynFolderDocument.next(Object.assign({}, this.dataStore).mdDynFolderDocument);
      },
        error => {
          console.log("failed to fetch mdfile list");
        });
  }

  loadDocumentFolder(path: string, level: number, typeOfSelection:string): Observable<MdFile[]> {
    let url = '../api/mdfiles/GetDynFoldersDocument';
        
    if (typeOfSelection==="FoldersAndFiles") {
      url = '../api/mdfiles/GetDynFoldersAndFilesDocument';
    }
    console.log(url);
    var params = new HttpParams().set('path', path).set('level', String(level));
    return this.http.get<MdFile[]>(url, { params });
  }



  loadPublishNodes(path: string, level: number): Observable<MdFile[]> {
    const url = '../api/mdPublishNodes';
    var params = new HttpParams().set('path', path).set('level', String(level));
    return this.http.get<MdFile[]>(url, { params });
  }


  GetHtml(path: string) { //, currentFile: MdFile
    const url = '../api/mdexplorer/' + path;
    return this.http.get(url, { responseType: 'text' })//, currentFile      
  }

  getLandingPage() {
    const url = '../api/mdfiles/GetLandingPage';
    return this.http.get<MdFile>(url);
  }

  SetLandingPage(file: MdFile) {
    const url = '../api/mdfiles/SetLandingPage';
    return this.http.post<MdFile>(url, file);
  }
  openFolderOnFileExplorer(file: MdFile) {
    const url = '../api/mdfiles/OpenFolderOnFileExplorer';
    return this.http.post<MdFile>(url, file);
  }

  deleteFile(file: MdFile) {
    const url = '../api/mdfiles/DeleteFile';
    return this.http.post<MdFile>(url, file);
      

      //this._mdFiles.next(Object.assign({}, this.dataStore).mdFiles);
    
  }

  recursiveDeleteFileFromDataStore(fileToFind: MdFile) {
    
    let dataFound: MdFile[] =[];    
    this.recursiveSearch(this.dataStore.mdFiles, fileToFind, dataFound);
    if (dataFound.length == 1) {
      var dataIndex = this.dataStore.mdFiles.indexOf(dataFound[0]);
      this.dataStore.mdFiles.splice(dataIndex, 1);
    }
    if (dataFound.length > 1) {
      //let cursor = this.dataStore.mdFiles;
      let currentFolder: MdFile[] = this.dataStore.mdFiles;
      for (var i = dataFound.length -1 ; i >0 ; i--) {
        currentFolder = currentFolder[currentFolder.indexOf(dataFound[i])].childrens;
      }
      currentFolder.splice(currentFolder.indexOf(dataFound[dataFound.length - 1]), 1);
      

    }
    this._mdFiles.next(Object.assign({}, this.dataStore).mdFiles);    
  }

  recursiveSearchForShowData(fileToFind):MdFile[] {
    let dataFound: MdFile[] = [];
    this.recursiveSearch(this.dataStore.mdFiles, fileToFind, dataFound);
    return dataFound;
  }


  CreateNewDirectoryEx(path: string, directoryName: string, directoryLevel: number) {
    const url = '../api/mdfiles/CreateNewDirectoryEx';
    var newData = {
      directoryPath: path,
      directoryName: directoryName,
      directoryLevel: directoryLevel,
    }
    return this.http.post<MdFile>(url, newData);
  }


  CreateNewDirectory(path: string, directoryName: string, directoryLevel: number) {
    const url = '../api/mdfiles/CreateNewDirectory';
    var newData = {
      directoryPath: path,
      directoryName: directoryName,
      directoryLevel: directoryLevel,
    }
    return this.http.post<MdFile[]>(url, newData);
  }

  RenameDirectory(path: string, directoryName: string, directoryLevel: number) {
    const url = '../api/mdfiles/RenameDirectory';
    var newData = {
      directoryPath: path,
      directoryName: directoryName,
      directoryLevel: directoryLevel,
    }
    return this.http.post<MdFile[]>(url, newData);
  }
  pasteFromClipboard(node: any) {
    const url = '../api/mdfiles/pasteFromClipboard';
    return this.http.post<any>(url, node);
  }


  addExistingFileToMDEProject(node: MdFile,path:String) {
    const url = '../api/mdfiles/addExistingFileToMDEProject';
    return this.http.post<string>(url, { mdFile: node, fullPath:path });
  }

  getTextFromClipboard() {
    const url = '../api/mdfiles/getTextFromClipboard';
    return this.http.get<any>(url)    
  }

  


  cloneTimerDocument(node: MdFile) {
    const url = '../api/mdfiles/CloneTimerMd';
    return this.http.post<MdFile[]>(url, node);
  }

  CreateNewMd(path: string, title: string, directoryLevel: number, documentTypeId: number, documentType: string) {
    const url = '../api/mdfiles/CreateNewMd';
    var newData = {
      directoryPath: path,
      title: title,
      directoryLevel: directoryLevel,
      documentTypeId: documentTypeId,
      documentType: documentType
    }
    return this.http.post<MdFile[]>(url, newData);
  }


  //fileFoundMd: boolean = false;


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

  setSelectedDirectoryFromNewDirectory(selectedDirectory: MdFile) {
    this._selectedDirectoryFromNewDirectory.next(selectedDirectory);
  }
   
  setSelectedMdFileFromToolbar(selectedFile: MdFile) {
    let returnFound = this.searchMdFileIntoDataStore(this.dataStore.mdFiles, selectedFile);
    this._selectedMdFileFromToolbar.next(returnFound);
  }

  setSelectedMdFileFromServer(selectedFile: MdFile) {    
    var returnFound = this.searchMdFileIntoDataStore(this.dataStore.mdFiles, selectedFile);
    this._serverSelectedMdFile.next(returnFound);
  }

  setSelectionMdFile(selectedFile: MdFile[]) {
    this._serverSelectedMdFile.next(selectedFile);
  }


  getMdFileFromDataStore(selectedFile: MdFile): MdFile {
    var returnFound = this.searchMdFileIntoDataStore(this.dataStore.mdFiles, selectedFile);
    return returnFound[0];
  }

  searchMdFileIntoDataStore(arrayMd: MdFile[], FileToFind: MdFile): MdFile[] {
    //this.fileFoundMd = false;
    var arrayFound: MdFile[] = [];
    this.recursiveSearch(arrayMd, FileToFind, arrayFound);
    return arrayFound;
  }


  recursiveSearch(arrayMd: MdFile[], fileTofind: MdFile, arrayFound: MdFile[]):boolean {
    if (arrayMd.length == 0) {
      return;
    }
    let found2 = false;
    var thatFile = arrayMd.find(_ => _.fullPath.toLowerCase() == fileTofind.fullPath.toLowerCase());

    if (thatFile == undefined) {
      for (var i = 0; i < arrayMd.length; i++) {
        var _ = arrayMd[i];
        if (!found2) { //this.fileFoundMd
          found2 = this.recursiveSearch(_.childrens, fileTofind, arrayFound);//, newFile
        }
        if (found2) { //this.fileFoundMd
          arrayFound.push(_);
          break;
        }
      }
    } else {
      found2 = true; //this.fileFoundMd
      arrayFound.push(thatFile);
    }
    return found2;
  }

}

export interface INewFileCreated {
  newName: string;
  newPath: string;
  newLevel: number;
  expandable: boolean;
  relativePath: boolean;
}
