import {
  Component,
  OnInit,
  OnDestroy,
  ChangeDetectorRef,
  NgZone
} from '@angular/core';
import { Subscription } from 'rxjs';
import { Router } from '@angular/router';
import { EmbeddedAppStateService, EmbeddedAppEntry } from '../../services/embedded-app-state.service';
import { BookmarksService } from '../../services/bookmarks.service';
import { ProjectsService } from '../../services/projects.service';
import { MdFileService } from '../../services/md-file.service';
import { MdFile } from '../../models/md-file';
import { Bookmark } from '../../services/Types/Bookmark';

@Component({
  selector: 'app-app-show',
  templateUrl: './app-show.component.html',
  styleUrls: ['./app-show.component.scss']
})
export class AppShowComponent implements OnInit, OnDestroy {

  apps: EmbeddedAppEntry[] = [];
  activeAppId: string | null = null;
  bookmarks: Bookmark[] = [];

  private subscriptions: Subscription[] = [];
  private unsubscribeCrashed: (() => void) | null = null;
  private isElectron = !!(window as any).electronAPI?.externalApp;

  constructor(
    private embeddedAppState: EmbeddedAppStateService,
    private bookmarksService: BookmarksService,
    private projectService: ProjectsService,
    private mdFileService: MdFileService,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {}

  ngOnInit(): void {
    this.subscriptions.push(
      this.embeddedAppState.apps$.subscribe(apps => {
        this.apps = apps;
        this.cdr.markForCheck();
      })
    );

    this.subscriptions.push(
      this.embeddedAppState.activeAppId$.subscribe(appId => {
        this.activeAppId = appId;
        this.cdr.markForCheck();
      })
    );

    // Bookmarks
    this.subscriptions.push(
      this.bookmarksService.bookmarks$.subscribe(bookmarks => {
        this.bookmarks = bookmarks;
        this.cdr.markForCheck();
      })
    );

    this.subscriptions.push(
      this.projectService.currentProjects$.subscribe(project => {
        if (project && project.id) {
          this.bookmarksService.initBookmark(project.id);
        }
      })
    );

    // Listen for app crashes
    if (this.isElectron) {
      this.unsubscribeCrashed = (window as any).electronAPI.externalApp.onCrashed(
        (data: { appId: string; exitCode: number }) => {
          this.ngZone.run(() => {
            this.embeddedAppState.unregisterApp(data.appId);
            this.cdr.markForCheck();
          });
        }
      );
    }
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(s => s.unsubscribe());
    if (this.unsubscribeCrashed) this.unsubscribeCrashed();
  }

  openDocument(bookmark: Bookmark): void {
    const mdfile = this.mdFileService.getMdFileFromDataStore(bookmark);
    this.embeddedAppState.deactivate();
    this.router.navigate(['/main/navigation/document']);
    this.mdFileService.setSelectedMdFileFromSideNav(mdfile);
  }

  toggleBookmark(bookmark: Bookmark): void {
    const currentProject = this.projectService.currentProjects$.value;
    if (currentProject && currentProject.id) {
      bookmark.projectId = currentProject.id;
      this.bookmarksService.toggleBookmark(bookmark);
    }
  }
}
