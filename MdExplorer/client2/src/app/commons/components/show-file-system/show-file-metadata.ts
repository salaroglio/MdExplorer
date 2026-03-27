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
  /** When true, shows a filename input in the bottom panel (Save As mode) */
  public saveAs?: boolean;
  /** Default filename suggestion for Save As mode */
  public defaultFileName?: string;
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
