import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap, filter } from 'rxjs/operators';
import {
  CompatibilityConfig,
  CompatibilityMode,
  SetCompatibilityModeRequest,
  ValidateDocumentRequest,
  ValidateDocumentResponse
} from '../models/compatibility-mode.model';
import { ProjectsService } from '../md-explorer/services/projects.service';
import { MdServerMessagesService } from '../signalR/services/server-messages.service';

@Injectable({
  providedIn: 'root'
})
export class CompatibilityModeService {
  private readonly apiUrl = '/api/compatibility';

  // Observable per il mode corrente
  private currentModeSubject = new BehaviorSubject<CompatibilityMode>(CompatibilityMode.MdExplorer);
  public currentMode$ = this.currentModeSubject.asObservable();

  private configSubject = new BehaviorSubject<CompatibilityConfig | null>(null);
  public config$ = this.configSubject.asObservable();

  constructor(
    private http: HttpClient,
    private projectsService: ProjectsService,
    private serverMessages: MdServerMessagesService
  ) {
    // Load initial mode
    this.loadCurrentMode();

    // Reload when project changes
    this.projectsService.currentProjects$
      .pipe(filter(project => project != null))
      .subscribe(() => {
        console.log('Project changed, reloading compatibility mode');
        this.loadCurrentMode();
      });
  }

  /**
   * Load current compatibility mode from server
   */
  loadCurrentMode(): void {
    // Reset to default before loading to avoid keeping old project's mode
    this.currentModeSubject.next(CompatibilityMode.MdExplorer);

    // Skip if connectionId not yet available (will be called again when project changes)
    const connectionId = this.serverMessages.connectionId;
    if (!connectionId) {
      console.log('ConnectionId not yet available, skipping compatibility mode load');
      return;
    }

    this.http.get<CompatibilityConfig>(`${this.apiUrl}/mode`)
      .subscribe({
        next: (config) => {
          console.log('Loaded compatibility config from backend:', config);

          this.configSubject.next(config);

          // Ensure mode is set, default to mdexplorer if missing
          const mode = config?.mode?.toLowerCase() || 'mdexplorer';
          console.log('Setting compatibility mode to:', mode);

          const compatMode = mode === 'github' ? CompatibilityMode.GitHub :
                             mode === 'commonmark' ? CompatibilityMode.CommonMark :
                             CompatibilityMode.MdExplorer;

          this.currentModeSubject.next(compatMode);
        },
        error: (error) => {
          console.error('Error loading compatibility mode:', error);
          // Default to mdexplorer on error
          this.currentModeSubject.next(CompatibilityMode.MdExplorer);
        }
      });
  }

  /**
   * Get current compatibility mode
   * @param projectPath Optional project path (for project settings dialog)
   */
  getCurrentMode(projectPath?: string): Observable<CompatibilityConfig> {
    const params: any = {};
    if (projectPath) {
      params.projectPath = projectPath;
    }
    return this.http.get<CompatibilityConfig>(`${this.apiUrl}/mode`, { params });
  }

  /**
   * Set compatibility mode
   */
  setCompatibilityMode(request: SetCompatibilityModeRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/mode`, request).pipe(
      tap(() => {
        // Reload mode after setting
        this.loadCurrentMode();
      })
    );
  }

  /**
   * Validate document for GitHub compatibility
   */
  validateDocument(request: ValidateDocumentRequest): Observable<ValidateDocumentResponse> {
    return this.http.post<ValidateDocumentResponse>(`${this.apiUrl}/validate`, request);
  }

  /**
   * Check if current mode is GitHub
   */
  isGitHubMode(): boolean {
    return this.currentModeSubject.value === CompatibilityMode.GitHub;
  }

  /**
   * Check if current mode is MdExplorer
   */
  isMdExplorerMode(): boolean {
    return this.currentModeSubject.value === CompatibilityMode.MdExplorer;
  }

  /**
   * Get current mode value synchronously
   */
  getCurrentModeValue(): CompatibilityMode {
    return this.currentModeSubject.value;
  }

  /**
   * Update mode directly (used when opening a project)
   */
  updateMode(mode: CompatibilityMode): void {
    console.log('Updating compatibility mode directly to:', mode);
    this.currentModeSubject.next(mode);
  }

  /**
   * Get mode display name
   */
  getModeDisplayName(mode: CompatibilityMode): string {
    switch (mode) {
      case CompatibilityMode.GitHub:
        return 'GitHub Compatible';
      case CompatibilityMode.CommonMark:
        return 'CommonMark';
      case CompatibilityMode.MdExplorer:
      default:
        return 'MdExplorer';
    }
  }

  /**
   * Get mode icon
   */
  getModeIcon(mode: CompatibilityMode): string {
    switch (mode) {
      case CompatibilityMode.GitHub:
        return 'public'; // Material icon for public/github
      case CompatibilityMode.CommonMark:
        return 'article'; // Material icon for document
      case CompatibilityMode.MdExplorer:
      default:
        return 'rocket_launch'; // Material icon for advanced features
    }
  }

  /**
   * Get mode color
   */
  getModeColor(mode: CompatibilityMode): string {
    switch (mode) {
      case CompatibilityMode.GitHub:
        return 'accent'; // Blue/green for GitHub
      case CompatibilityMode.CommonMark:
        return 'warn'; // Orange for standard
      case CompatibilityMode.MdExplorer:
      default:
        return 'primary'; // Primary theme color
    }
  }
}
