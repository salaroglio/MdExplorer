import { GitCredential } from './git-credential.model';

/**
 * Git Account model for repository-specific authentication
 */
export interface GitAccount {
  id?: string;
  repositoryPath: string;
  credentialId?: string;
  credential?: GitCredential;
  accountName?: string;
  accountType?: 'GitHub' | 'GitLab' | 'Bitbucket' | 'Generic';
  hasGitHubPAT?: boolean;
  hasGitLabToken?: boolean;
  hasSSHKeyPath?: boolean;
  hasBitbucketAppPassword?: boolean;
  hasHttpsPassword?: boolean;
  authUsername?: string;
  preferredAuthMethod?: 'token' | 'username_password' | 'ssh';
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
  credentialId?: string;  // Use existing credential
  // Fields for creating new credential (ignored if credentialId is set)
  accountName?: string;
  accountType?: string;
  authUsername?: string;
  gitHubPAT?: string;
  gitLabToken?: string;
  sshKeyPath?: string;
  bitbucketAppPassword?: string;
  httpsPassword?: string;
  // Repository-specific fields
  preferredAuthMethod?: string;
  username?: string;
  email?: string;
  notes?: string;
  isActive?: boolean;
}

/**
 * Request to update an existing Git account
 */
export interface UpdateGitAccountRequest {
  repositoryPath?: string;
  credentialId?: string;  // Switch to different credential
  // Fields for creating/updating credential
  accountName?: string;
  accountType?: string;
  authUsername?: string;
  gitHubPAT?: string;
  gitLabToken?: string;
  sshKeyPath?: string;
  bitbucketAppPassword?: string;
  httpsPassword?: string;
  // Repository-specific fields
  preferredAuthMethod?: string;
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
