// Segment of a compacted folder path (for VS Code-style compact folders)
export interface CompactSegment {
  name: string;       // Single folder name: "src", "main", "java"
  fullPath: string;   // Full path: "C:\project\src\main\java"
  level: number;      // Level in original tree
}

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

  // True when the folder owns a generated TOC file (<dirname>.md.directory)
  hasToc?: boolean;

  // True when the folder contains content the md-tree does not show (non-.md files or
  // markdown-empty subfolders). Drives the "reveal content" (eye) entry in the context menu.
  hasExtraContent?: boolean;
  // True once the extra content of this folder has been pulled into the tree (eye toggled on).
  extraLoaded?: boolean;
  // True on nodes that were injected by a folder "reveal" call (so a "hide" can remove them
  // again without touching the folder's real .md children).
  isExtra?: boolean;

  // Compact folder properties (VS Code-style)
  isCompacted?: boolean;                // True if this node is the result of compacting multiple folders
  compactedPath?: string;               // Compacted display path: "src / main / java"
  compactedSegments?: CompactSegment[]; // Segments for context menu selection

  // External app embedding
  appId?: string;
  appExecutable?: string;
  appArgs?: string[];
  appIcon?: string;
  appDescription?: string;
}

