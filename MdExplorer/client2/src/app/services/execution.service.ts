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

interface RunRequestPayload {
  type: 'mde-exec.requestRun';
  blockId: string;
  lang: string;
  code: string;
  params: Array<{ name: string; defaultValue: string; isSecret: boolean; description?: string; kind?: string }>;
  paramsInline?: boolean;
  projectPath: string;
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
  private readonly trustCache = new Map<string, boolean>();

  // While a block is running we remember which iframe originated it so that
  // streaming chunks go back to the right Window (also fine to broadcast,
  // but targeted postMessage is cheaper and less noisy in devtools).
  private readonly runningBlocks = new Map<string, Window>();

  constructor(
    private http: HttpClient,
    private dialog: MatDialog,
    private serverMessages: MdServerMessagesService,
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
