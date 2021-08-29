import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { IRefactoringFilesystemEvent } from '../models/irefactoring-filesystem-event';
import { RefactoringFilesystemEvent } from '../models/refactoring-filesystem-event';

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

}
