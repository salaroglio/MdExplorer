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
        books.push(bookmark);
      });

      this.bookmarks$.next(books);
    });    

  }

  toggleBookmark(bookmark: Bookmark): void {
    console.log('[BookmarksService] toggleBookmark() called');
    console.log('[BookmarksService] Bookmark received:', bookmark);
    console.log('[BookmarksService] Bookmark projectId:', bookmark.projectId);
    console.log('[BookmarksService] Bookmark fullPath:', bookmark.fullPath);
    
    const url = '../api/mdFiles/ToggleBookmark';
    console.log('[BookmarksService] Calling API:', url);
    
    let post$ = this.http.post<any>(url, bookmark);    
    post$.subscribe(
      response => {
        console.log('[BookmarksService] API response:', response);
        //this.bookmarks$.next(_);
      },
      error => {
        console.error('[BookmarksService] API error:', error);
        console.error('[BookmarksService] Error status:', error.status);
        console.error('[BookmarksService] Error message:', error.message);
      }
    );

    let currentBookmarks = this.bookmarks$.value;
    console.log('[BookmarksService] Current bookmarks:', currentBookmarks);
    
    let currentBookmark = currentBookmarks.find(_ => _.fullPath === bookmark.fullPath);
    if (currentBookmark == null || currentBookmark == undefined) {
      console.log('[BookmarksService] Bookmark not found, adding it');
      currentBookmarks.push(bookmark);
      this.bookmarks$.next(currentBookmarks);
    } else {
      console.log('[BookmarksService] Bookmark found, removing it');
      let currentBookmarkIndex = currentBookmarks.indexOf(currentBookmark);
      currentBookmarks.splice(currentBookmarkIndex, 1);
      this.bookmarks$.next(currentBookmarks);
    }
  }
}


