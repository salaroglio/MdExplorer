import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface EmbeddedAppEntry {
  appId: string;
  url: string;
  name: string;
}

@Injectable({
  providedIn: 'root'
})
export class EmbeddedAppStateService {

  private apps = new Map<string, EmbeddedAppEntry>();

  /** Which app should be visible right now (null = show router-outlet) */
  activeAppId$ = new BehaviorSubject<string | null>(null);

  /** List of all registered apps (for *ngFor in template) */
  apps$ = new BehaviorSubject<EmbeddedAppEntry[]>([]);

  registerApp(appId: string, url: string, name: string): void {
    if (!this.apps.has(appId)) {
      this.apps.set(appId, { appId, url, name });
      this.apps$.next(Array.from(this.apps.values()));
    }
  }

  activateApp(appId: string): void {
    if (this.apps.has(appId)) {
      this.activeAppId$.next(appId);
    }
  }

  deactivate(): void {
    this.activeAppId$.next(null);
  }

  isRegistered(appId: string): boolean {
    return this.apps.has(appId);
  }

  getAppUrl(appId: string): string | null {
    return this.apps.get(appId)?.url ?? null;
  }

  unregisterApp(appId: string): void {
    this.apps.delete(appId);
    this.apps$.next(Array.from(this.apps.values()));
    if (this.activeAppId$.value === appId) {
      this.activeAppId$.next(null);
    }
  }
}
