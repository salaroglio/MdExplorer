import { MdFile } from "../../md-explorer/models/md-file"

export class DataToPull {
  somethingIsToPull: boolean
  somethingIsToPush:boolean
  howManyFilesAreToPull: number
  connectionIsActive: boolean
  howManyCommitAreToPush: number
  whatFilesWillBeChanged: FileNameAndAuthor[]
}

export class FileNameAndAuthor {
  fileName: string
  author: string
  fullPath: string
  relativePath:string
  status: string
  mdFiles: MdFile[]
}
