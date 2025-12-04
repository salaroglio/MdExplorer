/**
 * Git provider types
 */
export type GitProvider = 'github' | 'gitlab' | 'bitbucket' | 'gitea' | 'azure' | 'generic';

/**
 * Authentication method types
 */
export type AuthMethod = 'username_password' | 'pat' | 'ssh';

/**
 * Request for parsing a remote URL
 */
export interface ParseRemoteUrlRequest {
  url: string;
}

/**
 * Response from parsing a remote URL
 */
export interface ParseRemoteUrlResponse {
  isValid: boolean;
  provider: GitProvider;
  host: string;
  owner: string;
  repoName: string;
  protocol: 'https' | 'ssh' | 'git';
  supportsAutoCreate: boolean;
  tokenCreationUrl: string | null;
  error: string | null;
}

/**
 * Request for validating remote with credentials
 */
export interface ValidateRemoteAuthRequest {
  remoteUrl: string;
  username?: string;
  password?: string;
  authMethod?: AuthMethod;
}

/**
 * Response from validating remote credentials
 */
export interface ValidateRemoteAuthResponse {
  isReachable: boolean;
  requiresAuth: boolean;
  credentialsValid: boolean;
  repositoryExists: boolean;
  provider: GitProvider;
  error: string | null;
}

/**
 * Request for setting up a generic remote
 */
export interface GenericSetupRemoteRequest {
  repositoryPath: string;
  remoteUrl: string;
  remoteName?: string;
  authMethod?: AuthMethod;
  username?: string;
  password?: string;
  token?: string;
  saveCredentials?: boolean;
  pushAfterAdd?: boolean;
  createRemoteRepo?: boolean;
  repoDescription?: string;
  isPrivate?: boolean;
}

/**
 * Response from setting up a generic remote
 */
export interface GenericSetupRemoteResponse {
  success: boolean;
  message?: string;
  error?: string;
  repositoryCreated: boolean;
  remoteUrl: string;
  durationMs: number;
}

/**
 * Data passed to the generic remote setup dialog
 */
export interface GitSetupRemoteGenericDialogData {
  projectPath: string;
  projectName: string;
  prefilledRemoteUrl?: string;  // Pre-filled URL when reconfiguring existing remote
}

/**
 * Provider display information for UI
 */
export interface ProviderInfo {
  id: GitProvider;
  name: string;
  icon: string;
  color: string;
}

/**
 * Provider display configurations
 */
export const PROVIDER_INFO: Record<GitProvider, ProviderInfo> = {
  github: {
    id: 'github',
    name: 'GitHub',
    icon: 'code',
    color: '#24292e'
  },
  gitlab: {
    id: 'gitlab',
    name: 'GitLab',
    icon: 'code',
    color: '#fc6d26'
  },
  bitbucket: {
    id: 'bitbucket',
    name: 'Bitbucket',
    icon: 'cloud',
    color: '#0052cc'
  },
  gitea: {
    id: 'gitea',
    name: 'Gitea',
    icon: 'pets',
    color: '#609926'
  },
  azure: {
    id: 'azure',
    name: 'Azure DevOps',
    icon: 'cloud_queue',
    color: '#0078d4'
  },
  generic: {
    id: 'generic',
    name: 'Git',
    icon: 'source',
    color: '#666666'
  }
};
