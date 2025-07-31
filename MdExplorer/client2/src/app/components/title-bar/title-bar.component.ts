import { Component, OnInit } from '@angular/core';
import { versionInfo } from '../../../environments/version';
import { MdNavigationService } from '../../md-explorer/services/md-navigation.service';
import { MdFileService } from '../../md-explorer/services/md-file.service';
import { Router } from '@angular/router';
import { LayoutService } from '../../md-explorer/services/layout.service';

@Component({
  selector: 'app-title-bar',
  templateUrl: './title-bar.component.html',
  styleUrls: ['./title-bar.component.scss']
})
export class TitleBarComponent implements OnInit {
  isElectron = false;
  version = versionInfo.version;
  isSidenavOpen = true;

  constructor(
    public navService: MdNavigationService,
    private mdFileService: MdFileService,
    private router: Router,
    private layoutService: LayoutService
  ) {
    // Check if running in Electron
    this.isElectron = !!(window && (window as any).electronAPI);
  }

  ngOnInit(): void {
    // Subscribe to sidenav state changes
    this.layoutService.sidenavOpen$.subscribe(isOpen => {
      this.isSidenavOpen = isOpen;
    });
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
}
