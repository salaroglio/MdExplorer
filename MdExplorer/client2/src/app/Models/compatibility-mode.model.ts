export interface CompatibilityConfig {
  mode: 'mdexplorer' | 'github' | 'commonmark';
  config: {
    githubOptions: GitHubCompatibilityOptions;
  };
}

export interface GitHubCompatibilityOptions {
  embedImages: boolean;
  stripInteractive: boolean;
  preserveEmoji: boolean;
}

export interface SetCompatibilityModeRequest {
  mode: string;
  githubOptions?: GitHubCompatibilityOptions;
  projectPath?: string;
}

export interface ValidateDocumentRequest {
  content: string;
}

export interface ValidateDocumentResponse {
  compatible: boolean;
  warnings: string[];
}

export enum CompatibilityMode {
  MdExplorer = 'mdexplorer',
  GitHub = 'github',
  CommonMark = 'commonmark'
}
