import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface IndexingState {
  isIndexed: boolean;
  indexingStatus: 'idle' | 'indexing' | 'completed';
}

@Injectable({
  providedIn: 'root'
})
export class IndexingStateService {
  private indexedFilesSubject = new BehaviorSubject<Map<string, IndexingState>>(new Map());
  
  indexedFiles$ = this.indexedFilesSubject.asObservable();

  /**
   * Verifica se un file è indicizzato
   */
  isFileIndexed(fullPath: string): boolean {
    const state = this.indexedFilesSubject.value.get(fullPath);
    return state?.isIndexed || false;
  }

  /**
   * Verifica se un file è in attesa di indicizzazione (markdown non indicizzato)
   */
  isFileWaiting(fullPath: string, fileType: string): boolean {
    const isMarkdownFile = fileType === 'mdFile' || fileType === 'mdFileTimer';
    const isIndexed = this.isFileIndexed(fullPath);
    return isMarkdownFile && !isIndexed;
  }

  /**
   * Aggiorna lo stato di indicizzazione di un file
   */
  updateFileState(fullPath: string, state: Partial<IndexingState>): void {
    const currentMap = new Map(this.indexedFilesSubject.value);
    const existingState = currentMap.get(fullPath) || { isIndexed: false, indexingStatus: 'idle' };
    
    currentMap.set(fullPath, { ...existingState, ...state });
    this.indexedFilesSubject.next(currentMap);
  }

  /**
   * Gestisce il rename di un file (rimuove vecchia chiave, aggiunge nuova)
   */
  handleFileRename(oldPath: string, newPath: string): void {
    const currentMap = new Map(this.indexedFilesSubject.value);
    const state = currentMap.get(oldPath);
    
    if (state) {
      currentMap.delete(oldPath);
      currentMap.set(newPath, state);
      this.indexedFilesSubject.next(currentMap);
    }
  }

  /**
   * Marca un file come indicizzato (utilizzato per Rule #1 fix)
   */
  markAsIndexed(fullPath: string): void {
    this.updateFileState(fullPath, { isIndexed: true, indexingStatus: 'completed' });
  }
}