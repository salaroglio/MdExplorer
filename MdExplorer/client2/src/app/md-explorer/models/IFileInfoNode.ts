// implementation of this interface is MDFile
export interface IFileInfoNode {
  name: string;
  path: string;
  relativePath: string;
  fullPath: string;
  type: string;
  level: number;
  expandable: boolean;
  isLoading: boolean;
  childrens?: IFileInfoNode[];
}

