import { IFileInfoNode } from "./IFileInfoNode";


export class MdFile implements IFileInfoNode{
  name: string;
  path: string;
  type: string;
  children: [];
}
