/**
 * Git provider types
 */
export type GitProvider = 'github' | 'gitlab' | 'bitbucket' | 'gitea' | 'azure' | 'generic';

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
  error: string | null;
}

/**
 * «Collega a un repository remoto»: solo l'URL, l'account git per quell'host e se fare
 * subito il primo push. Nessuna credenziale passa di qui: l'autenticazione la fa il git
 * di sistema col suo credential manager.
 */
export interface GenericSetupRemoteRequest {
  repositoryPath: string;
  remoteUrl: string;
  remoteName?: string;
  /** L'account git per l'host del remoto (con più account sullo stesso host git deve saperlo). */
  accountUsername?: string;
  pushAfterAdd?: boolean;
}

/**
 * Response from setting up a remote. `success` è vero solo se il remote è configurato E,
 * se richiesto, il push è riuscito; `error` è lo stderr di git così com'è.
 */
export interface GenericSetupRemoteResponse {
  success: boolean;
  message?: string;
  error?: string;
  remoteUrl: string;
  pushAttempted: boolean;
  pushSucceeded: boolean;
  durationMs: number;
}

/**
 * Data passed to the remote setup dialog
 */
export interface GitSetupRemoteGenericDialogData {
  projectPath: string;
  projectName: string;
  prefilledRemoteUrl?: string;  // Pre-filled URL when the repository already has an origin
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
  github: { id: 'github', name: 'GitHub', icon: 'code', color: '#24292e' },
  gitlab: { id: 'gitlab', name: 'GitLab', icon: 'code', color: '#fc6d26' },
  bitbucket: { id: 'bitbucket', name: 'Bitbucket', icon: 'cloud', color: '#0052cc' },
  gitea: { id: 'gitea', name: 'Gitea', icon: 'pets', color: '#609926' },
  azure: { id: 'azure', name: 'Azure DevOps', icon: 'cloud_queue', color: '#0078d4' },
  generic: { id: 'generic', name: 'Git', icon: 'source', color: '#666666' }
};
