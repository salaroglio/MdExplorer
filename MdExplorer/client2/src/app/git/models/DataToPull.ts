export class DataToPull {
  somethingIsToPull: boolean
  somethingIsToPush:boolean
  howManyFilesAreToPull: number
  connectionIsActive: boolean
  howManyCommitAreToPush: number
  whatFilesAreChanged: FileNameAndAuthor[]
}

export class FileNameAndAuthor {
  fileName: string
  author: string
  fullPath: string

}
