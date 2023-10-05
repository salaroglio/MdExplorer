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
      debugger;
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
    //const url = '../api/mdFiles/ToggleBookmark';
    //let post$ = this.http.post<any>(url, bookmark);    
    //post$.subscribe(_ => {
    //  this.bookmarks$.next(_);
    //});

    let currentBookmarks = this.bookmarks$.value;
    let currentBookmark = currentBookmarks.find(_ => _.path === bookmark.path);
    if (currentBookmark == null || currentBookmark == undefined) {
      // call api
      currentBookmarks.push(bookmark);
      this.bookmarks$.next(currentBookmarks);
    } else {
      // call api
      let currentBookmarkIndex = currentBookmarks.indexOf(currentBookmark);
      currentBookmarks.splice(currentBookmarkIndex, 1);
    }
  }
}


