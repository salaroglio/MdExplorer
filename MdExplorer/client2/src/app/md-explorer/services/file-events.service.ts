import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

export interface FileRenamedEvent {
  oldPath: string;
  newPath: string;
  isRule1Rename: boolean;
}

export interface FileIndexedEvent {
  fullPath: string;
  isIndexed: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class FileEventsService {
  private fileRenamedSubject = new Subject<FileRenamedEvent>();
  private fileIndexedSubject = new Subject<FileIndexedEvent>();

  fileRenamed$ = this.fileRenamedSubject.asObservable();
  fileIndexed$ = this.fileIndexedSubject.asObservable();

  /**
   * Emette evento di file rinominato
   */
  emitFileRenamed(event: FileRenamedEvent): void {
    this.fileRenamedSubject.next(event);
  }

  /**
   * Emette evento di file indicizzato
   */
  emitFileIndexed(event: FileIndexedEvent): void {
    this.fileIndexedSubject.next(event);
  }
}