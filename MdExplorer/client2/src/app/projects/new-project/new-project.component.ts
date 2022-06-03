import { FlatTreeControl } from '@angular/cdk/tree';
import { CollectionViewer, SelectionChange, DataSource } from '@angular/cdk/collections';
import { Component, Injectable, OnInit } from '@angular/core';
import { MatTreeFlatDataSource, MatTreeFlattener } from '@angular/material/tree';
import { BehaviorSubject, merge, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { IFileInfoNode } from '../../md-explorer/models/IFileInfoNode';
import { MdFile } from '../../md-explorer/models/md-file';
import { MdFileService } from '../../md-explorer/services/md-file.service';
import { COMPOSITION_BUFFER_MODE } from '@angular/forms';
import { ProjectsService } from '../../md-explorer/services/projects.service';
import { Router } from '@angular/router';

// IFileInfoNode è interfaccia
// MdFile è la classe -> DynamicFlatNode


/**
 * Database for dynamic data. When expanding a node in the tree, the data source will need to fetch
 * the descendants data from the database.
 */
@Injectable({ providedIn: 'root' })
export class DynamicDatabase {

   constructor(private mdFileService: MdFileService,) {
     this.mdFileService.loadDynFolders('root', 1);
     
     var md1 = new MdFile('12Folder', 'c:primoFolder', 0, true);
     var md2 = new MdFile('2Folder', 'c:primoFoldersottoFolder', 1, true);
     var md3 = new MdFile('3Folder', 'c:primoFoldersottoFoldersottoFolder', 2, true);
     var md4 = new MdFile('4Folder', 'c:primoFoldersottoFoldersottoFolder', 2, true);
     var md5 = new MdFile('5Folder', 'c:cuccu', 3, false);
     this.dataMap.set(md1, [md2]);
     this.dataMap.set(md2, [md3, md4]);
     //this.dataMap.set(md3, [md4]);
     this.dataMap.set(md4, [md5]);

    
    var test = this.dataMap.get(md1);
     this.rootLevelNodes = [md1];
  }

  dataMap = new Map<MdFile, MdFile[]>();
  rootLevelNodes: MdFile[]; 

  /** Initial data from database */
  initialData():MdFile[] {    
    return this.rootLevelNodes;
  }

  getChildren(node: MdFile): MdFile[] | undefined {
    
    var test = this.dataMap.get(node);   
    return test;
  }

  isExpandable(node: MdFile): boolean {
    return this.dataMap.has(node);
  }
}

export class DynamicDataSource implements DataSource<MdFile> {

  dataChange = new BehaviorSubject<MdFile[]>([]);

  get data(): MdFile[] { return this.dataChange.value; }
  set data(value: MdFile[]) {
    this._treeControl.dataNodes = value;
    this.dataChange.next(value);
  }

  constructor(private _treeControl: FlatTreeControl<MdFile>,
    private _database: DynamicDatabase, private _mdFileService: MdFileService ) {
    this.data = _database.initialData();
    this._mdFileService.loadDocumentFolder('root', 1).subscribe(_ => {
      debugger;
      this.data = _;
    });
    //this.dataChange = _mdFileService._mdDynFolderDocument;
    //_mdFileService.loadDynFolders('root', 1);
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
    debugger;
    this._mdFileService.loadDocumentFolder(node.path, node.level + 1).subscribe(_ => {
      debugger;
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
  selector: 'app-new-project',
  templateUrl: './new-project.component.html',
  styleUrls: ['./new-project.component.scss']
})
export class NewProjectComponent implements OnInit {

 
  activeNode: any;
  folder: {
    name: string,
    path: string
  }

  getLevel = (node: MdFile) => node.level;

  isExpandable = (node: MdFile) => node.expandable;

  treeControl: FlatTreeControl<MdFile>;

  dataSource: DynamicDataSource;

  hasChild = (_: number, _nodeData: MdFile) => _nodeData.expandable;
   

  constructor(database: DynamicDatabase,
                private mdFileService: MdFileService,
                private router: Router,
                private projectService: ProjectsService) {
    this.treeControl = new FlatTreeControl<MdFile>(this.getLevel, this.isExpandable);
    this.dataSource = new DynamicDataSource(this.treeControl, database, mdFileService);
  }

  ngOnInit(): void {
    this.folder = { name: "test", path: "test path" };
  }

  public getFolder(node: IFileInfoNode) {    
    this.folder.name = node.name;
    this.folder.path = node.path;
  }

  public closeDialog() {
    this.projectService.setNewFolderProject(this.folder.path).subscribe(_ => {
      var dateTime = new Date();
      this.mdFileService.loadAll(null, null);
      this.router.navigate(['/main']);
    });
  }

}
