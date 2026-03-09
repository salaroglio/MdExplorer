import { Component, OnInit, OnDestroy } from '@angular/core';
import { MatLegacyDialog as MatDialog } from '@angular/material/legacy-dialog';
import { versionInfo } from '../../../environments/version';
import { MdNavigationService } from '../../md-explorer/services/md-navigation.service';
import { MdFileService } from '../../md-explorer/services/md-file.service';
import { Router } from '@angular/router';
import { MdServerMessagesService } from '../../signalR/services/server-messages.service';
import { MdFile } from '../../md-explorer/models/md-file';
import { UnifiedSettingsDialogComponent } from '../unified-settings-dialog/unified-settings-dialog.component';
import { AppStoreSettingsDialogComponent } from '../app-store-settings-dialog/app-store-settings-dialog.component';

@Component({
  selector: 'app-title-bar',
  templateUrl: './title-bar.component.html',
  styleUrls: ['./title-bar.component.scss']
})
export class TitleBarComponent implements OnInit, OnDestroy {
  isElectron = false;
  version = versionInfo.version;
  buildTime = versionInfo.buildTime;
  isProjectOpened = false;
  isProjectsPage = false;

  // RAG indexing progress
  ragIndexing = false;
  ragProcessed = 0;
  ragTotal = 0;
  ragProgressPercent = 0;
  ragMessage = '';
  private ragDismissTimer: any = null;

  constructor(
    public navService: MdNavigationService,
    private mdFileService: MdFileService,
    private router: Router,
    private monitorMDService: MdServerMessagesService,
    private dialog: MatDialog
  ) {
    // Check if running in Electron
    this.isElectron = !!(window && (window as any).electronAPI);
  }

  ngOnInit(): void {
    // Subscribe to route changes to detect if we're in a project
    this.router.events.subscribe(() => {
      this.isProjectOpened = this.router.url.startsWith('/main');
      this.isProjectsPage = this.router.url === '/projects' || this.router.url.startsWith('/projects/');
    });

    // Initialize on first load
    this.isProjectOpened = this.router.url.startsWith('/main');
    this.isProjectsPage = this.router.url === '/projects' || this.router.url.startsWith('/projects/');

    // Subscribe to document navigation events from iframe links
    this.monitorMDService.addDocumentNavigatedListener(this.onDocumentNavigated, this);

    // Subscribe to RAG indexing progress
    this.monitorMDService.ragIndexingProgress$.subscribe(data => {
      if (data.status === 'completed' || data.status === 'error') {
        this.ragMessage = data.status === 'completed' ? 'Completed' : 'Error';
        this.ragProcessed = data.processed;
        this.ragTotal = data.total;
        this.ragProgressPercent = 100;
        // Keep visible for 3 seconds then dismiss
        this.ragDismissTimer = setTimeout(() => {
          this.ragIndexing = false;
          this.ragMessage = '';
        }, 3000);
      } else {
        // Clear dismiss timer if new indexing starts
        if (this.ragDismissTimer) {
          clearTimeout(this.ragDismissTimer);
          this.ragDismissTimer = null;
        }
        this.ragIndexing = true;
        this.ragProcessed = data.processed;
        this.ragTotal = data.total;
        this.ragProgressPercent = data.total > 0 ? Math.round((data.processed / data.total) * 100) : 0;
        this.ragMessage = data.message || '';
      }
    });
  }

  ngOnDestroy(): void {
    if (this.ragDismissTimer) {
      clearTimeout(this.ragDismissTimer);
    }
  }

  private onDocumentNavigated(data: any, objectThis: TitleBarComponent): void {
    console.log('[TitleBar] Document navigated event received:', data);

    // Create MdFile object from the data
    const mdFile: MdFile = {
      name: data.name,
      path: data.relativePath,
      relativePath: data.relativePath,
      fullPath: data.fullPath,
      fullDirectoryPath: data.fullDirectoryPath,
      level: 0,
      expandable: false,
      type: 'file',
      index: 0,
      isLoading: false,
      childrens: []
    };

    // Add to navigation history
    objectThis.navService.setNewNavigation(mdFile);
    // Update the selected file so other components (like React editor) know the current file
    objectThis.mdFileService.setSelectedMdFileFromSideNav(mdFile);
    console.log('[TitleBar] Added to navigation history:', mdFile);
    console.log('[TitleBar] Navigation stack:', objectThis.navService.navigation);
  }

  backward(): void {
    console.log('[TitleBar] backward() called');
    console.log('[TitleBar] navigation before:', this.navService.navigation);
    console.log('[TitleBar] navigationGhost before:', this.navService.navigationGhost);
    console.log('[TitleBar] currentIndex before:', this.navService.currentIndex);
    
    const navToMdFile = this.navService.back();
    console.log('[TitleBar] navToMdFile returned:', navToMdFile);
    
    console.log('[TitleBar] Navigating to route: /main/navigation/document');
    this.router.navigate(['/main/navigation/document']);
    
    console.log('[TitleBar] Calling setSelectedMdFileFromSideNav with:', navToMdFile);
    this.mdFileService.setSelectedMdFileFromSideNav(navToMdFile);
    
    console.log('[TitleBar] navigation after:', this.navService.navigation);
    console.log('[TitleBar] navigationGhost after:', this.navService.navigationGhost);
    console.log('[TitleBar] currentIndex after:', this.navService.currentIndex);
  }

  forward(): void {
    console.log('[TitleBar] forward() called');
    console.log('[TitleBar] navigation before:', this.navService.navigation);
    console.log('[TitleBar] navigationGhost before:', this.navService.navigationGhost);
    console.log('[TitleBar] currentIndex before:', this.navService.currentIndex);
    
    const navToMdFile = this.navService.forward();
    console.log('[TitleBar] navToMdFile returned:', navToMdFile);
    
    console.log('[TitleBar] Navigating to route: /main/navigation/document');
    this.router.navigate(['/main/navigation/document']);
    
    console.log('[TitleBar] Calling setSelectedMdFileFromSideNav with:', navToMdFile);
    this.mdFileService.setSelectedMdFileFromSideNav(navToMdFile);
    
    console.log('[TitleBar] navigation after:', this.navService.navigation);
    console.log('[TitleBar] navigationGhost after:', this.navService.navigationGhost);
    console.log('[TitleBar] currentIndex after:', this.navService.currentIndex);
  }

  minimizeWindow(): void {
    if (this.isElectron && (window as any).electronAPI) {
      (window as any).electronAPI.minimizeWindow();
    }
  }

  maximizeWindow(): void {
    if (this.isElectron && (window as any).electronAPI) {
      (window as any).electronAPI.maximizeWindow();
    }
  }

  closeWindow(): void {
    if (this.isElectron && (window as any).electronAPI) {
      (window as any).electronAPI.closeWindow();
    }
  }

  openSettings(tab: string = 'application'): void {
    this.dialog.open(UnifiedSettingsDialogComponent, {
      width: '900px',
      maxHeight: '80vh',
      data: { initialTab: tab }
    });
  }

  openAppStoreSettings(): void {
    this.dialog.open(AppStoreSettingsDialogComponent, {
      width: '700px',
      maxHeight: '80vh'
    });
  }

  navigateToProjects(): void {
    this.router.navigate(['/projects']);
  }

  navigateToAppStore(): void {
    this.router.navigate(['/main/app-store']);
  }
}
