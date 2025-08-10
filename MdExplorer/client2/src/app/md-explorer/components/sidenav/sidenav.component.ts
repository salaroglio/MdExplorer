
import { AfterViewInit, ChangeDetectorRef, Component, OnInit, OnDestroy, ViewChild, ChangeDetectionStrategy } from '@angular/core'; // Added ChangeDetectionStrategy
import { Router }                                          from '@angular/router';
import { BreakpointObserver, BreakpointState }   from '@angular/cdk/layout';
import { MdFileService }      from '../../services/md-file.service';
import { AppCurrentMetadataService } from '../../../services/app-current-metadata.service';
import { GITService } from '../../../git/services/gitservice.service';
import { MatSidenav } from '@angular/material/sidenav';
import { MdFile } from '../../models/md-file';
import { ProjectsService } from '../../services/projects.service';
import { MdNavigationService } from '../../services/md-navigation.service';
import { LayoutService } from '../../services/layout.service';



const SMALL_WIDTH_BREAKPOINT = 720;

@Component({
  selector: 'app-sidenav',
  templateUrl: './sidenav.component.html',
  styleUrls: ['./sidenav.component.scss'],

})
export class SidenavComponent implements OnInit, OnDestroy {


  public showText:boolean = false;
  public sideNavWidth: string = "240px";
  public isScreenSmall: boolean;
  public hooked: boolean = false;
  public titleProject: string;
  public currentBranch: string = null;
  @ViewChild('sidenav') sidenav: MatSidenav;
  
  // Memory leak prevention
  private mouseMoveListener?: (event: MouseEvent) => void;
  private mouseUpListener?: (event: MouseEvent) => void;
  
  // Performance and validation
  private readonly MIN_WIDTH = 200;
  private readonly MAX_WIDTH = 800;
  private debounceTimer: any;

  constructor(
    private breakpointObserver: BreakpointObserver,
    private mdFileService: MdFileService,
    private router: Router,
    private currentFolder: AppCurrentMetadataService,
    private gitService: GITService,
    private projectService: ProjectsService,
    public navService:MdNavigationService,
    private ref: ChangeDetectorRef, // Injected ChangeDetectorRef
    private layoutService: LayoutService
  ) {
    this.setupResizeListeners();

    this.currentFolder.folderName.subscribe((data: any) => {
      this.titleProject = data.currentFolder;
    });
    this.currentFolder.loadFolderName();
    this.gitService.currentBranch$.subscribe(_ => {
      this.currentBranch = _.name;
    });

    this.currentFolder.showSidenav.subscribe(_ => {
      if (this.sidenav != undefined ) {
        if (_) {
          this.sidenav.open();
        } else {
          this.sidenav.close();
        }
      }
    });

  }

  private setupResizeListeners(): void {
    this.mouseMoveListener = (event: MouseEvent) => {
      if (this.hooked) {
        const newWidth = this.validateWidth(event.clientX);
        this.sideNavWidth = newWidth + "px";
        this.layoutService.setSidenavWidth(newWidth);
      }
    };

    this.mouseUpListener = (event: MouseEvent) => {
      if (this.hooked) {
        this.hooked = false;
        // Restore normal cursor and text selection
        document.body.style.userSelect = '';
        document.body.style.cursor = '';
        const finalWidth = this.validateWidth(event.clientX);
        this.saveWidthDebounced(finalWidth);
      }
    };

    document.addEventListener("mousemove", this.mouseMoveListener);
    document.addEventListener("mouseup", this.mouseUpListener);
  }

  ngOnDestroy(): void {
    // Remove event listeners to prevent memory leaks
    if (this.mouseMoveListener) {
      document.removeEventListener("mousemove", this.mouseMoveListener);
    }
    if (this.mouseUpListener) {
      document.removeEventListener("mouseup", this.mouseUpListener);
    }
    // Clear debounce timer
    clearTimeout(this.debounceTimer);
  }

  private validateWidth(width: number): number {
    return Math.min(Math.max(width, this.MIN_WIDTH), this.MAX_WIDTH);
  }

  private saveWidthDebounced(width: number): void {
    clearTimeout(this.debounceTimer);
    this.debounceTimer = setTimeout(() => {
      this.projectService.currentProjects$.value.sidenavWidth = width;
      this.projectService.SetSideNavWidth(this.projectService.currentProjects$.value);
    }, 500); // Save only after 500ms of inactivity
  }

  resizeWidth(): void {
    this.hooked = true;
    // Prevent text selection during drag
    document.body.style.userSelect = 'none';
    document.body.style.cursor = 'col-resize';
  }

  stopResizeWidth(): void {





  }

  openProject(): void {
    var mdFile = new MdFile("Welcome to MDExplorer", '/../welcome.html',0,false);
    mdFile.relativePath = '/../../welcome.html';
    this.mdFileService.setSelectedMdFileFromSideNav(mdFile);
    this.router.navigate(['/projects']);
    this.projectService.currentProjects$.next(null);
  }


  ngOnInit(): void {

    this.breakpointObserver.observe([`(max-width:${SMALL_WIDTH_BREAKPOINT}px)`])
      .subscribe((state: BreakpointState) => {
        this.isScreenSmall = state.matches;
      });

    this.projectService.currentProjects$.subscribe(_ => {
      if (_ != null && _ != undefined) {
        if (_.sidenavWidth != null && _.sidenavWidth != 0) {
          this.sideNavWidth = _.sidenavWidth + "px";
          this.layoutService.setSidenavWidth(_.sidenavWidth);
        }
      }
    });


  }

  forward(): void {

    let navToMdFile = this.navService.forward();
    this.router.navigate(['/main/navigation/document']);
    this.mdFileService.setSelectedMdFileFromSideNav(navToMdFile);

  }

  backward(): void {

    let navToMdFile = this.navService.back();
    this.router.navigate(['/main/navigation/document']);
    this.mdFileService.setSelectedMdFileFromSideNav(navToMdFile);

  }
  
  onSidenavToggle(isOpen: boolean): void {
    this.layoutService.setSidenavOpen(isOpen);
    
    // Log delle dimensioni dopo un breve delay per permettere l'animazione
    setTimeout(() => {
      const sidenavContent = document.querySelector('mat-sidenav-content') as HTMLElement;
      const documentShow = document.querySelector('app-document-show') as HTMLElement;
      
        sidenavContent: {
          width: sidenavContent?.offsetWidth || 0,
          style: sidenavContent ? window.getComputedStyle(sidenavContent) : null
        },
        documentShow: {
          width: documentShow?.offsetWidth || 0,
          style: documentShow ? window.getComputedStyle(documentShow) : null
        }
      });
    }, 300);
  }

}
