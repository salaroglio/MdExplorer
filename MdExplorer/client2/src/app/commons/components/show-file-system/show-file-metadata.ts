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
