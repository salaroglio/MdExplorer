import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ServiceDto {
  id: string;
  blockId: string;
  projectPath: string;
  language: string;
  codePreview: string;
  pid: number;
  startedAt: string;
  status: string;          // running | exited | killed
  exitCode: number | null;
  detectedPort: number | null;
  uptimeMs: number;
}

/**
 * Talks to MdServicesController to list and stop long-running services started from
 * runnable fenced code blocks ("Run as service"). Used by the Settings > Services panel.
 */
@Injectable({ providedIn: 'root' })
export class ServicesMonitorService {
  private readonly baseUrl = '../api/MdServices';

  constructor(private http: HttpClient) {}

  list(): Observable<ServiceDto[]> {
    return this.http.get<ServiceDto[]>(`${this.baseUrl}/Services`);
  }

  stop(serviceId: string): Observable<{ stopped: boolean }> {
    return this.http.post<{ stopped: boolean }>(`${this.baseUrl}/StopService`, { serviceId });
  }
}
