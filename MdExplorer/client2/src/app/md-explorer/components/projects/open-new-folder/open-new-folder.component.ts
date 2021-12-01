import { Component, OnInit } from '@angular/core';
import { IFileInfoNode } from '../../../models/IFileInfoNode';
import { MdFileService } from '../../../services/md-file.service';
import { FlatTreeControl } from '@angular/cdk/tree';
import { MatTreeFlatDataSource, MatTreeFlattener } from '@angular/material/tree';
import { Observable } from 'rxjs';
import { MdFile } from '../../../models/md-file';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';


interface IFlatNode {
  expandable: boolean;
  name: string;
  level: number;
}


@Component({
  selector: 'app-open-new-folder',
  templateUrl: './open-new-folder.component.html',
  styleUrls: ['./open-new-folder.component.scss']
})
export class OpenNewFolderComponent implements OnInit {
  private _transformer = (node: IFileInfoNode, level: number) => {
    return {
      expandable: !!node.childrens && node.childrens.length > 0,
      name: node.name,
      level: level,
      path: node.path
    };
  }
  activeNode: any;
  folder: {
    name: string,
    path:string
  }

  treeControl = new FlatTreeControl<IFlatNode>(
    node => node.level, node => node.expandable);

  treeFlattener = new MatTreeFlattener(
    this._transformer, node => node.level, node => node.expandable, node => node.childrens);

  dataSource = new MatTreeFlatDataSource(this.treeControl, this.treeFlattener);

  hasChild = (_: number, node: IFlatNode) => node.expandable;

  ///////////////////////////////
  mdFiles: Observable<MdFile[]>;

  constructor(private mdFileService: MdFileService,
    public dialogRef: MatDialogRef<OpenNewFolderComponent>,) { }

  ngOnInit(): void {
    this.mdFiles = this.mdFileService.mdFoldersDocument;
    this.mdFileService.mdFoldersDocument.subscribe(data => {
      this.dataSource.data = data;
      this.folder = { name: "test", path:"test path" };
    });

    this.mdFileService.loadFolders();

    this.mdFiles.subscribe(data => {
      console.log(data);
    });
  }

  public getFolder(node: any) {
    this.folder.name = node.name;
    this.folder.path = node.path;
  }

  public closeDialog() {
    this.dialogRef.close(this.folder.path);
  }

}
