import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';

export interface IdeConfiguration {
  selectedIde: string;
  vscodePath: string;
  intellijPath: string;
}

export interface SetIdeConfigurationRequest {
  selectedIde: string;
  projectPath?: string;
}

@Injectable({
  providedIn: 'root'
})
export class IdeConfigurationService {
  private apiUrl = '../api/ideConfiguration';
  private currentConfigSubject = new BehaviorSubject<IdeConfiguration | null>(null);
  public currentConfig$ = this.currentConfigSubject.asObservable();

  constructor(private http: HttpClient) {}

  /**
   * Get IDE configuration for a project
   * @param projectPath Optional project path. If not provided, uses current project.
   */
  getIdeConfiguration(projectPath?: string): Observable<IdeConfiguration> {
    const params = projectPath ? { projectPath } : {};
    return this.http.get<IdeConfiguration>(`${this.apiUrl}/config`, { params }).pipe(
      tap(config => this.currentConfigSubject.next(config))
    );
  }

  /**
   * Set IDE configuration for a project
   */
  setIdeConfiguration(request: SetIdeConfigurationRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/config`, request).pipe(
      tap(() => {
        // Reload configuration after setting
        this.getIdeConfiguration(request.projectPath).subscribe();
      })
    );
  }

  /**
   * Get current configuration from subject (synchronous)
   */
  getCurrentConfig(): IdeConfiguration | null {
    return this.currentConfigSubject.value;
  }
}
