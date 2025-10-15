// Modern Git models for native authentication

export interface ModernGitRequest {
  ProjectPath: string;  // Match C# property name (PascalCase)
  CommitMessage?: string;  // Match C# property name (PascalCase)
}

export interface ModernGitResponse {
  success: boolean;
  message: string;
  errorMessage?: string;
  authenticationMethod?: string;
  durationMs: number;
  thereAreConflicts: boolean;
  changedFiles?: ChangedFileInfo[];
}

export interface ModernPullResponse extends ModernGitResponse {
  commitsPulled: number;
  filesChanged: number;
}

export interface ModernCommitResponse extends ModernGitResponse {
  commitHash?: string;
  filesChanged: number;
}

export interface ChangedFileInfo {
  fullPath: string;
  relativePath: string;
  fileName: string;
  status: string;
  author: string;
  mdFiles: FileStructureNode[];
}

export interface FileStructureNode {
  name: string;
  fullPath: string;
  relativePath: string;
  level: number;
  type: string;
  expandable: boolean;
}

export interface ModernBranchStatusResponse {
  name: string;
  somethingIsChangedInTheBranch: boolean;
  howManyFilesAreChanged: number;
  howManyCommitAreToPush: number;
  fullPath: string;
}

// Git History Models
export interface GitCommitInfo {
  hash: string;
  shortHash?: string;
  author: string;
  email: string;
  message: string;
  date: Date | string;
  branch: string;
  parents: string[];
  isMerge?: boolean;
}

export interface GitHistoryRequest {
  repositoryPath: string;
  maxCommits?: number;
}

export interface GitHistoryResponse {
  success: boolean;
  commits: GitCommitInfo[];
  count: number;
  error?: string;
}

// Remote Status Models
export interface RemoteStatus {
  hasRemote: boolean;
  remoteName?: string;
  remoteUrl?: string;
  isGitRepository: boolean;
  errorMessage?: string;
  canAuthenticate: boolean;
  authenticationMethod?: string;
}

export interface SetupRemoteRequest {
  repositoryPath: string;
  organization: string;
  repositoryName: string;
  repositoryDescription?: string;
  isPrivate?: boolean;
  saveOrganization: boolean;
  pushAfterAdd: boolean;
}

export interface SetupRemoteResponse {
  success: boolean;
  message?: string;
  error?: string;
  needsToken?: boolean;
  durationMs?: number;
}

// Legacy response compatibility adapter
export interface ModernResponsePull {
  isConnectionMissing: boolean;
  isAuthenticationMissing: boolean;
  thereAreConflicts: boolean;
  errorMessage?: string;
  whatFilesWillBeChanged: any[];
}