import { HttpClient } from '@angular/common/http';
import { Injectable, OnDestroy } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { IBranch, BranchInfo, CheckoutResult } from '../models/branch';
import { DataToPull } from '../models/DataToPull'
import { CloneInfo } from '../models/cloneRequest';
import { GitlabSetting } from '../models/gitlab-setting';
import { ITag } from '../models/Tag';
import { ResposneClone } from './responses/ResponseClone';
import {
  ModernGitRequest,
  ModernGitResponse,
  ModernPullResponse,
  ModernCommitResponse,
  ModernBranchStatusResponse,
  ModernResponsePull,
  GitCommitInfo,
  GitHistoryRequest,
  GitHistoryResponse,
  RemoteStatus,
  SetupRemoteRequest,
  SetupRemoteResponse,
  ChangedFilesResponse,
  GitChangedFile,
  DiscardFileResponse
} from '../models/modern-git-models';


@Injectable({
  providedIn: 'root'
})
export class GITService implements OnDestroy {
  private _Settings: BehaviorSubject<GitlabSetting[]>;
  private gitPollingInterval: any = null;
  private readonly ACTIVE_POLLING_INTERVAL = 60000; // 60 secondi quando attivo
  private readonly INACTIVE_POLLING_INTERVAL = 300000; // 5 minuti quando inattivo
  private currentProjectPath: string = null;
  
  public currentBranch$: BehaviorSubject<IBranch> = new BehaviorSubject<IBranch>(
    {
      id: "", name: "",
      somethingIsChangedInTheBranch: true,
      howManyFilesAreChanged: 0,            
      fullPath: "",
      howManyCommitAreToPush:0,
    });

  public commmitsToPull$: BehaviorSubject<DataToPull> = new BehaviorSubject<DataToPull>({
    howManyFilesAreToPull: 0,
    howManyCommitAreToPush:0,
    somethingIsToPull: false,
    somethingIsToPush:false,
    connectionIsActive: false,
    whatFilesWillBeChanged: []
  });

  constructor(
    private http: HttpClient
  ) {
    this.initializeSmartPolling();
  }

  /**
   * Set the current project path for modern Git operations
   */
  setProjectPath(path: string): void {
    this.currentProjectPath = path;
    // Trigger immediate poll with new path
    if (path) {
      this.performPoll();
    }
  }

  /**
   * Inizializza il polling intelligente che si adatta alla visibilità della finestra
   */
  private initializeSmartPolling(): void {
    // Polling iniziale immediato
    this.performPoll();
    
    // Avvia polling con intervallo attivo
    this.startPolling(this.ACTIVE_POLLING_INTERVAL);
    
    // Listener per cambio visibilità finestra
    document.addEventListener('visibilitychange', () => {
      this.handleVisibilityChange();
    });
    
    // Listener per focus/blur finestra (backup per browser che non supportano visibilitychange)
    window.addEventListener('focus', () => {
      this.handleWindowFocus();
    });
    
    window.addEventListener('blur', () => {
      this.handleWindowBlur();
    });
    
  }

  /**
   * Perform polling based on current configuration
   * IMPORTANT: Checks remote status first to authenticate, then fetches Git data using cached credentials
   */
  private performPoll(): void {
    if (this.currentProjectPath) {
      // Step 1: Check remote status first (authenticates and caches credentials)
      this.checkRemoteStatus(this.currentProjectPath).subscribe(
        remoteStatus => {
          // Only proceed with Git operations if authentication is successful
          if (remoteStatus.hasRemote && remoteStatus.canAuthenticate) {
            // Step 2: Now fetch Git data (will use cached credentials, no additional auth)
            this.modernGetBranchStatus(this.currentProjectPath).subscribe(
              branch => {
                this.currentBranch$.next(branch);
              },
              error => {
                console.error('Error in modern branch status:', error);
                // Set default empty state on error
                this.currentBranch$.next({
                  id: "", name: "unknown",
                  somethingIsChangedInTheBranch: false,
                  howManyFilesAreChanged: 0,
                  fullPath: this.currentProjectPath,
                  howManyCommitAreToPush: 0
                });
              }
            );

            this.modernGetDataToPull(this.currentProjectPath).subscribe(
              pullData => {
                this.commmitsToPull$.next(pullData);
              },
              error => {
                console.error('Error in modern data to pull:', error);
              }
            );
          } else if (!remoteStatus.hasRemote && remoteStatus.isGitRepository) {
            // No remote configured - still get branch status but skip pull/push data
            this.modernGetBranchStatus(this.currentProjectPath).subscribe(
              branch => {
                this.currentBranch$.next(branch);
              },
              error => {
                console.error('Error in modern branch status:', error);
              }
            );
          } else if (!remoteStatus.isGitRepository) {
            // Not a Git repository - emit empty state to clear UI
            console.log('📁 Not a Git repository - clearing Git state');
            this.currentBranch$.next({
              id: "",
              name: "",
              somethingIsChangedInTheBranch: false,
              howManyFilesAreChanged: 0,
              fullPath: this.currentProjectPath,
              howManyCommitAreToPush: 0
            });

            this.commmitsToPull$.next({
              somethingIsToPull: false,
              somethingIsToPush: false,
              howManyFilesAreToPull: 0,
              howManyCommitAreToPush: 0,
              connectionIsActive: false,
              whatFilesWillBeChanged: []
            });
          }
        },
        error => {
          console.error('Error checking remote status in poll:', error);
          // On error, also clear Git state
          this.currentBranch$.next({
            id: "",
            name: "",
            somethingIsChangedInTheBranch: false,
            howManyFilesAreChanged: 0,
            fullPath: this.currentProjectPath,
            howManyCommitAreToPush: 0
          });

          this.commmitsToPull$.next({
            somethingIsToPull: false,
            somethingIsToPush: false,
            howManyFilesAreToPull: 0,
            howManyCommitAreToPush: 0,
            connectionIsActive: false,
            whatFilesWillBeChanged: []
          });
        }
      );
    }
  }

  /**
   * Gestisce il cambio di visibilità della finestra
   */
  private handleVisibilityChange(): void {
    if (document.visibilityState === 'visible') {
      this.handleWindowFocus();
    } else {
      this.handleWindowBlur();
    }
  }

  /**
   * Quando la finestra diventa attiva: polling più frequente
   */
  private handleWindowFocus(): void {
    this.startPolling(this.ACTIVE_POLLING_INTERVAL);
    // Add short delay to avoid race condition with dialog updates (e.g., branch switch)
    setTimeout(() => {
      this.performPoll();
    }, 500);
  }

  /**
   * Quando la finestra diventa inattiva: polling meno frequente
   */
  private handleWindowBlur(): void {
    this.startPolling(this.INACTIVE_POLLING_INTERVAL);
  }

  /**
   * Avvia polling con intervallo specificato
   */
  private startPolling(interval: number): void {
    // Ferma polling esistente
    if (this.gitPollingInterval) {
      clearInterval(this.gitPollingInterval);
    }
    
    // Avvia nuovo polling
    this.gitPollingInterval = setInterval(() => {
      this.performPoll();
    }, interval);
  }

  /**
   * Ferma completamente il polling (per cleanup)
   */
  public stopPolling(): void {
    if (this.gitPollingInterval) {
      clearInterval(this.gitPollingInterval);
      this.gitPollingInterval = null;
    }
  }

  clone(request: CloneInfo): Observable<ResposneClone> {
    const url = '../api/gitfeatures/cloneRepository';
    return this.http.post<ResposneClone>(url, request);
  }


  getBranchList():Observable<IBranch[]> {
    const url = '../api/gitservice/branches';
    return this.http.get<IBranch[]>(url);
  }

  checkoutSelectedBranch(selected: IBranch) {
    const url = '../api/gitservice/branches/feat/checkoutBranch';
    return this.http.post<IBranch>(url, selected);
  }

  getTagList() {
    const url = '../api/gitservice/tags';
    return this.http.get<ITag[]>(url);
  }

  storeGitlabSettings(user: string, password: string, gitlabLink: string) {
    const url = '../api/gitservice/gitlabsettings';
    let setting = new GitlabSetting();    
    return this.http.post<GitlabSetting>(url, setting);
  }

  getGitlabSettings(): Observable<GitlabSetting[]> {
    const url = '../api/gitservice/gitlabsettings';
    return this.http.get<GitlabSetting[]>(url);
  }


  // ===== MODERN GIT METHODS WITH NATIVE AUTHENTICATION =====

  /**
   * Pull using modern Git service with native authentication
   */
  modernPull(projectPath: string): Observable<ModernResponsePull> {
    const request: ModernGitRequest = { ProjectPath: projectPath };
    const url = '../api/ModernGitToolbar/pull';
    
    return this.http.post<ModernPullResponse>(url, request).pipe(
      map(response => this.adaptModernResponseToLegacy(response))
    );
  }

  /**
   * Commit using modern Git service with native authentication
   */
  modernCommit(projectPath: string, commitMessage?: string): Observable<ModernResponsePull> {
    const request: ModernGitRequest = { 
      ProjectPath: projectPath, 
      CommitMessage: commitMessage 
    };
    const url = '../api/ModernGitToolbar/commit';
    
    return this.http.post<ModernCommitResponse>(url, request).pipe(
      map(response => this.adaptModernResponseToLegacy(response))
    );
  }

  /**
   * Commit and push using modern Git service with native authentication
   */
  modernCommitAndPush(projectPath: string, commitMessage?: string): Observable<ModernResponsePull> {
    const request: ModernGitRequest = { 
      ProjectPath: projectPath, 
      CommitMessage: commitMessage 
    };
    const url = '../api/ModernGitToolbar/commit-and-push';
    
    console.log('[DEBUG] Sending commit request:', JSON.stringify(request, null, 2));
    
    return this.http.post<ModernCommitResponse>(url, request).pipe(
      map(response => this.adaptModernResponseToLegacy(response))
    );
  }

  /**
   * Push using modern Git service with native authentication
   */
  modernPush(projectPath: string): Observable<ModernResponsePull> {
    const request: ModernGitRequest = { ProjectPath: projectPath };
    const url = '../api/ModernGitToolbar/push-v2';

    return this.http.post<ModernGitResponse>(url, request).pipe(
      map(response => this.adaptModernResponseToLegacy(response))
    );
  }

  /**
   * Clone repository using modern Git service with native authentication
   */
  modernClone(request: { url: string; localPath: string; branchName?: string }): Observable<{ success: boolean; error?: string }> {
    const url = '../api/ModernGit/clone';
    // Convert to PascalCase for C# backend
    const requestBody = {
      Url: request.url,
      LocalPath: request.localPath,
      BranchName: request.branchName || null
    };
    console.log('[GITService.modernClone] Sending to backend:', requestBody);
    return this.http.post<{ success: boolean; error?: string }>(url, requestBody).pipe(
      catchError(error => {
        console.error('[modernClone] Full error:', error);
        // Try to extract validation errors if present
        let errorMessage = 'Clone failed';
        if (error.error) {
          if (typeof error.error === 'string') {
            errorMessage = error.error;
          } else if (error.error.errors) {
            // Validation errors from ModelState
            const validationErrors = [];
            for (const field in error.error.errors) {
              validationErrors.push(`${field}: ${error.error.errors[field].join(', ')}`);
            }
            errorMessage = validationErrors.join('; ');
          } else if (error.error.message) {
            errorMessage = error.error.message;
          } else if (error.error.error) {
            errorMessage = error.error.error;
          }
        }
        return of({ success: false, error: errorMessage });
      })
    );
  }


  /**
   * Get branch status using modern Git service
   */
  modernGetBranchStatus(projectPath: string): Observable<IBranch> {
    const url = `../api/ModernGitToolbar/branch-status?projectPath=${encodeURIComponent(projectPath)}`;
    
    return this.http.get<ModernBranchStatusResponse>(url).pipe(
      map(response => ({
        id: '',
        name: response.name,
        somethingIsChangedInTheBranch: response.somethingIsChangedInTheBranch,
        howManyFilesAreChanged: response.howManyFilesAreChanged,
        howManyCommitAreToPush: response.howManyCommitAreToPush,
        fullPath: response.fullPath
      }))
    );
  }

  /**
   * Get data to pull/push using modern Git service
   */
  modernGetDataToPull(projectPath: string): Observable<DataToPull> {
    const url = `../api/ModernGitToolbar/get-data-to-pull?projectPath=${encodeURIComponent(projectPath)}`;
    
    return this.http.get<DataToPull>(url).pipe(
      catchError(error => {
        console.error('Error in modernGetDataToPull:', error);
        // Return empty data on error
        return of({
          somethingIsToPull: false,
          somethingIsToPush: false,
          howManyFilesAreToPull: 0,
          howManyCommitAreToPush: 0,
          connectionIsActive: false,
          whatFilesWillBeChanged: []
        });
      })
    );
  }

  /**
   * Get commit history for a repository
   */
  getCommitHistory(projectPath: string, maxCommits?: number): Observable<GitCommitInfo[]> {
    const request: GitHistoryRequest = {
      repositoryPath: projectPath,
      maxCommits: maxCommits || 50
    };
    const url = '../api/ModernGit/history';

    return this.http.post<GitHistoryResponse>(url, request).pipe(
      map(response => {
        if (response.success && response.commits) {
          // Convert date strings to Date objects if needed
          return response.commits.map(commit => ({
            ...commit,
            date: typeof commit.date === 'string' ? new Date(commit.date) : commit.date,
            shortHash: commit.hash ? commit.hash.substring(0, 7) : '',
            isMerge: commit.parents && commit.parents.length > 1
          }));
        }
        return [];
      }),
      catchError(error => {
        console.error('Error getting commit history:', error);
        return of([]);
      })
    );
  }

  /**
   * Check if repository has remote configured
   */
  checkRemoteStatus(projectPath: string): Observable<RemoteStatus> {
    const url = `../api/ModernGit/remote-status?repositoryPath=${encodeURIComponent(projectPath)}`;

    return this.http.get<RemoteStatus>(url).pipe(
      catchError(error => {
        console.error('Error checking remote status:', error);
        return of({
          hasRemote: false,
          isGitRepository: false,
          errorMessage: error.message || 'Failed to check remote status',
          canAuthenticate: false
        });
      })
    );
  }

  /**
   * Get the remote URL for a repository (typically origin)
   * Used by Share Project feature to generate shareable URLs
   */
  getRemoteUrl(projectPath: string): Observable<{ hasRemote: boolean; remoteUrl?: string; error?: string }> {
    const url = `../api/ModernGit/remote-url?repositoryPath=${encodeURIComponent(projectPath)}`;

    return this.http.get<{ hasRemote: boolean; remoteUrl?: string; error?: string }>(url).pipe(
      catchError(error => {
        console.error('Error getting remote URL:', error);
        return of({
          hasRemote: false,
          error: error.message || 'Failed to get remote URL'
        });
      })
    );
  }

  /**
   * Validate if a remote Git URL is reachable
   * Used before cloning to verify URL is accessible
   */
  validateRemoteUrl(gitUrl: string): Observable<{ isReachable: boolean; referenceCount?: number; error?: string; isAuthenticationError?: boolean }> {
    const url = `../api/ModernGit/validate-remote-url?url=${encodeURIComponent(gitUrl)}`;

    return this.http.get<{ isReachable: boolean; referenceCount?: number; error?: string; isAuthenticationError?: boolean }>(url).pipe(
      catchError(error => {
        console.error('Error validating remote URL:', error);
        return of({
          isReachable: false,
          error: error.message || 'Failed to validate remote URL'
        });
      })
    );
  }

  /**
   * Initialize a new Git repository
   */
  initRepository(request: any): Observable<any> {
    const url = '../api/ModernGit/init';
    return this.http.post<any>(url, request).pipe(
      catchError(error => {
        console.error('Error initializing Git repository:', error);
        throw error;
      })
    );
  }

  /**
   * Remove a remote from the repository
   */
  removeRemote(projectPath: string, remoteName: string = 'origin'): Observable<any> {
    const url = `../api/ModernGit/remove-remote?repositoryPath=${encodeURIComponent(projectPath)}&remoteName=${encodeURIComponent(remoteName)}`;

    return this.http.delete<any>(url).pipe(
      catchError(error => {
        console.error('Error removing remote:', error);
        return of({
          success: false,
          error: error.error?.error || error.message || 'Failed to remove remote'
        });
      })
    );
  }

  /**
   * Setup GitHub remote for repository (legacy - GitHub specific)
   */
  setupGitHubRemote(projectPath: string, organization: string, repositoryName: string,
                    saveOrganization: boolean = true, pushAfterAdd: boolean = true,
                    repositoryDescription?: string, isPrivate?: boolean): Observable<SetupRemoteResponse> {
    const request: SetupRemoteRequest = {
      repositoryPath: projectPath,
      organization: organization,
      repositoryName: repositoryName,
      repositoryDescription: repositoryDescription,
      isPrivate: isPrivate !== undefined ? isPrivate : true,
      saveOrganization: saveOrganization,
      pushAfterAdd: pushAfterAdd
    };
    const url = '../api/ModernGit/setup-remote';

    return this.http.post<SetupRemoteResponse>(url, request).pipe(
      catchError(error => {
        console.error('Error setting up remote:', error);
        return of({
          success: false,
          error: error.error?.error || error.message || 'Failed to setup remote'
        });
      })
    );
  }

  // #region Generic Remote Setup Methods

  /**
   * Parse a remote URL and detect provider
   */
  parseRemoteUrl(url: string): Observable<any> {
    const apiUrl = '../api/ModernGit/parse-remote-url';

    return this.http.post<any>(apiUrl, { url: url }).pipe(
      catchError(error => {
        console.error('Error parsing remote URL:', error);
        return of({
          isValid: false,
          error: error.error?.error || error.message || 'Failed to parse URL'
        });
      })
    );
  }

  /**
   * Validate remote with credentials
   */
  validateRemoteAuth(request: {
    remoteUrl: string;
    username?: string;
    password?: string;
    authMethod?: string;
  }): Observable<any> {
    const url = '../api/ModernGit/validate-remote-auth';

    return this.http.post<any>(url, request).pipe(
      catchError(error => {
        console.error('Error validating remote auth:', error);
        return of({
          isReachable: false,
          credentialsValid: false,
          error: error.error?.error || error.message || 'Failed to validate credentials'
        });
      })
    );
  }

  /**
   * Setup generic remote (supports any Git provider)
   */
  setupRemoteGeneric(request: {
    repositoryPath: string;
    remoteUrl: string;
    remoteName?: string;
    authMethod?: string;
    username?: string;
    password?: string;
    token?: string;
    saveCredentials?: boolean;
    pushAfterAdd?: boolean;
    createRemoteRepo?: boolean;
    repoDescription?: string;
    isPrivate?: boolean;
  }): Observable<any> {
    const url = '../api/ModernGit/setup-remote-generic';

    return this.http.post<any>(url, request).pipe(
      catchError(error => {
        console.error('Error setting up generic remote:', error);
        return of({
          success: false,
          error: error.error?.error || error.message || 'Failed to setup remote'
        });
      })
    );
  }

  // #endregion

  /**
   * Get saved GitHub organization
   */
  getGitHubOrganization(): Observable<string> {
    const url = '../api/ModernGit/github-organization';

    return this.http.get<{ organization: string }>(url).pipe(
      map(response => response.organization || ''),
      catchError(error => {
        console.error('Error getting GitHub organization:', error);
        return of('');
      })
    );
  }

  /**
   * Sets the GitHub personal access token
   */
  setGitHubToken(token: string): Observable<any> {
    const url = '../api/ModernGit/github-token';

    return this.http.post<any>(url, { token: token }).pipe(
      map(response => response),
      catchError(error => {
        console.error('Error setting GitHub token:', error);
        return of({ success: false });
      })
    );
  }

  /**
   * Gets the GitHub token status (masked)
   */
  getGitHubToken(): Observable<any> {
    const url = '../api/ModernGit/github-token';

    return this.http.get<any>(url).pipe(
      catchError(error => {
        console.error('Error getting GitHub token:', error);
        return of({ hasToken: false, maskedToken: '', tokenValid: false });
      })
    );
  }

  /**
   * Tests the GitHub token validity
   */
  testGitHubToken(): Observable<any> {
    const url = '../api/ModernGit/test-github-token';

    return this.http.post<any>(url, {}).pipe(
      map(response => response),
      catchError(error => {
        console.error('Error testing GitHub token:', error);
        return of({ success: false, tokenValid: false });
      })
    );
  }

  /**
   * Save GitHub organization for future use
   */
  saveGitHubOrganization(organization: string): Observable<boolean> {
    const url = '../api/ModernGit/github-organization';

    return this.http.post<{ success: boolean }>(url, { organization: organization }).pipe(
      map(response => response.success),
      catchError(error => {
        console.error('Error saving GitHub organization:', error);
        return of(false);
      })
    );
  }

  /**
   * Get list of all branches (local and remote)
   */
  getBranches(projectPath: string, includeRemote: boolean = true): Observable<BranchInfo[]> {
    const url = `../api/ModernGit/branches?repositoryPath=${encodeURIComponent(projectPath)}&includeRemote=${includeRemote}`;

    return this.http.get<BranchInfo[]>(url).pipe(
      catchError(error => {
        console.error('Error getting branches:', error);
        return of([]);
      })
    );
  }

  /**
   * Checkout/switch to a different branch
   * Sends connectionId to backend for client-specific SignalR notifications
   */
  checkoutBranch(projectPath: string, branchName: string, connectionId?: string): Observable<CheckoutResult> {
    const url = '../api/ModernGit/checkout';
    const request = {
      repositoryPath: projectPath,
      branchName: branchName,
      connectionId: connectionId || null // Include SignalR connectionId if provided
    };

    if (connectionId) {
      console.log('🔄 Checkout branch request with connectionId:', connectionId);
    }

    return this.http.post<CheckoutResult>(url, request).pipe(
      catchError(error => {
        console.error('Error checking out branch:', error);
        return of({
          success: false,
          error: error.error?.error || error.message || 'Failed to checkout branch'
        });
      })
    );
  }

  /**
   * Get repository status (for checking uncommitted changes)
   */
  getRepositoryStatus(projectPath: string): Observable<any> {
    const url = `../api/ModernGit/status?repositoryPath=${encodeURIComponent(projectPath)}`;

    return this.http.get<any>(url).pipe(
      catchError(error => {
        console.error('Error getting repository status:', error);
        return of({ hasChanges: false, files: [] });
      })
    );
  }

  /**
   * Get list of all changed files in the repository
   */
  getChangedFiles(projectPath: string): Observable<ChangedFilesResponse> {
    const url = `../api/ModernGitToolbar/changed-files?projectPath=${encodeURIComponent(projectPath)}`;

    return this.http.get<ChangedFilesResponse>(url).pipe(
      catchError(error => {
        console.error('Error getting changed files:', error);
        return of({ files: [], totalCount: 0 });
      })
    );
  }

  /**
   * Discard changes to a file (restore from HEAD) or unstage a new file
   */
  discardFile(projectPath: string, filePath: string, isNew: boolean): Observable<DiscardFileResponse> {
    const url = '../api/ModernGitToolbar/discard-file';
    const request = {
      projectPath: projectPath,
      filePath: filePath,
      isNew: isNew
    };

    return this.http.post<DiscardFileResponse>(url, request).pipe(
      catchError(error => {
        console.error('Error discarding file:', error);
        return of({
          success: false,
          errorMessage: error.error?.errorMessage || error.message || 'Failed to discard file',
          filePath: filePath
        });
      })
    );
  }

  /**
   * Delete an untracked/new file from disk
   */
  deleteFile(projectPath: string, filePath: string): Observable<DiscardFileResponse> {
    const url = '../api/ModernGitToolbar/delete-file';
    const request = {
      projectPath: projectPath,
      filePath: filePath,
      isNew: true
    };

    return this.http.post<DiscardFileResponse>(url, request).pipe(
      catchError(error => {
        console.error('Error deleting file:', error);
        return of({
          success: false,
          errorMessage: error.error?.errorMessage || error.message || 'Failed to delete file',
          filePath: filePath
        });
      })
    );
  }

  /**
   * Adapts modern Git response to legacy format for backward compatibility
   */
  private adaptModernResponseToLegacy(response: ModernGitResponse): ModernResponsePull {
    return {
      isConnectionMissing: false, // Modern service handles connection issues differently
      isAuthenticationMissing: false, // Modern service uses native authentication
      thereAreConflicts: response.thereAreConflicts,
      errorMessage: response.errorMessage,
      whatFilesWillBeChanged: response.changedFiles || []
    };
  }

  /**
   * Cleanup quando il service viene distrutto
   */
  ngOnDestroy(): void {
    this.stopPolling();
    
    // Rimuovi event listeners per evitare memory leak
    document.removeEventListener('visibilitychange', this.handleVisibilityChange);
    window.removeEventListener('focus', this.handleWindowFocus);
    window.removeEventListener('blur', this.handleWindowBlur);
    
  }

}
