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
    this._mdFileService.loadDocumentFolder('root', 0).subscribe(_ => {
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
    this._mdFileService.loadDocumentFolder(node.path, node.level + 1).subscribe(_ => {

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
  

  folder: {
    name: string,
    path: string
  }
  
  constructor(private router: Router,
              private database: DynamicDatabase,
              private mdFileService: MdFileService,
              private projectService: ProjectsService) {
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
  isFolder = (_: number, node: IFileInfoNode) => node.type == "folder";
  getLevel = (node: MdFile) => node.level;
  isExpandable = (node: MdFile) => node.expandable;
}
