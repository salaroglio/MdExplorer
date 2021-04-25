import { Component, OnInit } from '@angular/core';
import { BreakpointObserver, Breakpoints, BreakpointState } from '@angular/cdk/layout';
import { MdFileService } from '../../services/md-file.service';
import { Observable } from 'rxjs';
import { MdFile } from '../../models/md-file';

const SMALL_WIDTH_BREAKPOINT = 720;

@Component({
  selector: 'app-sidenav',
  templateUrl: './sidenav.component.html',
  styleUrls: ['./sidenav.component.scss']
})
export class SidenavComponent implements OnInit {

  mdFiles: Observable<MdFile[]>;

  public isScreenSmall: boolean;
  constructor(private breakpointObserver: BreakpointObserver
    , private mdFileService: MdFileService
  ) { }

  ngOnInit(): void {
    this.breakpointObserver.observe([`(max-width:${SMALL_WIDTH_BREAKPOINT}px)`])
      .subscribe((state: BreakpointState) => {
        this.isScreenSmall = state.matches
      });

    this.mdFiles = this.mdFileService.mdFiles;
    this.mdFileService.loadAll();

    this.mdFiles.subscribe(data => {
      console.log(data);
    });
  }

}
