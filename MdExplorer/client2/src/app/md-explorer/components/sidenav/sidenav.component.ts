import { Component, OnInit } from '@angular/core';
import { FlatTreeControl } from '@angular/cdk/tree';
import { MatTreeFlatDataSource, MatTreeFlattener } from '@angular/material/tree';
import { BreakpointObserver, Breakpoints, BreakpointState } from '@angular/cdk/layout';
import { MdFileService } from '../../services/md-file.service';
import { Observable } from 'rxjs';
import { MdFile } from '../../models/md-file';
import { IFileInfoNode } from '../../models/IFileInfoNode';
import { Router, ActivatedRoute } from '@angular/router';
import { SideNavDataService } from '../../services/side-nav-data.service';
import { MatDialog } from '@angular/material/dialog';
import { AppCurrentFolderService } from '../../../services/app-current-folder.service';

const SMALL_WIDTH_BREAKPOINT = 720;

const TREE_DATA: IFileInfoNode[] =[];

/** Flat node with expandable and level information */
interface IFlatNode {
  expandable: boolean;
  name: string;
  level: number;
  path: string;
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
  private activeNode: any;

  private hooked: boolean = false;

  public sideNavWidth: string = "240px";
  public classForBorderDiv: string = "border-div";
  public titleProject: string;

  constructor(private breakpointObserver: BreakpointObserver,
    private mdFileService: MdFileService,
    public dialog: MatDialog,
    private router: Router,
    private sideNavDataService: SideNavDataService,
    private route: ActivatedRoute,
    private currentFolder: AppCurrentFolderService
  ) {
    this.dataSource.data = TREE_DATA;
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
    this.mdFileService.serverSelectedMdFile.subscribe(_ => {
      debugger;

      const myClonedArray = [];
      _.forEach(val => myClonedArray.push(Object.assign({}, val)));

      while (myClonedArray.length > 1) {
        var toExpand = myClonedArray.pop();
        var test = this.treeControl.dataNodes.find(_=>_.path == toExpand.path) ;
        
        this.treeControl.expand(test);
      }      
      if (myClonedArray.length >0) {
        var toExpand = myClonedArray.pop();
        this.activeNode = this.treeControl.dataNodes.find(_ => _.path == toExpand.path);
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

  deferredOpenProject(data, objectThis): void {
    
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
    this.mdFileService.loadAll(this.deferredOpenProject,this); 
    
  }

  public getNode(node: any) {    
    var dateTime = new Date();
    this.sideNavDataService.currentPath = node.path;
    this.sideNavDataService.currentName = node.name;
    this.router.navigate(['main/navigation', dateTime.getTime()]); //, { relativeTo: this.route }
    this.activeNode = node;
  }
 

}
