
import { AfterViewInit, ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';
import { Router }                                          from '@angular/router';
import { BreakpointObserver, BreakpointState }   from '@angular/cdk/layout';
import { MdFileService }      from '../../services/md-file.service';
import { AppCurrentMetadataService } from '../../../services/app-current-metadata.service';
import { GITService } from '../../../git/services/gitservice.service';
import { MatSidenav } from '@angular/material/sidenav';


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
  @ViewChild('sidenav') sidenav: MatSidenav;

  constructor(
    private breakpointObserver: BreakpointObserver,
    private mdFileService: MdFileService,
    private router: Router,
    private currentFolder: AppCurrentMetadataService,
    
    private gitService: GITService,    
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
  }



  

}


