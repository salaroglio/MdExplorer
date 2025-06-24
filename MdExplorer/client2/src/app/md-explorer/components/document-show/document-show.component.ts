import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil, filter } from 'rxjs/operators';
import { MdFileService } from '../../services/md-file.service';
import { BookmarksService } from '../../services/bookmarks.service';
import { AppCurrentMetadataService } from '../../../services/app-current-metadata.service';
import { MdNavigationService } from '../../services/md-navigation.service';
import { ProjectsService } from '../../services/projects.service';
import { Router, NavigationEnd } from '@angular/router';
import { MdFile } from '../../models/md-file';
import { Bookmark } from '../../services/Types/Bookmark';

@Component({
  selector: 'app-document-show',
  templateUrl: './document-show.component.html',
  styleUrls: ['./document-show.component.scss']
})
export class DocumentShowComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  
  // Stati necessari per la toolbar e il contenuto
  currentFile$ = this.mdFileService.selectedMdFileFromSideNav;
  bookmarks: Bookmark[] = [];
  
  // Proprietà per gestione bookmark
  topOffsetContent: number = 0;
  
  // Proprietà per gestione toolbar
  classForToolbar: string = "showToolbar";

  constructor(
    private mdFileService: MdFileService,
    private bookmarksService: BookmarksService,
    private appMetadata: AppCurrentMetadataService,
    private navigationService: MdNavigationService,
    private projectService: ProjectsService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Setup delle subscriptions necessarie
    this.setupSubscriptions();
    
    // Aggiungi listener per quando l'iframe è caricato
    this.setupIframeListener();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setupSubscriptions(): void {
    // Sottoscrizione ai bookmark
    this.bookmarksService.bookmarks$
      .pipe(takeUntil(this.destroy$))
      .subscribe(bookmarks => {
        this.bookmarks = bookmarks;
      });
      
    // Sottoscrizione allo stato della toolbar
    this.mdFileService.whatDisplayForToolbar
      .pipe(takeUntil(this.destroy$))
      .subscribe(toolbarState => {
        if ((toolbarState === 'showToolbar' && this.classForToolbar !== toolbarState) ||
            (toolbarState === 'hideToolbar' && this.classForToolbar !== toolbarState + ' ' + 'hideToolbarNone')
        ) {
          this.classForToolbar = toolbarState;
          if (toolbarState === 'hideToolbar') {
            this.classForToolbar = toolbarState + ' ' + 'hideToolbarNone';
          }
        }
      });
      
    // Aggiungi listener per quando viene selezionato un nuovo file
    this.currentFile$
      .pipe(takeUntil(this.destroy$))
      .subscribe(file => {
        if (file) {
          // Aspetta che l'iframe carichi il nuovo contenuto
          setTimeout(() => {
            this.checkAndFixTocVisibility();
          }, 1500);
        }
      });
  }

  // Metodi per gestione bookmark migrati da sidenav
  openDocument(bookmark: Bookmark): void {
    const mdfile = this.mdFileService.getMdFileFromDataStore(bookmark);
    this.router.navigate(['/main/navigation/document']);
    this.mdFileService.setSelectedMdFileFromSideNav(mdfile);
  }

  toggleBookmark(bookmark: Bookmark): void {
    this.bookmarksService.toggleBookmark(bookmark);
  }

  onGetTopOffsetContent(topOffset: number): void {
    this.topOffsetContent = topOffset;
  }
  
  private setupIframeListener(): void {
    // Ascolta quando il router carica il MainContentComponent
    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        // Aspetta che l'iframe sia caricato e poi controlla la visibilità del TOC
        setTimeout(() => {
          this.checkAndFixTocVisibility();
        }, 1000);
      });
  }
  
  private checkAndFixTocVisibility(): void {
    const iframe = document.getElementById('mdIframe') as HTMLIFrameElement;
    
    if (iframe && iframe.contentWindow) {
      try {
        const iframeDoc = iframe.contentDocument || iframe.contentWindow.document;
        
        // Controlla se ci sono elementi TOC che necessitano di aggiustamenti di visibilità
        // (logica specifica per TOC può essere aggiunta qui se necessario)
        
      } catch (e) {
        console.error('Error accessing iframe content:', e);
      }
    }
  }
}