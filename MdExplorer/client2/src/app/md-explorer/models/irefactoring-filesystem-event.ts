export interface IRefactoringFilesystemEvent {
  Id: string;
  EventName: string;
  NewFullPath: string;
  OldFullPath: string;
  RefactoringGroupId: string;
  Processed: boolean;
}
