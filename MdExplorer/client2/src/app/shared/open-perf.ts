import { isDevMode } from '@angular/core';

/**
 * Sonda dei tempi dell'apertura di un progetto (03/11/2026): dal click alla schermata del documento.
 * Solo in sviluppo (isDevMode) o con localStorage 'mde-perf' = '1'; righe '[OPEN-PERF]' in console,
 * da confrontare con le righe '[PERF]' del servizio (stesso orologio: stessa macchina).
 */
export class OpenPerf {
  private static t0 = 0;

  static get enabled(): boolean {
    try {
      return isDevMode() || localStorage.getItem('mde-perf') === '1';
    } catch {
      return isDevMode();
    }
  }

  /** Il click: azzera il cronometro. */
  static start(what: string): void {
    if (!OpenPerf.enabled) { return; }
    OpenPerf.t0 = performance.now();
    // il buffer di default (250) in una sessione lunga è già pieno: le richieste dell'apertura non ci finirebbero
    try { performance.setResourceTimingBufferSize(5000); } catch { /* browser senza */ }
    console.log(`[OPEN-PERF] ${OpenPerf.clock()} +0 ms  ${what}`);
  }

  static mark(what: string): void {
    if (!OpenPerf.enabled || OpenPerf.t0 === 0) { return; }
    console.log(`[OPEN-PERF] ${OpenPerf.clock()} +${Math.round(performance.now() - OpenPerf.t0)} ms  ${what}`);
  }

  /**
   * Dove è andato il tempo di una richiesta, dalla Resource Timing del browser:
   * coda nel browser (fetchStart→requestStart: connessioni occupate), server (requestStart→responseStart),
   * download. Senza connessioni libere il browser trattiene la richiesta e il servizio non la vede ancora.
   */
  static request(urlPart: string): void {
    if (!OpenPerf.enabled) { return; }
    const entries = performance.getEntriesByType('resource')
      .filter(e => e.name.includes(urlPart)) as PerformanceResourceTiming[];
    const e = entries[entries.length - 1];
    if (!e) {
      OpenPerf.mark(`${urlPart}: nessuna Resource Timing`);
      return;
    }
    const queued = e.requestStart > 0 ? Math.round(e.requestStart - e.fetchStart) : -1;
    const server = e.requestStart > 0 ? Math.round(e.responseStart - e.requestStart) : -1;
    const download = Math.round(e.responseEnd - e.responseStart);
    OpenPerf.mark(`${urlPart}: in coda nel browser ${queued} ms, server ${server} ms, download ${download} ms, totale ${Math.round(e.duration)} ms`);
    const pending = performance.getEntriesByType('resource')
      .filter(r => (r as PerformanceResourceTiming).responseEnd > e.fetchStart && r.startTime < e.fetchStart && r.name.includes('/api/'))
      .map(r => r.name.replace(/^.*\/api\//, '').replace(/\?.*$/, ''));
    if (pending.length) {
      OpenPerf.mark(`  richieste API ancora aperte quando è partita: ${pending.length} → ${pending.join(', ')}`);
    }
  }

  private static clock(): string {
    const d = new Date();
    return d.toTimeString().substring(0, 8) + '.' + String(d.getMilliseconds()).padStart(3, '0');
  }
}
