export interface IRefactoringInvolvedFilesAction {
  Id: string;
  SuggestedAction: string;
  FileName: string;
  Fullpath: string;
  OldLinkStored: string;
  NewLinkStored: string;
  CreationDate: Date;
}
