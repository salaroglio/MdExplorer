import { Injectable, NgZone } from '@angular/core';

/**
 * La finestra si accorge da sé di non rispondere, e lo dice al servizio (POST api/Diagnostics/UiStall), che lo scrive
 * nel registro accanto a ciò che i processi stavano facendo: è così che «non riesco a scrivere» arriva in «Invia
 * diagnostica» (23/11/2026, un PC lento dove la casella di MarkAgent non prendeva i tasti).
 *
 * Due misure, fuori da Angular per non pesare a loro volta:
 *  - un battito ogni mezzo secondo: se arriva in ritardo, il ciclo della finestra era bloccato per quel tempo;
 *  - da un tasto premuto al ridisegno successivo: quanto ha aspettato il carattere prima di comparire.
 *  - chi teneva occupata la finestra: Chromium dice, per ogni ridisegno lungo, quali script giravano, da quale file e
 *    funzione e chi li aveva chiamati (un timer, un evento, una risposta di rete). È ciò che distingue «la macchina è
 *    lenta» da «questo pezzo dell'applicazione gira troppo».
 * Si spedisce al più un rapporto ogni 30 secondi, e solo se c'è stato qualcosa.
 */
@Injectable({ providedIn: 'root' })
export class UiStallMonitorService {
  private static readonly BEAT_MS = 500;
  /** Un battito in ritardo di più di così è un blocco che si sente. */
  private static readonly STALL_MS = 300;
  /** Un tasto che compare dopo più di così si sente «in ritardo». */
  private static readonly SLOW_KEY_MS = 150;
  private static readonly REPORT_EVERY_MS = 30000;

  private started = false;
  private count = 0;
  private maxMs = 0;
  private totalMs = 0;
  private slowKeys = 0;
  private maxKeyMs = 0;
  /** Chi ha tenuto occupata la finestra: «chi l'ha chiamato | file | funzione | finestra» → tempo e volte. */
  private culprits = new Map<string, { totalMs: number; maxMs: number; count: number }>();

  constructor(private zone: NgZone) {}

  start(): void {
    if (this.started) return;
    this.started = true;
    this.zone.runOutsideAngular(() => {
      let last = performance.now();
      setInterval(() => {
        const now = performance.now();
        const late = now - last - UiStallMonitorService.BEAT_MS;
        last = now;
        // Una finestra nascosta o ridotta a icona ha i timer rallentati dal browser: non è un blocco.
        if (document.hidden) return;
        if (late >= UiStallMonitorService.STALL_MS) {
          this.count++;
          this.totalMs += late;
          this.maxMs = Math.max(this.maxMs, late);
        }
      }, UiStallMonitorService.BEAT_MS);

      document.addEventListener('keydown', () => {
        const pressed = performance.now();
        requestAnimationFrame(() => {
          const waited = performance.now() - pressed;
          if (waited >= UiStallMonitorService.SLOW_KEY_MS) {
            this.slowKeys++;
            this.maxKeyMs = Math.max(this.maxKeyMs, waited);
          }
        });
      }, { capture: true, passive: true });

      this.watchLongFrames();
      setInterval(() => this.report(), UiStallMonitorService.REPORT_EVERY_MS);
    });
  }

  /**
   * I ridisegni lunghi, con gli script che li hanno occupati (`long-animation-frame`, Chromium 123+). Dove non c'è,
   * restano le altre due misure. Del file si tiene solo il nome: niente indirizzi, niente parametri.
   */
  private watchLongFrames(): void {
    try {
      const observer = new PerformanceObserver(list => {
        for (const frame of list.getEntries() as any[]) {
          if (frame.duration < UiStallMonitorService.STALL_MS) continue;
          let inScripts = 0;
          for (const script of frame.scripts || []) {
            inScripts += script.duration;
            if (script.duration < 100) continue;
            const file = String(script.sourceURL || '').split('?')[0].split('#')[0].split('/').pop() || '(pagina)';
            this.blame(`${script.invoker || script.invokerType || '?'} | ${file} | ${script.sourceFunctionName || '(anonima)'} | ${script.windowAttribution || '?'}`, script.duration);
          }
          // Il tempo che non è di nessuno script: calcolo degli stili, impaginazione, disegno.
          const rest = frame.duration - inScripts;
          if (rest >= 100) this.blame('(nessuno script: stili, impaginazione, disegno)', rest);
        }
      });
      observer.observe({ type: 'long-animation-frame', buffered: true } as any);
    } catch { /* questo Chromium non lo ha: si va avanti con il battito e i tasti */ }
  }

  private blame(who: string, ms: number): void {
    const known = this.culprits.get(who) || { totalMs: 0, maxMs: 0, count: 0 };
    known.totalMs += ms; known.maxMs = Math.max(known.maxMs, ms); known.count++;
    this.culprits.set(who, known);
  }

  private report(): void {
    if (this.count === 0 && this.slowKeys === 0 && this.culprits.size === 0) return;
    const culprits = [...this.culprits.entries()]
      .sort((a, b) => b[1].totalMs - a[1].totalMs).slice(0, 8)
      .map(([who, v]) => `${Math.round(v.totalMs)} ms in ${v.count} volte (max ${Math.round(v.maxMs)}): ${who}`);
    this.culprits.clear();
    const body = JSON.stringify({
      count: this.count,
      maxMs: Math.round(this.maxMs),
      totalMs: Math.round(this.totalMs),
      slowKeys: this.slowKeys,
      maxKeyMs: Math.round(this.maxKeyMs),
      windowSeconds: UiStallMonitorService.REPORT_EVERY_MS / 1000,
      route: location.pathname,
      culprits,
    });
    this.count = 0; this.maxMs = 0; this.totalMs = 0; this.slowKeys = 0; this.maxKeyMs = 0;
    // Senza HttpClient: niente intercettori, niente giro di Angular. Se il servizio non risponde, il rapporto si perde.
    fetch('/api/Diagnostics/UiStall', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body, keepalive: true })
      .catch(() => { /* la misura non deve mai disturbare l'applicazione */ });
  }
}
