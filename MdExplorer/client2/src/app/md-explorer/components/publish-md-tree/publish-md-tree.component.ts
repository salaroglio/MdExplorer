import { FlatTreeControl } from '@angular/cdk/tree';
import { Component, OnInit, ViewChild } from '@angular/core';
import { CollectionViewer, SelectionChange, DataSource } from '@angular/cdk/collections';
import { MatMenuTrigger } from '@angular/material/menu';
import { Router } from '@angular/router';
import { IFileInfoNode } from '../../models/IFileInfoNode';
import { MdFile } from '../../models/md-file';
import { ProjectsService } from '../../services/projects.service';
import { DynamicDatabase } from '../../../projects/new-project/new-project.component';
import { MdFileService } from '../../services/md-file.service';
import { BehaviorSubject, merge, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { NewMarkdownComponent } from '../dialogs/new-markdown/new-markdown.component';
import { MatDialog } from '@angular/material/dialog';
import { NewDirectoryComponent } from '../dialogs/new-directory/new-directory.component';
import { ChangeDirectoryComponent } from '../dialogs/change-directory/change-directory.component';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Clipboard } from '@angular/cdk/clipboard';
import { DeleteMarkdownComponent } from '../dialogs/delete-markdown/delete-markdown.component';

class DynamicDataSource implements DataSource<MdFile> {

  dataChange = new BehaviorSubject<MdFile[]>([]);

  get data(): MdFile[] { return this.dataChange.value; }
  set data(value: MdFile[]) {
    this._treeControl.dataNodes = value;
    this.dataChange.next(value);
  }

  constructor(private _treeControl: FlatTreeControl<MdFile>,
    private _database: DynamicDatabase,
    private _mdFileService: MdFileService) {
    this.data = _database.initialData();
    this._mdFileService.loadPublishNodes('root', 0).subscribe(_ => {
      debugger;
      this.data = _;
    });
  }

  connect(collectionViewer: CollectionViewer): Observable<MdFile[]> {
    this._treeControl.expansionModel.changed.subscribe(change => {
      if ((change as SelectionChange<MdFile>).added ||
        (change as SelectionChange<MdFile>).removed) {
        this.handleTreeControl(change as SelectionChange<MdFile>);
      }
    });

    return merge(collectionViewer.viewChange, this.dataChange).pipe(map(() => this.data));
  }

  disconnect(collectionViewer: CollectionViewer): void { }

  /** Handle expand/collapse behaviors */
  handleTreeControl(change: SelectionChange<MdFile>) {
    if (change.added) {
      change.added.forEach(node => this.toggleNode(node, true));
    }
    if (change.removed) {
      change.removed.slice().reverse().forEach(node => this.toggleNode(node, false));
    }
  }

  /**
   * Toggle the node, remove from display list
   */
  toggleNode(node: MdFile, expand: boolean) {
    this._mdFileService.loadPublishNodes(node.path, node.level + 1).subscribe(_ => {

      const children = _;
      const index = this.data.indexOf(node);

      if (!children || index < 0) { // If no children, or cannot find the node, no op
        return;
      }

      node.isLoading = true;

      setTimeout(() => {
        if (expand) {
          const nodes = children; // punto per fare chiamata remota
          this.data.splice(index + 1, 0, ...nodes);
        } else {
          let count = 0;
          for (let i = index + 1; i < this.data.length
            && this.data[i].level > node.level; i++, count++) { }
          this.data.splice(index + 1, count);
        }

        // notify the change
        this.dataChange.next(this.data);
        node.isLoading = false;
      });
    });
  }
}


@Component({
  selector: 'app-publish-md-tree',
  templateUrl: './publish-md-tree.component.html',
  styleUrls: ['./publish-md-tree.component.scss']
})
export class PublishMdTreeComponent implements OnInit {
  
  menuTopLeftPosition = { x: 0, y: 0 }
  @ViewChild(MatMenuTrigger, { static: true }) matMenuTrigger: MatMenuTrigger;
  folder: {
    name: string,
    path: string
  }
  
  constructor(private router: Router,
              private database: DynamicDatabase,
              private mdFileService: MdFileService,
              private projectService: ProjectsService,
              public dialog: MatDialog,
              private snackBar: MatSnackBar,
              private clipboard: Clipboard,
              ) {
    this.treeControl = new FlatTreeControl<MdFile>(this.getLevel, this.isExpandable);
    this.dataSource = new DynamicDataSource(this.treeControl, database, mdFileService);
  }


  ngOnInit(): void {
  }
  public getFolder(node: IFileInfoNode) {
    this.folder.name = node.name;
    this.folder.path = node.path;
  }
  activeNode: any;
  treeControl: FlatTreeControl<MdFile>;
  dataSource: DynamicDataSource;
  isFolder = (_: number, node: IFileInfoNode) => node.type == "publishFolder";
  isEmptyRoot = (_: number, node: IFileInfoNode) => node.type == "root";
  getLevel = (node: MdFile) => node.level;
  isExpandable = (node: MdFile) => node.expandable;


  onRightClick(event: MouseEvent, item) {
    // preventDefault avoids to show the visualization of the right-click menu of the browser
    event.preventDefault();
    if (item == null) {
      item = new MdFile("root", "root", 0, false);
      item.fullPath = "root";
    }
    // we record the mouse position in our object
    this.menuTopLeftPosition.x = event.clientX;
    this.menuTopLeftPosition.y = event.clientY;

    // we open the menu
    // we pass to the menu the information about our object
    this.matMenuTrigger.menuData = { item: item }

    // we open the menu
    this.matMenuTrigger.openMenu();

  }

  createMdOn(node: MdFile) {

    this.dialog.open(NewMarkdownComponent, {
      width: '300px',
      //height:'400px',
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

  renameDirectoryOn(node: MdFile) {
    if (node == null) {
      node = new MdFile("root", "root", 0, false);
      node.fullPath = "root";
    }
    this.dialog.open(ChangeDirectoryComponent, {
      width: '300px',
      data: node,
    });
  }

  openFolderOn(node: MdFile) {
    this.mdFileService.openFolderOnFileExplorer(node).subscribe(_ => {
      this.snackBar.open("file explorer open", "", { duration: 500 });
    });
  }

  getLinkFromNode(node: MdFile) {
    this.clipboard.copy(node.relativePath);

  }

  deleteFile(node: MdFile) {
    this.dialog.open(DeleteMarkdownComponent, {
      width: '300px',
      data: node,
    });

  }

  getNode(node: MdFile) {
    this.router.navigate(['/main/navigation/document']);
    this.mdFileService.setSelectedMdFileFromSideNav(node);
    this.activeNode = node;
  }

  openGitlabSettingsFromNode(node: MdFile) {
    this.router.navigate(['/main/navigation/gitlabsettings']);
  }

}
