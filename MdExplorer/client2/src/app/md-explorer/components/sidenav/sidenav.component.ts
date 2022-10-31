
import { ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';
import { Router }                                          from '@angular/router';
import { BreakpointObserver, BreakpointState }   from '@angular/cdk/layout';
import { MdFileService }      from '../../services/md-file.service';
import { AppCurrentFolderService } from '../../../services/app-current-folder.service';
import { GITService } from '../../../git/services/gitservice.service';


const SMALL_WIDTH_BREAKPOINT = 720;

@Component({
  selector: 'app-sidenav',
  templateUrl: './sidenav.component.html',
  styleUrls: ['./sidenav.component.scss']
})
export class SidenavComponent implements OnInit {
  
  public sideNavWidth: string = "240px";
  public isScreenSmall: boolean;
  private hooked: boolean = false;
  public classForBorderDiv: string = "border-div";
  public titleProject: string;
  public currentBranch: string = null;

  constructor(
    private breakpointObserver: BreakpointObserver,
    private mdFileService: MdFileService,
    private router: Router,
    private currentFolder: AppCurrentFolderService,
    private ref: ChangeDetectorRef,
    private gitService: GITService
  ) {
    document.addEventListener("mousemove", (event) => {
      if (this.hooked) {
        this.sideNavWidth = event.clientX + "px";
      }
    });
    document.addEventListener("mouseup", (event) => {
      if (this.hooked) {
        this.stopResizeWidth();
      }
    });
    
    this.currentFolder.folderName.subscribe((data: any) => {      
      this.titleProject = data.currentFolder;
    });
    this.currentFolder.loadFolderName();
    this.gitService.getCurrentBranch().subscribe(_ => {
      this.currentBranch = _.name;
    });
  }

  resizeWidth(): void {    
    this.hooked = true;
    this.classForBorderDiv = "border-div-moving";
  }

  stopResizeWidth(): void {    
    this.hooked = false;
    this.classForBorderDiv = "border-div";
  }

  openProject(): void {    
    this.router.navigate(['/projects']);
  }


  ngOnInit(): void {
    
    this.breakpointObserver.observe([`(max-width:${SMALL_WIDTH_BREAKPOINT}px)`])
      .subscribe((state: BreakpointState) => {
        this.isScreenSmall = state.matches
      });

   
    this.mdFileService.whatDisplayForToolbar.subscribe(_ => {
      
      if ( (_ == 'showToolbar' && this.whatClass != _) ||
        (_ == 'hideToolbar' && this.whatClass != _ + ' ' + 'hideToolbarNone') 
        //|| (_ == 'hideToolbar' + ' ' + 'hideToolbarNone' && this.whatClass != _ + ' ' + 'hideToolbarNone')
      ) { // check if something is truely changed
        this.whatClass = _;
        this.sleep(300).then(m => {
          if (_ == 'hideToolbar' && _ != undefined) {
            this.whatClass = _ + ' ' + 'hideToolbarNone'
            this.ref.detectChanges();
          }
        });
        this.ref.detectChanges();
      }
    });
  }
  sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
  }

  public whatClass: string = "showToolbar";
  

}


