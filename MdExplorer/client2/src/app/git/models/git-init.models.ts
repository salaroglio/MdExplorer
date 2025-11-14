/**
 * Request for initializing a Git repository
 */
export interface InitRepositoryRequest {
  /** Path to the directory where Git repository should be initialized */
  repositoryPath: string;

  /** Name of the initial branch (default: "main") */
  initialBranch?: string;

  /** Template for .gitignore file */
  gitignoreTemplate?: 'mdexplorer' | 'node' | 'python' | 'csharp' | 'none';
}

/**
 * Response from initializing a Git repository
 */
export interface InitRepositoryResponse {
  /** Whether the initialization was successful */
  success: boolean;

  /** Success or error message */
  message: string;

  /** Whether the directory is now a Git repository */
  isGitRepository: boolean;

  /** Path to the initialized repository */
  repositoryPath?: string;

  /** Name of the initial branch created */
  initialBranch?: string;
}

/**
 * .gitignore template option
 */
export interface GitignoreTemplate {
  value: string;
  label: string;
  description: string;
}

/**
 * Available .gitignore templates
 */
export const GITIGNORE_TEMPLATES: GitignoreTemplate[] = [
  {
    value: 'mdexplorer',
    label: 'MdExplorer (Default)',
    description: 'Ignores .md/, database files, logs, and temporary files'
  },
  {
    value: 'node',
    label: 'Node.js',
    description: 'Ignores node_modules/, npm logs, build outputs, and .env files'
  },
  {
    value: 'python',
    label: 'Python',
    description: 'Ignores __pycache__/, virtual environments, and distribution files'
  },
  {
    value: 'csharp',
    label: '.NET / C#',
    description: 'Ignores bin/, obj/, Visual Studio files, and build outputs'
  },
  {
    value: 'none',
    label: 'None',
    description: 'Creates an empty .gitignore file'
  }
];
