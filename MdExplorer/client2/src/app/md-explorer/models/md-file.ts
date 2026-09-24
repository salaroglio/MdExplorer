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

  // True when the folder owns a generated TOC file (<dirname>.md.directory)
  hasToc?: boolean;

  // True for *.agent.md files (agentic markdown: launchable/schedulable agents)
  isAgentFile?: boolean;

  // Folder "reveal extra content" (eye) state — see IFileInfoNode for semantics.
  hasExtraContent?: boolean;
  extraLoaded?: boolean;
  isExtra?: boolean;
  // A revealed non-markdown file whose content is text: clickable, shown colored in the panel.
  isTextFile?: boolean;

  // A slide deck opened on one of its slides: reveal.js's position ('#/3' or '#/3/1'). Set by the
  // breadcrumb between decks and by the history of the title-bar arrows.
  slideHash?: string;

  // Compact folder properties (VS Code-style)
  isCompacted?: boolean;
  compactedPath?: string;
  compactedSegments?: CompactSegment[];

  // External app embedding
  appId?: string;
  appExecutable?: string;
  appArgs?: string[];
  appIcon?: string;
  appDescription?: string;
}
