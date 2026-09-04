import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { MatLegacyDialog as MatDialog } from '@angular/material/legacy-dialog';
import { firstValueFrom } from 'rxjs';

import { MdServerMessagesService } from '../signalR/services/server-messages.service';
import {
  RunCommandDialogComponent,
  RunCommandDialogData,
  RunCommandDialogResult,
} from '../commons/components/run-command-dialog/run-command-dialog.component';
import { ShowFileSystemComponent } from '../commons/components/show-file-system/show-file-system.component';
import { ShowFileMetadata } from '../commons/components/show-file-system/show-file-metadata';
import { TranslateService } from '@ngx-translate/core';

interface RunRequestPayload {
  type: 'mde-exec.requestRun';
  blockId: string;
  lang: string;
  code: string;
  params: Array<{ name: string; defaultValue: string; isSecret: boolean; description?: string; kind?: string; options?: string[] }>;
  paramsInline?: boolean;
  projectPath: string;
  documentPath?: string;
  mode?: 'batch' | 'service';
}

/**
 * Orchestrates the "runnable fenced code block" feature:
 *   - Listens for `mde-exec.requestRun` postMessage events from the markdown iframe.
 *   - Enforces per-project trust (one-time, confirmed in the same parameter dialog).
 *   - Calls /api/MdExecution/Run with the filled-in parameters.
 *   - Forwards SignalR streaming events (execution.output/completed/error) back to the iframe.
 */
@Injectable({ providedIn: 'root' })
export class ExecutionService {
  private readonly baseUrl = '../api/MdExecution';
  private readonly serviceBaseUrl = '../api/MdServices';
  private readonly trustCache = new Map<string, boolean>();

  // While a block is running we remember which iframe originated it so that
  // streaming chunks go back to the right Window (also fine to broadcast,
  // but targeted postMessage is cheaper and less noisy in devtools).
  private readonly runningBlocks = new Map<string, Window>();

  // Tracks which block currently owns a running service (blockId -> serviceId),
  // used for the "already running — stop and restart?" guard.
  private readonly runningServicesByBlock = new Map<string, string>();

  constructor(
    private http: HttpClient,
    private dialog: MatDialog,
    private serverMessages: MdServerMessagesService,
    private translate: TranslateService,
  ) {
    this.setupIframeListener();
    this.setupSignalRListeners();
  }

  private setupIframeListener(): void {
    window.addEventListener('message', (event: MessageEvent) => {
      const data = event.data;
      if (!data || !data.type) return;
      const src = (event.source as Window) || null;

      if (data.type === 'mde-exec.requestRun') {
        // Fire-and-forget — do not await so that we never block the message loop.
        this.handleRunRequest(src, data).catch(err => {
          console.error('[ExecutionService] Unhandled error:', err);
          this.postToIframe(src, 'mde-exec.error', {
            blockId: data.blockId,
            message: err?.message || 'Unexpected error',
          });
        });
      } else if (data.type === 'mde-exec.requestStopService') {
        this.handleStopServiceRequest(src, data).catch(err => {
          console.error('[ExecutionService] Stop service error:', err);
          this.postToIframe(src, 'mde-exec.error', {
            blockId: data.blockId,
            message: err?.message || 'Failed to stop service',
          });
        });
      } else if (data.type === 'mde-exec.queryServices') {
        // An iframe (re)loaded and wants to know which of its blocks have a service running,
        // so they can re-render in the Stop state instead of resetting to Run.
        this.handleQueryServices(src, data).catch(err =>
          console.error('[ExecutionService] queryServices error:', err));
      } else if (data.type === 'mde-exec.requestPathPicker') {
        this.handlePathPickerRequest(src, data).catch(err => {
          console.error('[ExecutionService] Path picker error:', err);
          // Picker errors are non-fatal — silently send a null path so the iframe just leaves
          // the existing value in place.
          this.postToIframe(src, 'mde-exec.pathPicked', {
            blockId: data.blockId,
            paramName: data.paramName,
            path: null,
          });
        });
      }
    });
  }

  private async handlePathPickerRequest(
    source: Window | null,
    req: { blockId: string; paramName: string; mode: string; projectPath: string; currentValue?: string },
  ): Promise<void> {
    const isDir = req.mode === 'dir' || req.mode === 'folder' || req.mode === 'directory';
    const isOutFile = req.mode === 'out-file' || req.mode === 'output-file' || req.mode === 'save-file';

    const data = new ShowFileMetadata();
    // The dialog interprets 'root' as the current project root, regardless of OS.
    // It uses HTTP-backed listing so it works the same in browser and Electron, on Win/Linux/macOS.
    data.start = 'root';

    if (isOutFile) {
      // Save-As flow: user picks the destination folder + types the filename. The dialog
      // composes `<folder><sep><filename>` and returns the full path in afterClosed.data.
      data.title = 'Save output file';
      data.typeOfSelection = 'Folders';
      data.saveAs = true;
      data.buttonText = 'Save';
      // Pre-fill the filename field with the existing value's basename, if any.
      const current = req.currentValue || '';
      const m = current.match(/[\\/]([^\\/]+)$/);
      data.defaultFileName = m ? m[1] : current;
    } else {
      data.title = isDir ? 'Select a folder' : 'Select a file';
      data.typeOfSelection = isDir ? 'Folders' : 'FoldersAndFiles';
      data.buttonText = isDir ? 'Select folder' : 'Select file';
    }

    const ref = this.dialog.open(ShowFileSystemComponent, {
      width: '900px',
      height: '700px',
      data,
    });
    const result = await firstValueFrom(ref.afterClosed());
    const path: string | null = result?.data || null;

    this.postToIframe(source, 'mde-exec.pathPicked', {
      blockId: req.blockId,
      paramName: req.paramName,
      path,
    });
  }

  private setupSignalRListeners(): void {
    this.serverMessages.executionOutput$.subscribe(data => {
      this.dispatch(data.blockId, 'mde-exec.output', data);
    });
    this.serverMessages.executionCompleted$.subscribe(data => {
      this.dispatch(data.blockId, 'mde-exec.completed', data);
      this.runningBlocks.delete(data.blockId);
    });
    this.serverMessages.executionError$.subscribe(data => {
      this.dispatch(data.blockId, 'mde-exec.error', data);
      this.runningBlocks.delete(data.blockId);
    });

    // Long-running services
    this.serverMessages.serviceStarted$.subscribe(dto => {
      if (dto?.blockId) {
        this.runningServicesByBlock.set(dto.blockId, dto.id);
        this.dispatch(dto.blockId, 'mde-exec.serviceStarted', { blockId: dto.blockId, serviceId: dto.id, pid: dto.pid });
      }
    });
    this.serverMessages.serviceOutput$.subscribe(data => {
      this.dispatch(data.blockId, 'mde-exec.output', data);
    });
    this.serverMessages.serviceStopped$.subscribe(dto => {
      if (dto?.blockId) {
        this.runningServicesByBlock.delete(dto.blockId);
        this.dispatch(dto.blockId, 'mde-exec.serviceStopped', {
          blockId: dto.blockId, serviceId: dto.id, exitCode: dto.exitCode, status: dto.status,
        });
        this.runningBlocks.delete(dto.blockId);
      }
    });
  }

  private async handleStopServiceRequest(
    source: Window | null,
    req: { blockId: string; serviceId: string | null },
  ): Promise<void> {
    const serviceId = req.serviceId || this.runningServicesByBlock.get(req.blockId);
    if (!serviceId) return;
    await firstValueFrom(this.http.post(`${this.serviceBaseUrl}/StopService`, { serviceId }));
    // The serviceStopped$ SignalR event will revert the block UI.
  }

  private async handleRunRequest(source: Window | null, req: RunRequestPayload): Promise<void> {
    const projectPath = (req.projectPath || '').trim();
    if (!projectPath) {
      this.postToIframe(source, 'mde-exec.error', {
        blockId: req.blockId,
        message: 'No project path available for this document',
      });
      return;
    }

    const trusted = await this.fetchTrust(projectPath);

    // When params come from inline toolbar controls, the user has already typed values
    // into the rendered block — we just collect them and skip the params dialog. The
    // trust dialog still runs on the first execution in a project (without the params
    // section, which would be redundant).
    const inline = !!req.paramsInline;
    const declaredParams = req.params || [];
    const parametersFromInline: { [name: string]: string } = {};
    declaredParams.forEach(p => { parametersFromInline[p.name] = p.defaultValue || ''; });

    const needsParamsDialog = !inline && declaredParams.length > 0;
    const needsTrustDialog = !trusted;

    let parametersForRun: { [name: string]: string } = parametersFromInline;
    let userTrustConfirmed = trusted;

    if (needsParamsDialog || needsTrustDialog) {
      const dialogData: RunCommandDialogData = {
        language: req.lang,
        code: req.code,
        params: needsParamsDialog ? declaredParams : [],
        needsTrust: needsTrustDialog,
        projectPath,
      };
      const ref = this.dialog.open<RunCommandDialogComponent, RunCommandDialogData, RunCommandDialogResult>(
        RunCommandDialogComponent,
        { data: dialogData, width: '560px', autoFocus: true, restoreFocus: true },
      );
      const result = await firstValueFrom(ref.afterClosed());

      if (!result || !result.confirmed) {
        this.postToIframe(source, 'mde-exec.cancelled', { blockId: req.blockId });
        return;
      }

      if (needsParamsDialog) {
        parametersForRun = result.parameters || {};
      }

      if (needsTrustDialog) {
        if (!result.trustProject) {
          this.postToIframe(source, 'mde-exec.denied', {
            blockId: req.blockId,
            reason: 'Execution not enabled for this project',
          });
          return;
        }
        try {
          await firstValueFrom(this.http.post(`${this.baseUrl}/SetTrust`, { projectPath, trusted: true }));
          this.trustCache.set(projectPath, true);
          userTrustConfirmed = true;
        } catch (err: any) {
          this.postToIframe(source, 'mde-exec.error', {
            blockId: req.blockId,
            message: 'Failed to persist trust: ' + (err?.message || err),
          });
          return;
        }
      }
    }

    if (!userTrustConfirmed) {
      this.postToIframe(source, 'mde-exec.denied', {
        blockId: req.blockId,
        reason: 'Execution not enabled for this project',
      });
      return;
    }

    // Remember the iframe so streamed chunks can be routed back.
    if (source) this.runningBlocks.set(req.blockId, source);

    if (req.mode === 'service') {
      await this.startService(source, req, projectPath, parametersForRun);
      return;
    }

    try {
      await firstValueFrom(this.http.post(`${this.baseUrl}/Run`, {
        blockId: req.blockId,
        language: req.lang,
        code: req.code,
        projectPath,
        parameters: parametersForRun,
      }));
    } catch (err: any) {
      const message = err?.error?.error || err?.message || 'Execution failed';
      this.postToIframe(source, 'mde-exec.error', { blockId: req.blockId, message });
      this.runningBlocks.delete(req.blockId);
    }
  }

  private async startService(
    source: Window | null,
    req: RunRequestPayload,
    projectPath: string,
    parameters: { [name: string]: string },
  ): Promise<void> {
    // Duplicate guard: if a service is already running for this block, ask to stop & restart.
    const existing = this.runningServicesByBlock.get(req.blockId);
    if (existing) {
      const ok = window.confirm(
        this.translate.instant('UNIFIED_SETTINGS.SERVICE_RESTART_CONFIRM')
      );
      if (!ok) {
        this.postToIframe(source, 'mde-exec.cancelled', { blockId: req.blockId });
        return;
      }
      try {
        await firstValueFrom(this.http.post(`${this.serviceBaseUrl}/StopService`, { serviceId: existing }));
        this.runningServicesByBlock.delete(req.blockId);
      } catch (err: any) {
        this.postToIframe(source, 'mde-exec.error', {
          blockId: req.blockId,
          message: 'Failed to stop the existing service: ' + (err?.error?.error || err?.message || err),
        });
        return;
      }
    }

    try {
      await firstValueFrom(this.http.post(`${this.serviceBaseUrl}/RunService`, {
        blockId: req.blockId,
        language: req.lang,
        code: req.code,
        projectPath,
        documentPath: req.documentPath || '',
        parameters,
      }));
      // The service.started SignalR event flips the block into its running state.
    } catch (err: any) {
      const message = err?.error?.error || err?.message || 'Service start failed';
      this.postToIframe(source, 'mde-exec.error', { blockId: req.blockId, message });
      this.runningBlocks.delete(req.blockId);
    }
  }

  /**
   * Replies to an iframe's `mde-exec.queryServices` on (re)load: looks up the backend's live
   * service list and, for every running service whose owning document matches this iframe,
   * sends a `mde-exec.serviceStarted` so the matching block re-renders in its Stop state.
   *
   * Matching is keyed on (documentPath + blockId), NOT blockId alone: blockId is a hash of the
   * code + position, so an identical block in a different document shares it — without the
   * documentPath scope we would wrongly flip (and let you Stop) another document's service.
   */
  private async handleQueryServices(
    source: Window | null,
    req: { documentPath?: string },
  ): Promise<void> {
    if (!source) return;
    const docPath = this.normalizePath(req.documentPath || '');
    if (!docPath) return;

    let services: any[] = [];
    try {
      services = await firstValueFrom(this.http.get<any[]>(`${this.serviceBaseUrl}/Services`));
    } catch {
      return; // backend unreachable — nothing to re-link, leave blocks idle
    }

    (services || []).forEach(svc => {
      if (!svc || svc.status !== 'running' || !svc.blockId) return;
      if (this.normalizePath(svc.documentPath || '') !== docPath) return;
      this.runningServicesByBlock.set(svc.blockId, svc.id);
      this.postToIframe(source, 'mde-exec.serviceStarted', {
        blockId: svc.blockId,
        serviceId: svc.id,
        pid: svc.pid,
      });
    });
  }

  // Normalizes a filesystem path for cross-boundary comparison (unify separators, drop a
  // trailing slash, case-fold). Both sides originate from the same DocumentPath attribute,
  // so this is belt-and-suspenders against incidental separator/case differences.
  private normalizePath(p: string): string {
    return (p || '').replace(/\\/g, '/').replace(/\/+$/, '').toLowerCase();
  }

  private async fetchTrust(projectPath: string): Promise<boolean> {
    const cached = this.trustCache.get(projectPath);
    if (cached !== undefined) return cached;
    try {
      const params = new HttpParams().set('projectPath', projectPath);
      const resp = await firstValueFrom(
        this.http.get<{ trusted: boolean }>(`${this.baseUrl}/GetTrust`, { params })
      );
      const trusted = !!resp?.trusted;
      this.trustCache.set(projectPath, trusted);
      return trusted;
    } catch {
      return false;
    }
  }

  private dispatch(blockId: string, type: string, payload: any): void {
    const target = blockId ? this.runningBlocks.get(blockId) : null;
    if (target) {
      this.postToIframe(target, type, payload);
      return;
    }
    // Fallback: broadcast — useful right after reload when the map is empty.
    this.broadcastToIframes(type, payload);
  }

  private postToIframe(source: Window | null, type: string, payload: any): void {
    if (!source) {
      this.broadcastToIframes(type, payload);
      return;
    }
    try {
      source.postMessage({ type, ...payload }, '*');
    } catch (e) {
      console.warn('[ExecutionService] postMessage failed, falling back to broadcast:', e);
      this.broadcastToIframes(type, payload);
    }
  }

  private broadcastToIframes(type: string, payload: any): void {
    const iframes = document.querySelectorAll('iframe');
    iframes.forEach(iframe => {
      try {
        iframe.contentWindow?.postMessage({ type, ...payload }, '*');
      } catch { /* cross-origin or closed — ignore */ }
    });
  }
}
