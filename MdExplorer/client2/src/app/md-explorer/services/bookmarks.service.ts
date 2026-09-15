import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, combineLatest } from 'rxjs';
import { MdFile } from '../models/md-file';
import { Bookmark } from './Types/Bookmark';
import { HttpClient, HttpParams } from '@angular/common/http';
import { GetBookmarkResponseDto } from './Types/GetBookmarkResponse';
import { MdFileService } from './md-file.service';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class BookmarksService {

  public bookmarks$: BehaviorSubject<Bookmark[]> = new BehaviorSubject<Bookmark[]>([]);

  constructor(private http: HttpClient,
    private mdFileService: MdFileService) { }

  initBookmark(projectId: string): void {
    // passing projectId
    let books: Bookmark[] = [];
    const url = '../api/mdFiles/GetBookmarks';
   
    var params = new HttpParams().set('projectId', projectId)

    let bookmarks$$ = this.http.get<GetBookmarkResponseDto[]>(url, { params });
    let bookmarksWaitingAllMdFiles$ = combineLatest([this.mdFileService._mdFiles, bookmarks$$]);
    bookmarksWaitingAllMdFiles$.subscribe(([mdFiles, bookmarks]) => {
      let mdcheck = mdFiles;      
      bookmarks.forEach(_ => {
        // devo ricostruirmi la lista di mdFile
        let mdFileToSearch = new MdFile(_.name, _.fullPath, null, null);
        let mdfile = this.mdFileService.getMdFileFromDataStore(mdFileToSearch);
        let bookmark = new Bookmark(mdFileToSearch);
        bookmark.fullPath = _.fullPath;
        bookmark.displayName = _.displayName || _.name;
        books.push(bookmark);
      });

      this.bookmarks$.next(books);
    });

  }

  // One-shot reload of the resolved labels (no combineLatest: the md-file tree
  // is already loaded when this runs, right after a toggle).
  private refreshBookmarks(projectId: string): void {
    const url = '../api/mdFiles/GetBookmarks';
    const params = new HttpParams().set('projectId', projectId);
    this.http.get<GetBookmarkResponseDto[]>(url, { params }).subscribe(bookmarks => {
      const books = bookmarks.map(_ => {
        const bookmark = new Bookmark(new MdFile(_.name, _.fullPath, null, null));
        bookmark.fullPath = _.fullPath;
        bookmark.displayName = _.displayName || _.name;
        return bookmark;
      });
      this.bookmarks$.next(books);
    });
  }

  toggleBookmark(bookmark: Bookmark): void {
    const url = '../api/mdFiles/ToggleBookmark';
    
    let post$ = this.http.post<any>(url, bookmark);
    post$.subscribe(_ => {
      // Reload so the new entry gets its server-resolved label
      // (document title + folder disambiguation on duplicates).
      this.refreshBookmarks(bookmark.projectId);
    });

    let currentBookmarks = this.bookmarks$.value;
    let currentBookmark = currentBookmarks.find(_ => _.fullPath === bookmark.fullPath);
    if (currentBookmark == null || currentBookmark == undefined) {
      bookmark.displayName = bookmark.displayName || bookmark.name;
      currentBookmarks.push(bookmark);
      this.bookmarks$.next(currentBookmarks);
    } else {
      let currentBookmarkIndex = currentBookmarks.indexOf(currentBookmark);
      currentBookmarks.splice(currentBookmarkIndex, 1);
      this.bookmarks$.next(currentBookmarks);
    }
  }

  // Persist the new order: the array passed in is already in the desired order.
  reorderBookmarks(projectId: string, orderedBookmarks: Bookmark[]): void {
    // Optimistic UI update (the numbered index in the overlay follows the array order)
    this.bookmarks$.next(orderedBookmarks);

    const url = '../api/mdFiles/ReorderBookmarks';
    const payload = {
      projectId: projectId,
      orderedFullPaths: orderedBookmarks.map(_ => _.fullPath)
    };
    this.http.post<any>(url, payload).subscribe();
  }
}


