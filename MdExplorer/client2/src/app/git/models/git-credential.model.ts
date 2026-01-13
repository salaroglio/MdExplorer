/**
 * Git Credential model for shared authentication credentials
 * Can be reused across multiple repositories
 */
export interface GitCredential {
  id?: string;
  accountName: string;
  accountType: 'GitHub' | 'GitLab' | 'Bitbucket' | 'Generic';
  authUsername?: string;
  hasGitHubPAT?: boolean;
  hasGitLabToken?: boolean;
  hasSSHKeyPath?: boolean;
  hasBitbucketAppPassword?: boolean;
  hasHttpsPassword?: boolean;
  isActive: boolean;
  createdAt?: Date;
  updatedAt?: Date;
}

/**
 * Request to create a new Git credential
 */
export interface CreateGitCredentialRequest {
  accountName: string;
  accountType: string;
  authUsername?: string;
  gitHubPAT?: string;
  gitLabToken?: string;
  sshKeyPath?: string;
  bitbucketAppPassword?: string;
  httpsPassword?: string;
  isActive?: boolean;
}

/**
 * Request to update an existing Git credential
 */
export interface UpdateGitCredentialRequest {
  accountName?: string;
  accountType?: string;
  authUsername?: string;
  gitHubPAT?: string;
  gitLabToken?: string;
  sshKeyPath?: string;
  bitbucketAppPassword?: string;
  httpsPassword?: string;
  isActive?: boolean;
}
