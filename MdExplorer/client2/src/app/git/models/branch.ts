export interface IBranch {
  id: string
  name: string
  somethingIsChangedInTheBranch: boolean
  howManyFilesAreChanged: number
  howManyCommitAreToPush: number
  fullPath:string
}

export interface BranchInfo {
  name: string;
  isRemote: boolean;
  isCurrentBranch: boolean;
  remoteName?: string;
  upstreamBranch?: string;
  ahead?: number;
  behind?: number;
}

export interface CheckoutResult {
  success: boolean;
  message?: string;
  error?: string;
  hasUncommittedChanges?: boolean;
  durationMs?: number;
  branchName?: string;  // Current branch name after checkout
}
