export interface PlatformBuild {
  downloadUrl: string;
  executableName: string;
  version?: string;
}

export interface StoreCatalogApp {
  id: string;
  name: string;
  description?: string;
  version: string;
  icon?: string;
  downloadUrl: string;
  executableName: string;
  defaultArgs?: string[];
  changelog?: string;
  platforms?: { [key: string]: PlatformBuild };
  repoId?: string;
  repoLabel?: string;
}

export interface RepoInfo {
  id: string;
  label: string;
  repoName?: string;
  repoDescription?: string;
  repoCompany?: string;
  repoLogo?: string;
}

export interface StoreCatalog {
  version: string;
  repoName?: string;
  repoDescription?: string;
  repoCompany?: string;
  repoLogo?: string;
  apps: StoreCatalogApp[];
  repos?: RepoInfo[];
  failedRepos?: number;
}

export interface InstalledApp {
  appId: string;
  name: string;
  version: string;
  icon?: string;
  description?: string;
  installedAt: string;
  updatedAt?: string;
  localPath: string;
  executableName: string;
  platform?: string;
}

export interface AppStoreRepository {
  id: string;
  label: string;
  url: string;
  username?: string;
  passwordConfigured?: boolean;
  sortOrder: number;
}
