import { CompactSegment, IFileInfoNode } from "./IFileInfoNode";


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
  index: number;

  // Nuove proprietà per caricamento incrementale
  isIndexed?: boolean;
  indexingStatus?: 'idle' | 'indexing' | 'completed';
  indexingProgress?: number;

  // Development tags per classificare le cartelle
  developmentTags?: string[];

  // Compact folder properties (VS Code-style)
  isCompacted?: boolean;
  compactedPath?: string;
  compactedSegments?: CompactSegment[];
}
