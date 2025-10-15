export class ShowFileMetadata {
  constructor() {
  }
  public start: string;
  public title: string;
  public typeOfSelection: string;

  // Nuove proprietà opzionali
  public buttonText?: string;
  public fileExtensions?: string[];
  public showFileDetails?: boolean;
  public allowMultipleSelection?: boolean;
  public placeholder?: string;
}

// Interface per segmenti del breadcrumb
export interface BreadcrumbSegment {
  name: string;
  fullPath: string;
  icon?: string;
}

// Interface per dati del dialog di creazione directory
export interface NewDirectoryDialogData {
  parentNode: any; // MdFile
  parentPath: string;
  parentName: string;
  isRoot: boolean;
  currentPath: string;
}
