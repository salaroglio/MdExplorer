import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
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

  getRefactoringSourceActionList() {    
    const url = '../api/refactoringfiles/GetRefactoringSourceActionList';
    return this.http.get<IRefactoringSourceAction[]>(url);
  }

  renameFileName(data: any): Observable<IChangeFileData> {
    const url = '../api/refactoringfiles/RenameFileName';
    var newData = {
      message: data.message,
      fromFileName: data.fromFileName,
      toFileName: data.toFileName,
      fullPath: data.fullPath,
      relativePath:data.relativePath,
    }
    return this.http.post<IChangeFileData>(url, newData );
  }

}


export interface IChangeFileData {
  oldName: string;
  newName: string;
  oldPath: string;
  newPath: string;
  oldLevel: number;
  newLevel: number;
  expandable: boolean;
  relativePath: boolean;
  
}
