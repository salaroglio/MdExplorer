import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { IRefactoringFilesystemEvent } from '../models/irefactoring-filesystem-event';
import { RefactoringFilesystemEvent } from '../models/refactoring-filesystem-event';
import { IRefactoringSourceAction } from '../models/irefactoring-source-action';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MdRefactoringService {

  constructor(private http: HttpClient) {

  }

  getFileEventList() {
    const url = '../api/refactoringfiles/GetRefactoringFileEventList';
    return this.http.get<IRefactoringFilesystemEvent[]>(url);
  }

  getRefactoringSourceActionList(SourceActionId: string) {
    var params = new HttpParams().set('RefactoringSourceActionId', SourceActionId);
    const url = '../api/refactoringfiles/GetRefactoringSourceActionList';
    return this.http.get<IRefactoringSourceAction[]>(url, { params });
  }

  /**
   * La finestra delle regole (nome del file come il titolo): la stessa rinomina dell'albero, quindi link ricalcolati e
   * nessuna sovrascrittura. La risposta ha la forma che la finestra usa.
   */
  renameFileName(data: any): Observable<IChangeFileData> {
    const sep = (data.fullPath || '').includes('\\') ? '\\' : '/';
    return this.http.post<IChangeFileData>('../api/mdfiles/RenameMdFile', {
      sourcePath: data.fullPath + sep + data.fromFileName,
      newName: data.toFileName,
      level: data.level ?? 0,
    });
  }


 

}




export interface IChangeFileData {
  RefactoringSourceActionId: string;
  oldName: string;
  newName: string;
  oldPath: string;
  newPath: string;
  oldLevel: number;
  newLevel: number;
  expandable: boolean;
  relativePath: string;
  
}
