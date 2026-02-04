import { Injectable, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Observable, BehaviorSubject, Subject } from 'rxjs';

export interface TransferInfo {
  infoHash: string;
  name: string;
  progress: number;
  downloadSpeed: number;
  uploadSpeed: number;
  peers: number;
  status: string;
  size: number;
  downloaded: number;
  uploaded: number;
  eta: number;
  magnetUri?: string;
  path?: string;
  isSeeding: boolean;
  isDownloading: boolean;
}

export interface ShareResult {
  success: boolean;
  infoHash?: string;
  magnetUri?: string;
  name?: string;
  size?: number;
  error?: string;
}

export interface P2PStats {
  activeTransfers: number;
  totalDownloaded: number;
  totalUploaded: number;
  downloadSpeed: number;
  uploadSpeed: number;
  peersConnected: number;
}

export interface P2PStatus {
  enabled: boolean;
  httpRunning: boolean;
  stats?: P2PStats;
}

export interface PeerStatus {
  found: boolean;
  status: 'seeding' | 'seeding_no_peers' | 'downloading' | 'downloading_no_peers' | 'completed' | 'unknown';
  numPeers: number;
  downloadSpeed?: number;
  uploadSpeed?: number;
  progress?: number;
  size?: number;
  name?: string;
  timeRemaining?: number;
  message?: string;
}

export interface P2PFileInfo {
  found: boolean;
  magnetUri?: string;
  infoHash?: string;
  size?: number;
  addedAt?: string;
  error?: string;
}

export interface P2PMetadata {
  files: { [filename: string]: P2PFileInfo };
}

export interface P2PProjectFile {
  filename: string;
  magnetUri?: string;
  infoHash?: string;
  size?: number;
  addedAt?: string;
}

export interface P2PProject {
  id: string;
  name: string;
  path: string;
  files: { [filename: string]: P2PProjectFile } | any;
}

export interface RestoreSeedingResult {
  message: string;
  restored: number;
  errors?: string[];
}

export interface TrackerInfo {
  url: string;
  reachable: boolean;
  authenticated: boolean;
  error?: string;
  latency?: number;
}

export interface TrackerOverallStatus {
  reachable: boolean;
  authenticated: boolean;
  status: 'connected' | 'unauthorized' | 'unreachable';
}

export interface TrackerStatusResponse {
  hasToken: boolean;
  trackers: TrackerInfo[];
  overall: TrackerOverallStatus;
}

@Injectable({
  providedIn: 'root'
})
export class P2PService implements OnDestroy {
  private hubConnection: HubConnection | null = null;
  private baseUrl = '/api/P2P';

  // Observables for state
  private _status$ = new BehaviorSubject<P2PStatus | null>(null);
  public status$ = this._status$.asObservable();

  private _transfers$ = new BehaviorSubject<TransferInfo[]>([]);
  public transfers$ = this._transfers$.asObservable();

  private _transferProgress$ = new Subject<TransferInfo>();
  public transferProgress$ = this._transferProgress$.asObservable();

  private _transferComplete$ = new Subject<TransferInfo>();
  public transferComplete$ = this._transferComplete$.asObservable();

  private _transferError$ = new Subject<{ infoHash: string; error: string }>();
  public transferError$ = this._transferError$.asObservable();

  // New observables for peer activity feedback
  private _peerConnected$ = new Subject<{
    infoHash: string;
    peerAddress: string;
    numPeers: number;
    torrentName: string;
    timestamp: string;
  }>();
  public peerConnected$ = this._peerConnected$.asObservable();

  private _uploadActivity$ = new Subject<{
    infoHash: string;
    bytes: number;
    uploadSpeed: number;
    totalUploaded: number;
    numPeers: number;
    torrentName: string;
    timestamp: string;
  }>();
  public uploadActivity$ = this._uploadActivity$.asObservable();

  // Aggregated state for UI widget
  private _activeUploads$ = new BehaviorSubject<number>(0);
  public activeUploads$ = this._activeUploads$.asObservable();

  private _totalPeers$ = new BehaviorSubject<number>(0);
  public totalPeers$ = this._totalPeers$.asObservable();

  private _isAvailable$ = new BehaviorSubject<boolean>(false);
  public isAvailable$ = this._isAvailable$.asObservable();

  constructor(private http: HttpClient) {
    this.checkAvailability();
  }

  /**
   * Check if P2P service is available (Premium module loaded and Electron plugin running)
   */
  checkAvailability(): void {
    this.http.get<P2PStatus>(`${this.baseUrl}/status`).subscribe({
      next: (status) => {
        this._status$.next(status);
        this._isAvailable$.next(status?.enabled || false);

        if (status?.enabled) {
          this.initializeSignalR();
          this.refreshTransfers();
          // Auto-restore seeding for all projects with P2P files
          this.autoRestoreAll();
        }
      },
      error: () => {
        this._isAvailable$.next(false);
        this._status$.next(null);
      }
    });
  }

  /**
   * Auto-restore seeding for all projects that have P2P metadata
   * Called automatically when P2P service becomes available
   */
  private autoRestoreAll(): void {
    // First get all projects from MdExplorer
    this.http.get<{ id: string; name: string; path: string }[]>('../api/MdProjects/GetProjects').subscribe({
      next: (projects) => {
        if (projects && projects.length > 0) {
          // Send to P2P plugin to restore seeding
          this.http.post<{ total: number; withP2P: number; restored: number }>(`${this.baseUrl}/auto-restore-all`, { projects }).subscribe({
            next: (result) => {
              if (result.restored > 0) {
                console.log(`[P2P] Auto-restored ${result.restored} files from ${result.withP2P} projects`);
                this.refreshTransfers();
              }
            },
            error: (err) => {
              console.error('[P2P] Auto-restore error:', err);
            }
          });
        }
      },
      error: (err) => {
        console.error('[P2P] Failed to get projects for auto-restore:', err);
      }
    });
  }

  private initializeSignalR(): void {
    if (this.hubConnection) {
      return; // Already initialized
    }

    this.hubConnection = new HubConnectionBuilder()
      .withUrl('/signalr/p2p')
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect()
      .build();

    // Setup event handlers
    this.hubConnection.on('TransferProgress', (transfer: TransferInfo) => {
      this._transferProgress$.next(transfer);
      this.updateTransferInList(transfer);
    });

    this.hubConnection.on('TransferComplete', (transfer: TransferInfo) => {
      this._transferComplete$.next(transfer);
      this.updateTransferInList(transfer);
    });

    this.hubConnection.on('TransferError', (data: { infoHash: string; error: string }) => {
      this._transferError$.next(data);
    });

    // Peer connected event - when a remote peer connects to download from us
    this.hubConnection.on('PeerConnected', (data: {
      infoHash: string;
      peerAddress: string;
      numPeers: number;
      torrentName: string;
      timestamp: string;
    }) => {
      console.log('[P2PService] Peer connected:', data);
      this._peerConnected$.next(data);
      this._totalPeers$.next(data.numPeers);
    });

    // Upload activity event - periodic updates while uploading
    this.hubConnection.on('UploadActivity', (data: {
      infoHash: string;
      bytes: number;
      uploadSpeed: number;
      totalUploaded: number;
      numPeers: number;
      torrentName: string;
      timestamp: string;
    }) => {
      console.log('[P2PService] Upload activity:', data);
      this._uploadActivity$.next(data);
      this._totalPeers$.next(data.numPeers);
      // Count active uploads (those with speed > 0)
      if (data.uploadSpeed > 0) {
        this._activeUploads$.next(1); // Simplified: at least one active
      }
    });

    // Start connection
    this.startConnection();
  }

  private async startConnection(): Promise<void> {
    if (!this.hubConnection) return;

    try {
      await this.hubConnection.start();
      console.log('[P2PService] SignalR connection established');

      // Subscribe to all transfer updates
      await this.hubConnection.invoke('SubscribeToAllTransfers');
    } catch (err) {
      console.error('[P2PService] Error establishing SignalR connection:', err);
      setTimeout(() => this.startConnection(), 5000);
    }
  }

  private updateTransferInList(transfer: TransferInfo): void {
    const transfers = this._transfers$.value;
    const index = transfers.findIndex(t => t.infoHash === transfer.infoHash);

    if (index >= 0) {
      transfers[index] = transfer;
    } else {
      transfers.push(transfer);
    }

    this._transfers$.next([...transfers]);
  }

  // API Methods

  /**
   * Get P2P service status
   */
  getStatus(): Observable<P2PStatus> {
    return this.http.get<P2PStatus>(`${this.baseUrl}/status`);
  }

  /**
   * Get P2P health check
   */
  getHealth(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/health`);
  }

  /**
   * Get tracker connectivity status
   * Returns information about whether the tracker is reachable and authenticated
   */
  getTrackerStatus(): Observable<TrackerStatusResponse> {
    return this.http.get<TrackerStatusResponse>(`${this.baseUrl}/tracker-status`);
  }

  /**
   * Get P2P statistics
   */
  getStats(): Observable<P2PStats> {
    return this.http.get<P2PStats>(`${this.baseUrl}/stats`);
  }

  /**
   * Get all active transfers
   */
  getTransfers(): Observable<TransferInfo[]> {
    return this.http.get<TransferInfo[]>(`${this.baseUrl}/transfers`);
  }

  /**
   * Refresh the transfers list
   */
  refreshTransfers(): void {
    this.getTransfers().subscribe({
      next: (transfers) => {
        this._transfers$.next(transfers);
      },
      error: (err) => {
        console.error('[P2PService] Error fetching transfers:', err);
      }
    });
  }

  /**
   * Get a specific transfer by info hash
   */
  getTransfer(infoHash: string): Observable<TransferInfo> {
    return this.http.get<TransferInfo>(`${this.baseUrl}/transfers/${infoHash}`);
  }

  /**
   * Share a file via P2P
   * @param filePath Full path to the file to share
   * @param name Optional display name
   */
  shareFile(filePath: string, name?: string): Observable<ShareResult> {
    return this.http.post<ShareResult>(`${this.baseUrl}/share`, { filePath, name });
  }

  /**
   * Download from a magnet link
   * @param magnetUri Magnet URI to download
   * @param destPath Optional destination path
   */
  download(magnetUri: string, destPath?: string): Observable<ShareResult> {
    return this.http.post<ShareResult>(`${this.baseUrl}/download`, { magnetUri, destPath });
  }

  /**
   * Pause a transfer
   */
  pauseTransfer(infoHash: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/transfers/${infoHash}/pause`, {});
  }

  /**
   * Resume a transfer
   */
  resumeTransfer(infoHash: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/transfers/${infoHash}/resume`, {});
  }

  /**
   * Stop and remove a transfer
   * @param deleteFiles Whether to delete the downloaded files
   */
  stopTransfer(infoHash: string, deleteFiles: boolean = false): Observable<any> {
    return this.http.delete(`${this.baseUrl}/transfers/${infoHash}?deleteFiles=${deleteFiles}`);
  }

  /**
   * Parse a magnet URI to get info before downloading
   */
  parseMagnet(magnetUri: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/parse-magnet`, { magnetUri });
  }

  /**
   * Copy a file to .p2pshare/files/, start seeding it, and append a P2P link to the markdown document.
   * This is the main method for the "Add file to share via P2P" feature.
   * @param sourcePath Full path to the source file to copy and share
   * @param documentPath Full path to the markdown document where the link will be appended
   */
  copyAndShareFile(sourcePath: string, documentPath: string): Observable<ShareResult> {
    return this.http.post<ShareResult>(`${this.baseUrl}/copy-and-share`, {
      sourcePath,
      documentPath
    });
  }

  /**
   * Check if a file exists at a relative path within the project
   * @param path Relative path to check (e.g., ".p2pshare/files/video.mp4")
   * @param projectPath Full path to the project root
   */
  checkFile(path: string, projectPath: string): Observable<{ exists: boolean; fullPath: string }> {
    return this.http.get<{ exists: boolean; fullPath: string }>(
      `${this.baseUrl}/check-file?path=${encodeURIComponent(path)}&projectPath=${encodeURIComponent(projectPath)}`
    );
  }

  /**
   * Get peer status for a specific torrent by infoHash.
   * Returns number of peers, download/upload speeds, and transfer status.
   * @param infoHash The torrent info hash
   */
  getPeerStatus(infoHash: string): Observable<PeerStatus> {
    return this.http.get<PeerStatus>(`${this.baseUrl}/peer-status/${infoHash}`);
  }

  /**
   * Get P2P metadata for a project.
   * Returns the contents of .p2pshare/metadata.json if it exists.
   * @param projectPath Full path to the project root
   */
  getMetadata(projectPath: string): Observable<P2PMetadata> {
    return this.http.get<P2PMetadata>(
      `${this.baseUrl}/metadata?projectPath=${encodeURIComponent(projectPath)}`
    );
  }

  /**
   * Get P2P info for a specific file by filename.
   * Returns magnetUri, infoHash, size from metadata.json.
   * @param filename The filename to look up
   * @param projectPath Full path to the project root
   */
  getFileInfo(filename: string, projectPath: string): Observable<P2PFileInfo> {
    return this.http.get<P2PFileInfo>(
      `${this.baseUrl}/file-info/${encodeURIComponent(filename)}?projectPath=${encodeURIComponent(projectPath)}`
    );
  }

  /**
   * Get all projects that have P2P sharing enabled, along with their metadata.
   * Accepts a list of projects and returns only those that have .p2pshare/metadata.json.
   * Used by the P2P Manager to show files organized by project.
   * @param projects List of projects with id, name, and path
   */
  getProjectsWithP2P(projects: { id: string; name: string; path: string }[]): Observable<P2PProject[]> {
    return this.http.post<P2PProject[]>(`${this.baseUrl}/projects-with-p2p`, { projects });
  }

  /**
   * Restore seeding for all files in a project's metadata.json.
   * Called when opening a project to resume P2P sharing.
   * @param projectPath Full path to the project root
   */
  restoreSeeding(projectPath: string): Observable<RestoreSeedingResult> {
    return this.http.post<RestoreSeedingResult>(`${this.baseUrl}/restore-seeding`, { projectPath });
  }

  /**
   * Format bytes to human readable string
   */
  formatBytes(bytes: number, decimals: number = 2): string {
    if (bytes === 0) return '0 B';

    const k = 1024;
    const dm = decimals < 0 ? 0 : decimals;
    const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];

    const i = Math.floor(Math.log(bytes) / Math.log(k));

    return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
  }

  /**
   * Format speed to human readable string
   */
  formatSpeed(bytesPerSecond: number): string {
    return this.formatBytes(bytesPerSecond) + '/s';
  }

  /**
   * Format ETA to human readable string
   */
  formatEta(seconds: number): string {
    if (seconds <= 0 || !isFinite(seconds)) return '--';

    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const secs = Math.floor(seconds % 60);

    if (hours > 0) {
      return `${hours}h ${minutes}m`;
    } else if (minutes > 0) {
      return `${minutes}m ${secs}s`;
    } else {
      return `${secs}s`;
    }
  }

  ngOnDestroy(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }
}
