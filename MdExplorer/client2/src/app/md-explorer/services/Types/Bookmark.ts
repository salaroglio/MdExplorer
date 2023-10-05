import { MdFile } from "../../models/md-file"

export class Bookmark extends MdFile {
  projectId: string

  constructor(mdFile: MdFile) {
    super(mdFile.name,mdFile.path,mdFile.level,mdFile.expandable);
    this.childrens = mdFile.childrens;
    this.fullDirectoryPath = mdFile.fullDirectoryPath;
    this.fullPath = mdFile.fullPath;
    this.index = mdFile.index;
    this.isLoading = mdFile.isLoading;
    this.relativePath = mdFile.relativePath;
    this.type = mdFile.type;
    
  }

}
