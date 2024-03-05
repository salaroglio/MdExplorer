import { MdFile } from "../../md-explorer/models/md-file"
import { FileNameAndAuthor } from "./DataToPull"

export class ResponsePull {
  isConnectionMissing: boolean
  isAuthenticationMissing: boolean
  thereAreConflicts: boolean
  errorMessage: string
  whatFilesWillBeChanged: FileNameAndAuthor[]
}

