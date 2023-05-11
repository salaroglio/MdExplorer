export interface IBranch {
  id: string
  name: string
  somethingIsChangedInTheBranch: boolean
  somethingIsToPull:boolean
  howManyFilesAreChanged: number
  howManyFilesAreToPull:number
  fullPath:string
}
