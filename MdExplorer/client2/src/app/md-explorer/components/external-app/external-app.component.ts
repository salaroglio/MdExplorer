import {
  Component,
  OnInit,
  OnDestroy,
  ChangeDetectorRef,
  NgZone
} from '@angular/core';
import { Subscription } from 'rxjs';
import { MdFileService } from '../../services/md-file.service';
import { MdFile } from '../../models/md-file';

@Component({
  selector: 'app-external-app',
  templateUrl: './external-app.component.html',
  styleUrls: ['./external-app.component.scss']
})
export class ExternalAppComponent implements OnInit, OnDestroy {

  state: 'idle' | 'loading' | 'ready' | 'error' = 'idle';
  errorMessage = '';
  currentAppId: string | null = null;
  appUrl: string | null = null;

  private subscription: Subscription;
  private unsubscribeCrashed: (() => void) | null = null;
  private isElectron = !!(window as any).electronAPI?.externalApp;

  constructor(
    private mdFileService: MdFileService,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {}

  ngOnInit(): void {
    this.subscription = this.mdFileService.selectedMdFileFromSideNav.subscribe(
      (node: MdFile) => {
        if (node?.type === 'externalApp' && node.appId) {
          this.openApp(node);
        }
      }
    );

    if (this.isElectron) {
      this.unsubscribeCrashed = (window as any).electronAPI.externalApp.onCrashed(
        (data: { appId: string; exitCode: number }) => {
          this.ngZone.run(() => {
            if (data.appId === this.currentAppId) {
              this.state = 'error';
              this.appUrl = null;
              this.errorMessage = `App process exited unexpectedly (exit code: ${data.exitCode})`;
              this.cdr.markForCheck();
            }
          });
        }
      );
    }
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
    if (this.unsubscribeCrashed) this.unsubscribeCrashed();
    // Do NOT terminate the process — singleton behaviour: process stays alive
    // so re-opening the app is instant. Processes are cleaned up on app quit.
  }

  private async openApp(node: MdFile): Promise<void> {
    // If same app is already showing, nothing to do
    if (node.appId === this.currentAppId && this.state === 'ready') return;

    this.currentAppId = node.appId;
    this.state = 'loading';
    this.appUrl = null;
    this.errorMessage = '';
    this.cdr.markForCheck();

    if (!this.isElectron) {
      this.state = 'error';
      this.errorMessage = 'External apps are only available in the desktop (Electron) version.';
      this.cdr.markForCheck();
      return;
    }

    try {
      // Main process spawns the app and polls readiness (no CORS issues)
      // — only returns when the HTTP server is up, or returns an error.
      const result = await (window as any).electronAPI.externalApp.launch(
        node.appId,
        node.appExecutable,
        node.appArgs ?? []
      );

      if (!result.success) {
        this.ngZone.run(() => {
          this.state = 'error';
          this.errorMessage = result.error ?? 'Failed to launch external app.';
          this.cdr.markForCheck();
        });
        return;
      }

      this.ngZone.run(() => {
        if (this.currentAppId === node.appId) {
          this.appUrl = `http://localhost:${result.port}/`;
          this.state = 'ready';
          this.cdr.markForCheck();
        }
      });
    } catch (err: any) {
      this.ngZone.run(() => {
        this.state = 'error';
        this.errorMessage = err?.message ?? 'Unknown error launching external app.';
        this.cdr.markForCheck();
      });
    }
  }

  retry(): void {
    const node = this.mdFileService.currentSelectedMdFile;
    if (node?.type === 'externalApp' && node.appId === this.currentAppId) {
      this.currentAppId = null; // force re-open
      this.openApp(node);
    }
  }

}
