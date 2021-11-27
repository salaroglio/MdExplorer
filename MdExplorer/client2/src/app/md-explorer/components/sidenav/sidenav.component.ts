import { Component, OnInit } from '@angular/core';
import { FlatTreeControl } from '@angular/cdk/tree';
import { MatTreeFlatDataSource, MatTreeFlattener } from '@angular/material/tree';

import { BreakpointObserver, Breakpoints, BreakpointState } from '@angular/cdk/layout';
import { MdFileService } from '../../services/md-file.service';
import { Observable } from 'rxjs';
import { MdFile } from '../../models/md-file';
import { IFileInfoNode } from '../../models/IFileInfoNode';
import { MonitorMDService } from '../../services/monitor-md.service';
import { Router, ActivatedRoute } from '@angular/router';
import { debug } from 'node:console';
import { getLocaleDateTimeFormat } from '@angular/common';
import { MainContentComponent } from '../main-content/main-content.component';
import { SideNavDataService } from '../../services/side-nav-data.service';
import { MatDialog } from '@angular/material/dialog';
import { ProjectsComponent } from '../projects/projects.component';

const SMALL_WIDTH_BREAKPOINT = 720;

const TREE_DATA: IFileInfoNode[] =[];

/** Flat node with expandable and level information */
interface IFlatNode {
  expandable: boolean;
  name: string;
  level: number;
}

@Component({
  selector: 'app-sidenav',
  templateUrl: './sidenav.component.html',
  styleUrls: ['./sidenav.component.scss']
})
export class SidenavComponent implements OnInit {

  private _transformer = (node: IFileInfoNode, level: number) => {
    return {
      expandable: !!node.childrens && node.childrens.length > 0,
      name: node.name,
      level: level,
      path:node.path
    };
  }
  treeControl = new FlatTreeControl<IFlatNode>(
    node => node.level, node => node.expandable);

  treeFlattener = new MatTreeFlattener(
    this._transformer, node => node.level, node => node.expandable, node => node.childrens);

  dataSource = new MatTreeFlatDataSource(this.treeControl, this.treeFlattener);

  hasChild = (_: number, node: IFlatNode) => node.expandable;

  ///////////////////////////////
  mdFiles: Observable<MdFile[]>;

  public isScreenSmall: boolean;
  constructor(private breakpointObserver: BreakpointObserver,
    private mdFileService: MdFileService,
    public dialog: MatDialog,
    private router: Router,
    private sideNavDataService: SideNavDataService
  ) {
    this.dataSource.data = TREE_DATA;
    
  }

  openProject(): void {
    const dialogRef = this.dialog.open(ProjectsComponent, {
      width: '600px',
      data: { name: 'test' }
    });
  }


  ngOnInit(): void {
    this.breakpointObserver.observe([`(max-width:${SMALL_WIDTH_BREAKPOINT}px)`])
      .subscribe((state: BreakpointState) => {
        this.isScreenSmall = state.matches
      });

    this.mdFiles = this.mdFileService.mdFiles;
    this.mdFileService.mdFiles.subscribe(data => {
      this.dataSource.data = data;
    });

    this.mdFileService.loadAll();

    this.mdFiles.subscribe(data => {
      console.log(data);
    });

    
  }

  public getNode(node: any) {
    var dateTime = new Date();
    this.sideNavDataService.currentPath = node.path;
    this.sideNavDataService.currentName = node.name;
    this.router.navigate(['/main', dateTime.getTime()]);
  }
 

}
