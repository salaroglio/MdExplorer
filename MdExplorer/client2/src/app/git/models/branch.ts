export interface IBranch {
  id: string
  name: string
  somethingIsChangedInTheBranch: boolean
  howManyFilesAreChanged: number
  howManyCommitAreToPush: number
  fullPath:string
}
