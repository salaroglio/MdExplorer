import { IRefactoringFilesystemEvent } from "./irefactoring-filesystem-event";

export class RefactoringFilesystemEvent implements IRefactoringFilesystemEvent{
    Id: string;
    EventName: string;
    NewFullPath: string;
    OldFullPath: string;
    RefactoringGroupId: string;
    Processed: boolean;

}
