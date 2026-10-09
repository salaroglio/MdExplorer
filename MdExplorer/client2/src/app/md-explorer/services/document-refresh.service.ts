import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Subject } from 'rxjs';

/**
 * Thin channel that lets the toolbar (app-bar) ask the document viewer
 * (MainContentComponent) to reload the currently open document iframe.
 *
 * The reload logic lives in MainContentComponent.refreshCurrentFile(); this
 * service only carries the "please refresh now" signal so the two sibling
 * components don't need a direct reference to each other.
 *
 * The other way round, the viewer says whether the page it shows is a slide
 * deck, so the toolbar can offer the PDF export of the slides.
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

  private readonly _slideDeckShown = new BehaviorSubject<boolean>(false);

  /** Whether the viewer shows a slide deck (a reveal.js page). */
  readonly slideDeckShown$: Observable<boolean> = this._slideDeckShown.asObservable();

  /** Called by the viewer each time its page has loaded. */
  setSlideDeckShown(shown: boolean): void {
    if (this._slideDeckShown.value !== shown) {
      this._slideDeckShown.next(shown);
    }
  }
}
