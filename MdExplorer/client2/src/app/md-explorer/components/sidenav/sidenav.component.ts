
import { AfterViewInit, ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';
import { Router }                                          from '@angular/router';
import { BreakpointObserver, BreakpointState }   from '@angular/cdk/layout';
import { MdFileService }      from '../../services/md-file.service';
import { AppCurrentMetadataService } from '../../../services/app-current-metadata.service';
import { GITService } from '../../../git/services/gitservice.service';
import { MatSidenav } from '@angular/material/sidenav';
import { MdFile } from '../../models/md-file';
import { BookmarksService } from '../../services/bookmarks.service';
import { ProjectsService } from '../../services/projects.service';
import { Bookmark } from '../../services/Types/Bookmark';
import { MdNavigationService } from '../../services/md-navigation.service';



const SMALL_WIDTH_BREAKPOINT = 720;

@Component({
  selector: 'app-sidenav',
  templateUrl: './sidenav.component.html',
  styleUrls: ['./sidenav.component.scss'],
 
})
export class SidenavComponent implements OnInit {
 

  public showText:boolean = false;
  public sideNavWidth: string = "240px";
  public isScreenSmall: boolean;
  private hooked: boolean = false;
  public classForBorderDiv: string = "border-div";
  public titleProject: string;
  public currentBranch: string = null;
  public bookmarks: MdFile[]
  @ViewChild('sidenav') sidenav: MatSidenav;

  constructor(
    private breakpointObserver: BreakpointObserver,
    private mdFileService: MdFileService,    
    private router: Router,
    private currentFolder: AppCurrentMetadataService,
    private bookmarksService:BookmarksService,
    private gitService: GITService,
    private projectService: ProjectsService,
    public navService:MdNavigationService,
  ) {
    document.addEventListener("mousemove", (event) => {
      if (this.hooked) {
        this.sideNavWidth = event.clientX + "px";
      }
    });
    document.addEventListener("mouseup", (event) => {
      console.log(this.hooked);
      
      if (this.hooked) {
        
        //this.stopResizeWidth();
        this.hooked = false;
        this.classForBorderDiv = "border-div";
        this.projectService.currentProjects$.value.sidenavWidth = event.clientX;
        this.projectService.SetSideNavWidth(this.projectService.currentProjects$.value); 
       
      }
    });
    
    this.currentFolder.folderName.subscribe((data: any) => {      
      this.titleProject = data.currentFolder;
    });
    this.currentFolder.loadFolderName();
    this.gitService.getCurrentBranch().subscribe(_ => {
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

  resizeWidth(): void {    
    this.hooked = true;
    this.classForBorderDiv = "border-div-moving";
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
        this.isScreenSmall = state.matches
      });
    this.bookmarksService.bookmarks$.subscribe(_ => this.bookmarks = _);
    
    this.projectService.currentProjects$.subscribe(_ => {      
      if (_ != null && _ != undefined) {
        
        this.bookmarksService.initBookmark(_.id);
        if (_.sidenavWidth != null && _.sidenavWidth != 0) {
          this.sideNavWidth = _.sidenavWidth + "px";
        }
        
      }
    });
    
  }

  openDocument(bookmark: MdFile) {    
    let mdfile = this.mdFileService.getMdFileFromDataStore(bookmark);
    
    this.router.navigate(['/main/navigation/document']);
    this.mdFileService.setSelectedMdFileFromSideNav(mdfile);
  }

  toggleBookmark(mdFile: MdFile) {
    let bookmark: Bookmark = new Bookmark(mdFile);
    bookmark.projectId = this.projectService.currentProjects$.value.id;    
    this.bookmarksService.toggleBookmark(bookmark);
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

}


