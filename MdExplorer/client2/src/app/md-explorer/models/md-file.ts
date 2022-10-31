import { IFileInfoNode } from "./IFileInfoNode";


export class MdFile implements IFileInfoNode {
  

  constructor(public name:string,public path:string, public level: number, public expandable:boolean) {

  }
  
  type: string;
  //children: MdFile[];
  isLoading: boolean;
  childrens: MdFile[];
  fullPath: string;
  relativePath: string;
  fullDirectoryPath: string;
}
