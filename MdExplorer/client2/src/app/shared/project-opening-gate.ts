import { Injectable } from '@angular/core';
import { HttpContext, HttpContextToken } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { filter, first } from 'rxjs/operators';

/** Come sta l'apertura, per il riquadro «Costruzione della conoscenza». */
export interface ProjectOpeningState {
  opening: boolean;
  /** Le richieste trattenute, come `controller/azione`, nell'ordine di arrivo. */
  held: string[];
  startedAt: number | null;
  /** Durata dell'ultima apertura conclusa (ms); null finché non ne è finita una. */
  lastDurationMs: number | null;
}

/** Marca la richiesta che APRE il progetto: è l'unica che passa mentre il cancello è chiuso. */
export const OPENS_PROJECT = new HttpContextToken<boolean>(() => false);

/**
 * Il cancello dell'apertura di un progetto (06/11/2026).
 * <p>
 * La lista dei progetti va alla pagina del progetto AL CLICK, senza aspettare SetFolderProject: a freddo,
 * subito dopo l'avvio o un aggiornamento, il servizio impiegava fino a 7-9 s e la lista restava lì ferma.
 * Ma la pagina, appena nasce, chiede l'albero dei file e molto altro, e il servizio risponderebbe «connessione
 * non registrata». Allora, finché il progetto si sta aprendo, l'interceptor del ConnectionId TRATTIENE le
 * richieste /api e le lascia partire quando il progetto è pronto: la pagina si disegna subito, i dati arrivano
 * appena possono. Nessun componente deve saperlo.
 * </p>
 * Senza dipendenze, perché lo usa l'interceptor: ProjectsService (che usa HttpClient) creerebbe un ciclo.
 */
@Injectable({ providedIn: 'root' })
export class ProjectOpeningGate {
  private readonly _opening$ = new BehaviorSubject<boolean>(false);
  private readonly _state$ = new BehaviorSubject<ProjectOpeningState>({ opening: false, held: [], startedAt: null, lastDurationMs: null });

  /** true mentre SetFolderProject è in corso. */
  readonly opening$: Observable<boolean> = this._opening$.asObservable();

  /** Lo stato con le richieste trattenute: lo mostra il riquadro dei lavori in sfondo. */
  readonly state$: Observable<ProjectOpeningState> = this._state$.asObservable();

  get isOpening(): boolean { return this._opening$.value; }

  /** Il contesto da dare alla richiesta SetFolderProject. */
  static openingRequest(): { context: HttpContext } {
    return { context: new HttpContext().set(OPENS_PROJECT, true) };
  }

  close(): void {
    this._state$.next({ opening: true, held: [], startedAt: Date.now(), lastDurationMs: null });
    this._opening$.next(true);
  }

  /** Va chiamato SEMPRE alla fine di SetFolderProject, anche se fallisce: altrimenti ogni richiesta resta ferma. */
  open(): void {
    if (!this._opening$.value) { return; } // già aperto (tap e finalize lo chiamano entrambi)
    const s = this._state$.value;
    this._state$.next({ ...s, opening: false, lastDurationMs: s.startedAt ? Date.now() - s.startedAt : null });
    this._opening$.next(false);
  }

  /** L'interceptor la chiama per ogni richiesta che trattiene. */
  held(url: string): void {
    const s = this._state$.value;
    this._state$.next({ ...s, held: [...s.held, ProjectOpeningGate.shortName(url)] });
  }

  /** `../api/mdfiles/GetShallowStructure?x=1` → `mdfiles/GetShallowStructure`. */
  static shortName(url: string): string {
    const path = url.split('?')[0].replace(/^(\.\.)?\/api\//i, '');
    return path.split('/').slice(0, 2).join('/');
  }

  /** Emette una volta, appena il cancello è aperto (subito, se lo è già). */
  whenOpen(): Observable<boolean> {
    return this._opening$.pipe(filter(opening => !opening), first());
  }
}
