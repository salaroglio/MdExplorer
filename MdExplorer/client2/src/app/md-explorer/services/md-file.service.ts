import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { MdFile } from '../models/md-file';
import { IDocumentSettings } from './Types/IDocumentSettings';
import { MdServerMessagesService } from '../../signalR/services/server-messages.service';
import { SpecialFolder, Drive } from '../../commons/components/show-file-system/file-explorer.models';

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
    const url = '../api/mdfiles/MoveMdFile';
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


  // This function adds a new file,
  // looking for the right position in the
  // folder hierarchy.
  // It assumes that all structures are complete,
  // and the only thing to add is the file itself.
  addNewFile(data: MdFile[]) {
    // searching directories    
    const currentItem = data[0];
    
    // Assicuriamoci che le proprietà di indicizzazione siano preservate
    if (currentItem.type === 'mdFile' || currentItem.type === 'mdFileTimer') {
      // Preserva le proprietà esistenti o imposta i default
      currentItem.isIndexed = currentItem.isIndexed ?? true; // Default true per nuovi file
      currentItem.indexingStatus = currentItem.indexingStatus ?? 'completed';
    }
    
    const currentFolder = this.dataStore.mdFiles.find(item => item.fullPath == currentItem.fullPath);

    if (currentFolder) {
      this.recursiveSearchFolder(data, 0, currentFolder);
    } else {
      // The file is in the root
      const dummyItem = this.dataStore.mdFiles.pop();
      this.dataStore.mdFiles.push(currentItem, dummyItem); // Simplified push operation
      this._mdFiles.next({ ...this.dataStore }.mdFiles); // Simplified object cloning and notification
    }
  }


  // This function adds new directories
  // if one or more on the file path are missing.
  // At the end of the process, it will call the classic addNewFile method.
  addNewDirectoryExtended(folders: MdFile[]) {

    let currentfolder = [];
    folders.forEach((folder, index) => {      
      const dataFound: MdFile[] = [];
      this.recursiveSearch(this.dataStore.mdFiles, folder, dataFound);
      currentfolder.push(folder);
      if (dataFound.length === 0) {        
        this.addNewDirectory(currentfolder);
      }
      
    });
      
       
  }

  // This function adds a new directory.
  // Assuming that all directories/folders are already present,
  // and there is just one to add consequently to
  // what already exists in the store.
  addNewDirectory(data: MdFile[]) {
    //alert(JSON.stringify(data, null, 2));
    // Initialize the current item and mark it as expandable
    const currentItem = data[0];
    currentItem.expandable = true;

    // Search for the directory in the current datastore
    const currentFolder = this.dataStore.mdFiles.find(item => item.fullPath == currentItem.fullPath);

    if (currentFolder) {
      // If found, perform a recursive search to insert the directory
      this.recursiveSearchFolder(data, 0, currentFolder);
    } else {
      // If the directory is in the root, handle the dummy item and reinsert
      const dummyItem = this.dataStore.mdFiles.pop(); // Remove the last item (dummy)
      this.dataStore.mdFiles.push(currentItem, dummyItem); // Add the current item and then the dummy back

      // Notify subscribers of the update
      this._mdFiles.next({ ...this.dataStore }.mdFiles);
    }
  }




  recursiveSearchFolder(data: MdFile[], i: number, parentFolder: MdFile) {
    
    const currentItem = data[i + 1];
    if (!currentItem) return; // Guard clause
    
    // Assicuriamoci che le proprietà di indicizzazione siano preservate
    if (currentItem.type === 'mdFile' || currentItem.type === 'mdFileTimer') {
      currentItem.isIndexed = currentItem.isIndexed ?? true;
      currentItem.indexingStatus = currentItem.indexingStatus ?? 'completed';
    }
    
    const currentFolder = parentFolder.childrens.find(folder => folder.fullPath == currentItem.fullPath);

    if (currentFolder) {
      this.recursiveSearchFolder(data, i + 1, currentFolder);
    } else {
      parentFolder.childrens.push(currentItem); // Directly use currentItem
      this._mdFiles.next({ ...this.dataStore }.mdFiles); // Simplified notification
    }
  }





  getShallowStructure(): Observable<MdFile[]> {
    const url = '../api/mdfiles/GetShallowStructure?connectionId=' + this.mdServerMessages.connectionId;
    return this.http.get<MdFile[]>(url);
  }

  loadAll(callback: (data: any, objectThis: any) => any, objectThis: any) {
    const url = '../api/mdfiles/GetShallowStructure?connectionId=' + this.mdServerMessages.connectionId;    
    return this.http.get<MdFile[]>(url)
      .subscribe(data => {
        // Assicuriamo che tutte le proprietà siano definite fin dall'inizio
        this.initializeIndexingProperties(data);
        this.dataStore.mdFiles = data;        
        this._mdFiles.next([...this.dataStore.mdFiles]);       
        if (callback != null) {
          callback(data, objectThis);
        }
      },
        error => {
          console.log("failed to fetch mdfile list");
        });
  }

  private initializeIndexingProperties(nodes: any[]): void {
    nodes.forEach(node => {
      // Assicura che le proprietà esistano fin dall'inizio
      if (node.type === 'mdFile' || node.type === 'mdFileTimer') {
        node.isIndexed = node.isIndexed ?? false;
        node.indexingStatus = node.indexingStatus ?? 'idle';
      }
      if (node.childrens && node.childrens.length > 0) {
        this.initializeIndexingProperties(node.childrens);
      }
    });
  }

  updateFileIndexStatus(path: string, isIndexed: boolean): void {
    // Ricostruisce completamente l'array invece di modificare gli oggetti esistenti
    const updateNodeInArray = (nodes: any[]): any[] => {
      return nodes.map(node => {
        if (node.fullPath === path) {
          // Crea un nuovo oggetto invece di modificare quello esistente
          return {
            ...node,
            isIndexed: isIndexed,
            indexingStatus: isIndexed ? 'completed' : 'idle'
          };
        }
        
        if (node.childrens && node.childrens.length > 0) {
          return {
            ...node,
            childrens: updateNodeInArray(node.childrens)
          };
        }
        
        return node;
      });
    };

    // Ricostruisce completamente l'array
    this.dataStore.mdFiles = updateNodeInArray(this.dataStore.mdFiles);
    
    // Emette il nuovo array
    this._mdFiles.next([...this.dataStore.mdFiles]);
  }

  // Forza aggiornamento stato indicizzazione per file rinominati Rule #1
  forceFileAsIndexed(filePath: string): void {
    this.updateFileIndexStatus(filePath, true);
    
    setTimeout(() => {
      this.mdServerMessages.triggerRule1ForceUpdate(filePath);
    }, 100);
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

  setDevelopmentTags(folder: MdFile, projectRoot: string, tags: string[]) {
    const url = '../api/mdfiles/SetDevelopmentTags';
    return this.http.post(url, {
      folderPath: folder.fullPath,
      projectRoot: projectRoot,
      tags: tags
    });
  }

  openFolderOnFileExplorer(file: MdFile) {
    console.log('[MdFileService] openFolderOnFileExplorer() called');
    console.log('[MdFileService] file:', file);
    console.log('[MdFileService] file.fullPath:', file.fullPath);
    
    const url = '../api/mdfiles/OpenFolderOnFileExplorer';
    console.log('[MdFileService] POST to:', url);
    
    return this.http.post<MdFile>(url, file).pipe(
      tap(response => {
        console.log('[MdFileService] Response received:', response);
      }),
      catchError(error => {
        console.error('[MdFileService] Error in openFolderOnFileExplorer:', error);
        throw error;
      })
    );
  }

  deleteFile(file: MdFile) {
    const url = '../api/mdfiles/DeleteFile';
    return this.http.post<MdFile>(url, file);
      

      //this._mdFiles.next(Object.assign({}, this.dataStore).mdFiles);
    
  }

  //Minimum information to set
  // 1. fullPath:ex: "C:\Users\Carlo\Documents\2-personale\sviluppo\MdExplorer\UnitTestMdExplorer\RockSolidEdition\using-chatGPT\eargaer.md"
  // 2. level: not important

  recursiveDeleteFileFromDataStore(fileToFind: MdFile) {
    
    const dataFound: MdFile[] = [];
    this.recursiveSearch(this.dataStore.mdFiles, fileToFind, dataFound);

    if (dataFound.length === 1) {
      const dataIndex = this.dataStore.mdFiles.indexOf(dataFound[0]);
      if (dataIndex > -1) {
        this.dataStore.mdFiles.splice(dataIndex, 1);
      }
    } if (dataFound.length > 1) {
      //let cursor = this.dataStore.mdFiles;
      let currentFolder: MdFile[] = this.dataStore.mdFiles;
      for (var i = dataFound.length -1 ; i >0 ; i--) {
        currentFolder = currentFolder[currentFolder.indexOf(dataFound[i])].childrens;
      }
      currentFolder.splice(currentFolder.indexOf(dataFound[0]), 1);
    }
    this._mdFiles.next({ ...this.dataStore }.mdFiles); 

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
    console.log('[MdFileService] pasteFromClipboard called with:', node);
    console.log('[MdFileService] Making POST request to:', url);
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
    
    if (!leaf) {
      console.error('❌ [Service] File non trovato nel datastore:', oldFile.name);
      return;
    }
    
    // Aggiorna le proprietà del file
    leaf.name = newFile.name;
    leaf.fullPath = newFile.fullPath;
    leaf.path = newFile.path;
    leaf.relativePath = newFile.relativePath;
    
    // Per file rinominati via Rule #1, forza come indicizzato
    leaf.isIndexed = true;
    leaf.indexingStatus = 'completed';
    
    // Forza nuova referenza per triggerare OnPush change detection
    this._mdFiles.next([...this.dataStore.mdFiles]);
    this._serverSelectedMdFile.next([...returnFound]);
    
    // Notifica il tree component per aggiornare il Set di tracking
    this.mdServerMessages.triggerRule1ForceUpdate(leaf.fullPath);
  }

  setSelectedMdFileFromSideNav(selectedFile: MdFile) {
    console.log('[MdFileService] setSelectedMdFileFromSideNav called with:', selectedFile);
    console.log('[MdFileService] _selectedMdFileFromSideNav value before:', this._selectedMdFileFromSideNav.value);
    console.log('[MdFileService] _selectedMdFileFromSideNav has observers:', this._selectedMdFileFromSideNav.observers?.length || 0);
    this._selectedMdFileFromSideNav.next(selectedFile);
    console.log('[MdFileService] _selectedMdFileFromSideNav value after:', this._selectedMdFileFromSideNav.value);
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


  recursiveSearch(arrayMd: MdFile[], fileToFind: MdFile, arrayFound: MdFile[]): boolean {
    if (arrayMd.length === 0) {
      return false;
    }    
    const thatFile = arrayMd.find(item => item.fullPath.toLowerCase() === fileToFind.fullPath.toLowerCase());

    if (!thatFile) {
      return arrayMd.some(item => {
        const found = this.recursiveSearch(item.childrens, fileToFind, arrayFound);
        if (found) {
          arrayFound.push(item);
        }
        return found;
      });
    } else {
      arrayFound.push(thatFile);
      return true;
    }
  }

  // New methods for file explorer functionality
  getSpecialFolders(): Observable<SpecialFolder[]> {
    const url = '../api/mdfiles/GetSpecialFolders';
    return this.http.get<SpecialFolder[]>(url);
  }

  getDrives(): Observable<Drive[]> {
    const url = '../api/mdfiles/GetDrives';
    return this.http.get<Drive[]>(url);
  }

  getNetworkShares(): Observable<any[]> {
    const url = '../api/mdfiles/GetNetworkShares';
    return this.http.get<any[]>(url);
  }


}

export interface INewFileCreated {
  newName: string;
  newPath: string;
  newLevel: number;
  expandable: boolean;
  relativePath: boolean;
}
