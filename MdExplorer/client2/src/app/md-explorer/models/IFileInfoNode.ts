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
  index: number;
  isIndexed?: boolean;
  indexingStatus?: 'idle' | 'indexing' | 'completed';
  indexingProgress?: number; // 0-100
  developmentTags?: string[]; // Tags for development folders (e.g., "program", "tests", "docs")
}

