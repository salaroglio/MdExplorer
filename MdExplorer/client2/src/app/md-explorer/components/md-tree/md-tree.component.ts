import { FlatTreeControl } from '@angular/cdk/tree';
import { Component, OnInit, ViewChild } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatMenuTrigger } from '@angular/material/menu';
import { MatTreeFlatDataSource, MatTreeFlattener } from '@angular/material/tree';
import { Observable } from 'rxjs';
import { IFileInfoNode } from '../../models/IFileInfoNode';
import { MdFile } from '../../models/md-file';
import { MdFileService } from '../../services/md-file.service';
import { NewDirectoryComponent } from '../dialogs/new-directory/new-directory.component';
import { NewMarkdownComponent } from '../dialogs/new-markdown/new-markdown.component';

const TREE_DATA: IFileInfoNode[] = [];

@Component({
  selector: 'app-md-tree',
  templateUrl: './md-tree.component.html',
  styleUrls: ['./md-tree.component.scss']
})
export class MdTreeComponent implements OnInit {
  
  private hooked: boolean = false;
  private activeNode: any;

  menuTopLeftPosition = { x: 0, y: 0 }
  @ViewChild(MatMenuTrigger, { static: true }) matMenuTrigger: MatMenuTrigger;

  

  private _transformer = (node: IFileInfoNode, level: number) => {
    return {
      expandable: (!!node.childrens && node.childrens.length > 0) || node.type == "folder",
      name: node.name,
      level: level,
      path: node.path,
      relativePath: node.path,
      fullPath: node.fullPath,
      type: node.type,
    };
  }
  treeControl = new FlatTreeControl<IFileInfoNode>(
    node => node.level, node => node.expandable);

  treeFlattener = new MatTreeFlattener(
    this._transformer, node => node.level, node => node.expandable, node => node.childrens);

  dataSource = new MatTreeFlatDataSource(this.treeControl, this.treeFlattener);

  hasChild = (_: number, node: IFileInfoNode) => node.expandable;

  isFolder = (_: number, node: IFileInfoNode) => node.type == "folder";

  ///////////////////////////////
  mdFiles: Observable<MdFile[]>;


  constructor(
    private mdFileService: MdFileService,
    public dialog: MatDialog,
  ) {
    this.dataSource.data = TREE_DATA;
    
    this.mdFileService.serverSelectedMdFile.subscribe(_ => {

      const myClonedArray = [];
      _.forEach(val => myClonedArray.push(Object.assign({}, val)));

      while (myClonedArray.length > 1) {
        var toExpand = myClonedArray.pop();
        var test = this.treeControl.dataNodes.find(_ => _.path == toExpand.path);

        this.treeControl.expand(test);
      }
      if (myClonedArray.length > 0) {
        var toExpand = myClonedArray.pop();
        this.activeNode = this.treeControl.dataNodes.find(_ => _.path == toExpand.path);
      }
    });
  }

  ngOnInit(): void {
    this.mdFiles = this.mdFileService.mdFiles;
    this.mdFileService.mdFiles.subscribe(data => {
      this.dataSource.data = data;
    });
    this.mdFileService.loadAll(this.deferredOpenProject, this);

  }

  deferredOpenProject(data, objectThis): void {

  }

  onRightClick(event: MouseEvent, item) {
    // preventDefault avoids to show the visualization of the right-click menu of the browser
    event.preventDefault();

    // we record the mouse position in our object
    this.menuTopLeftPosition.x = event.clientX;
    this.menuTopLeftPosition.y = event.clientY;

    // we open the menu
    // we pass to the menu the information about our object
    this.matMenuTrigger.menuData = { item: item }

    // we open the menu
    this.matMenuTrigger.openMenu();

  }

  public getNode(node: MdFile) {
    this.mdFileService.setSelectedMdFileFromSideNav(node);
    this.activeNode = node;
  }

  // Manu management

  createMdOn(node: MdFile) {
    if (node == null) {
      node = new MdFile("root", "root", 0, false);
      node.fullPath = "root";
    }
    this.dialog.open(NewMarkdownComponent, {
      width: '300px',
      data: node,
    });
  }

  createDirectoryOn(node: MdFile) {
    if (node == null) {
      node = new MdFile("root", "root", 0, false);
      node.fullPath = "root";
    }
    this.dialog.open(NewDirectoryComponent, {
      width: '300px',
      data: node,
    });
  }
}
