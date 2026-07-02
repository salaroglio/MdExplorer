import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';

/**
 * Thin channel that lets the toolbar (app-bar) ask the document viewer
 * (MainContentComponent) to reload the currently open document iframe.
 *
 * The reload logic lives in MainContentComponent.refreshCurrentFile(); this
 * service only carries the "please refresh now" signal so the two sibling
 * components don't need a direct reference to each other.
 */
@Injectable({ providedIn: 'root' })
export class DocumentRefreshService {
  private readonly _refresh = new Subject<void>();

  /** Emits whenever a manual document refresh is requested. */
  readonly refresh$: Observable<void> = this._refresh.asObservable();

  /** Ask the open document to reload (called by the toolbar refresh button). */
  requestRefresh(): void {
    this._refresh.next();
  }
}
