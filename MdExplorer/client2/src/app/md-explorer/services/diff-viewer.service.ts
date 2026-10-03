import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

/** Quale differenza si sta guardando, e di quale repository. */
export interface DiffRequest {
  projectPath: string;
  /** Vuoto = la radice del contesto; altrimenti il percorso del submodule. */
  repo: string;
  /** Percorso del file, relativo al repository indicato. */
  path: string;
  /**
   * Percorso precedente, per i file rinominati. Va passato a git insieme al nuovo:
   * con un percorso solo la rinomina non viene riconosciuta e il file sembra nuovo.
   */
  oldPath?: string;
  /** Etichetta del repository, per l'intestazione. */
  repoLabel: string;
  agent: string | null;
}

/**
 * Il tramite fra l'elenco delle modifiche — che sta nella barra laterale — e il riquadro grande,
 * dove la differenza si legge davvero.
 *
 * Il diff sotto la riga costringeva a leggere codice in una colonna stretta, spingendo giù il
 * resto dell'elenco: si perdeva il posto proprio mentre si confrontava. Il riquadro grande è
 * dove si guarda un documento, ed è dove va guardata anche la sua differenza.
 */
@Injectable({ providedIn: 'root' })
export class DiffViewerService {
  /** `null` = nessuna differenza aperta, il riquadro torna al documento. */
  readonly opened$ = new BehaviorSubject<DiffRequest | null>(null);

  show(request: DiffRequest): void {
    this.opened$.next(request);
  }

  /** Si chiude anche da sola quando si apre un documento: due cose non stanno nello stesso posto. */
  close(): void {
    if (this.opened$.value !== null) this.opened$.next(null);
  }
}
