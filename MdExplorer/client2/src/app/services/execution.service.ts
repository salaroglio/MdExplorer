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

interface RunRequestPayload {
  type: 'mde-exec.requestRun';
  blockId: string;
  lang: string;
  code: string;
  params: Array<{ name: string; defaultValue: string; isSecret: boolean; description?: string; kind?: string }>;
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
      if (!data || data.type !== 'mde-exec.requestRun') return;
      const src = (event.source as Window) || null;
      // Fire-and-forget — do not await so that we never block the message loop.
      this.handleRunRequest(src, data).catch(err => {
        console.error('[ExecutionService] Unhandled error:', err);
        this.postToIframe(src, 'mde-exec.error', {
          blockId: data.blockId,
          message: err?.message || 'Unexpected error',
        });
      });
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

    const dialogData: RunCommandDialogData = {
      language: req.lang,
      code: req.code,
      params: req.params || [],
      needsTrust: !trusted,
      projectPath,
    };

    const needsDialog = !trusted || (req.params && req.params.length > 0);
    let result: RunCommandDialogResult | undefined;
    if (needsDialog) {
      const ref = this.dialog.open<RunCommandDialogComponent, RunCommandDialogData, RunCommandDialogResult>(
        RunCommandDialogComponent,
        { data: dialogData, width: '560px', autoFocus: true, restoreFocus: true },
      );
      result = await firstValueFrom(ref.afterClosed());
    } else {
      // Trusted project + no parameters: run directly like Jupyter Shift-Enter.
      result = { confirmed: true, trustProject: true, parameters: {} };
    }

    if (!result || !result.confirmed) {
      this.postToIframe(source, 'mde-exec.cancelled', { blockId: req.blockId });
      return;
    }

    if (!trusted) {
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
      } catch (err: any) {
        this.postToIframe(source, 'mde-exec.error', {
          blockId: req.blockId,
          message: 'Failed to persist trust: ' + (err?.message || err),
        });
        return;
      }
    }

    // Remember the iframe so streamed chunks can be routed back.
    if (source) this.runningBlocks.set(req.blockId, source);

    try {
      await firstValueFrom(this.http.post(`${this.baseUrl}/Run`, {
        blockId: req.blockId,
        language: req.lang,
        code: req.code,
        projectPath,
        parameters: result.parameters || {},
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
