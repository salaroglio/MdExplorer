import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { MdFile } from '../models/md-file';

@Injectable({
  providedIn: 'root'
})
export class BookmarksService {

  public bookmarks$: BehaviorSubject<MdFile[]> = new BehaviorSubject<MdFile[]>([]);

  constructor() {}
  

  toggleBookmark(bookmark: MdFile): void {
    let currentBookmarks = this.bookmarks$.value;
    let currentBookmark = currentBookmarks.find(_ => _.path === bookmark.path);    
    if (currentBookmark == null || currentBookmark == undefined) {
      currentBookmarks.push(bookmark);
      this.bookmarks$.next(currentBookmarks);
    } else {
      let currentBookmarkIndex = currentBookmarks.indexOf(currentBookmark);
      currentBookmarks.splice(currentBookmarkIndex,1);
    }
  }
}

export class MdBookmark {
  id: number
  name: string
  show: boolean
  path:string
}

