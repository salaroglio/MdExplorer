import {
  Component,
  OnInit,
  OnDestroy,
  ChangeDetectorRef,
  NgZone
} from '@angular/core';
import { Subscription } from 'rxjs';
import { Router } from '@angular/router';
import { MdFileService } from '../../services/md-file.service';
import { MdFile } from '../../models/md-file';
import { EmbeddedAppStateService } from '../../services/embedded-app-state.service';

@Component({
  selector: 'app-external-app',
  templateUrl: './external-app.component.html',
  styleUrls: ['./external-app.component.scss']
})
export class ExternalAppComponent implements OnInit, OnDestroy {

  state: 'idle' | 'loading' | 'ready' | 'error' | 'notInstalled' = 'idle';
  errorMessage = '';
  currentAppId: string | null = null;
  currentAppName: string | null = null;

  private subscription: Subscription;
  private isElectron = !!(window as any).electronAPI?.externalApp;

  constructor(
    private mdFileService: MdFileService,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone,
    private router: Router,
    private embeddedAppState: EmbeddedAppStateService
  ) {}

  ngOnInit(): void {
    this.subscription = this.mdFileService.selectedMdFileFromSideNav.subscribe(
      (node: MdFile) => {
        if (node?.type === 'externalApp' && node.appId) {
          this.openApp(node);
        } else if (node?.type === 'externalAppNotInstalled' && node.appId) {
          this.showNotInstalled(node);
        }
      }
    );
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
    // Do NOT terminate the process — singleton behaviour: process stays alive
    // so re-opening the app is instant. Processes are cleaned up on app quit.
  }

  private async openApp(node: MdFile): Promise<void> {
    const appId = node.appId;

    // If app is already registered in the persistent service, just activate it
    if (this.embeddedAppState.isRegistered(appId)) {
      this.currentAppId = appId;
      this.state = 'ready';
      this.embeddedAppState.activateApp(appId);
      this.cdr.markForCheck();
      return;
    }

    this.currentAppId = appId;
    this.state = 'loading';
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
          const url = `http://localhost:${result.port}/`;
          this.embeddedAppState.registerApp(appId, url, node.name);
          this.embeddedAppState.activateApp(appId);
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
      // Unregister so it will be re-launched fresh
      this.embeddedAppState.unregisterApp(node.appId);
      this.currentAppId = null;
      this.openApp(node);
    }
  }

  private showNotInstalled(node: MdFile): void {
    this.currentAppId = node.appId;
    this.currentAppName = node.name;
    this.state = 'notInstalled';
    this.errorMessage = '';
    this.cdr.markForCheck();
  }

  goToAppStore(): void {
    this.router.navigate(['/main/app-store']);
  }

}
