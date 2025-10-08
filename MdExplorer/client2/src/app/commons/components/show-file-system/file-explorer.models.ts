export interface SpecialFolder {
  name: string;
  path: string;
  icon: string;
}

export interface Drive {
  name: string;
  path: string;
  icon: string;
  label: string;
  totalSize: number;
  freeSpace: number;
  driveType: string;
}

export interface FileExplorerState {
  currentPath: string;
  navigationHistory: string[];
  selectedFolder: {
    name: string;
    path: string;
  };
}