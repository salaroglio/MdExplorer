/**
 * Git Account model for repository-specific authentication
 */
export interface GitAccount {
  id?: string;
  repositoryPath: string;
  accountName: string;
  accountType: 'GitHub' | 'GitLab' | 'Bitbucket' | 'Generic';
  hasGitHubPAT?: boolean;
  hasGitLabToken?: boolean;
  hasSSHKeyPath?: boolean;
  username?: string;
  email?: string;
  notes?: string;
  isActive: boolean;
  createdAt?: Date;
  updatedAt?: Date;
}

/**
 * Request to create a new Git account
 */
export interface CreateGitAccountRequest {
  repositoryPath: string;
  accountName: string;
  accountType: string;
  gitHubPAT?: string;
  gitLabToken?: string;
  sshKeyPath?: string;
  username?: string;
  email?: string;
  notes?: string;
  isActive?: boolean;
}

/**
 * Request to update an existing Git account
 */
export interface UpdateGitAccountRequest {
  repositoryPath: string;
  accountName: string;
  accountType: string;
  gitHubPAT?: string;
  gitLabToken?: string;
  sshKeyPath?: string;
  username?: string;
  email?: string;
  notes?: string;
  isActive?: boolean;
}

/**
 * Result of testing Git credentials
 */
export interface GitConnectionTestResult {
  success: boolean;
  message?: string;
  error?: string;
}
