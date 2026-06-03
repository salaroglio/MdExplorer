import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

/**
 * Stato unificato della barra "Building knowledge" mostrata in basso a destra
 * durante l'indicizzazione asincrona del progetto.
 *
 * Eventi SignalR che alimentano questo stato:
 *  - parsingProjectStart  → reset() (percent=0)
 *  - knowledgeProgress    → setProgress() (incremento da backend per ogni folder)
 *  - parsingProjectStop   → setComplete() (percent=100), poi auto-dismiss
 *
 * Il flusso esce dal vecchio pattern di MatSnackBar.open() ripetuto per ogni
 * folder (che faceva "scoppiettare" UI). Adesso UNA sola snackbar custom
 * (IndexingProgressSnackComponent) resta aperta per tutta l'indicizzazione e
 * si abbevera da questo BehaviorSubject.
 */
export interface IndexingProgressState {
  percent: number;       // 0–100
  processed: number;     // folder o file processati
  total: number;         // folder o file totali
  done: boolean;         // true quando parsingProjectStop è arrivato
}

@Injectable({ providedIn: 'root' })
export class IndexingProgressService {

  private readonly _state$ = new BehaviorSubject<IndexingProgressState>({
    percent: 0,
    processed: 0,
    total: 0,
    done: false
  });

  public readonly state$: Observable<IndexingProgressState> = this._state$.asObservable();

  public reset(): void {
    this._state$.next({ percent: 0, processed: 0, total: 0, done: false });
  }

  public setProgress(processed: number, total: number, percent: number): void {
    const safePercent = Math.max(0, Math.min(100, percent));
    this._state$.next({ percent: safePercent, processed, total, done: false });
  }

  public setComplete(): void {
    const prev = this._state$.value;
    this._state$.next({ percent: 100, processed: prev.processed, total: prev.total, done: true });
  }
}
