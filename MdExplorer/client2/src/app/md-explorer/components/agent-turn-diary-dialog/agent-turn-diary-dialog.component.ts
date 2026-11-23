import { AfterViewChecked, Component, ElementRef, Inject, OnDestroy, OnInit, SecurityContext, ViewChild } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { marked } from 'marked';
import { MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA, MatLegacyDialogRef as MatDialogRef } from '@angular/material/legacy-dialog';
import { Subscription } from 'rxjs';
import { AgentTurnDiary, AgentTurnDiaryEntry, AgentTurnDiaryService } from '../../services/agent-turn-diary.service';
import { AgentReviewService, MergeRequest } from '../../services/agent-review.service';
import { MdServerMessagesService } from '../../../signalR/services/server-messages.service';
import { AgentMailboxNotificationService } from '../../../services/agent-mailbox-notification.service';

export interface AgentTurnDiaryDialogData {
  /** L'identificativo del turno: quello dell'agente «al lavoro» nella toolbar, o di un turno finito. */
  turnId: string;
  agentName: string;
  engine: string;
}

/**
 * Il diario di un turno: che cosa scrive, pensa e usa un agente mentre lavora, e che cosa ha fatto a turno finito.
 * Un pannello a sé, di sola lettura, che non blocca il resto dell'applicazione: si apre dall'indicatore «agenti al
 * lavoro» della toolbar. Segue l'uscita finché si sta in fondo; se si torna su a leggere, non trascina via.
 */
@Component({
  selector: 'app-agent-turn-diary-dialog',
  templateUrl: './agent-turn-diary-dialog.component.html',
  styleUrls: ['./agent-turn-diary-dialog.component.scss'],
})
export class AgentTurnDiaryDialogComponent implements OnInit, OnDestroy, AfterViewChecked {
  diary: AgentTurnDiary | null = null;
  entries: AgentTurnDiaryEntry[] = [];
  /**
   * La risposta resa come markdown, per voce: si ricalcola solo quando la voce cambia. Una stringa tenuta, non un
   * metodo nel template: con `[innerHTML]="metodo()"` il testo verrebbe ridisegnato a ogni giro e non si potrebbe selezionare.
   */
  html: { [seq: number]: string } = {};
  /** «Ferma»: il primo clic chiede conferma (per qualche secondo), il secondo ferma. */
  stopArmed = false;
  stopping = false;
  stopError: string | null = null;
  private stopArmedTimer: any = null;
  /** Perché il diario non si legge (turno troppo vecchio, servizio che non risponde): lo si dice, niente pannello vuoto. */
  loadError: string | null = null;
  /**
   * Il lavoro che questo turno ha consegnato da un posto isolato, finché aspetta una decisione: da qui lo si va a
   * rivedere e a unire. Arriva poco DOPO la fine del turno (salvataggio e pubblicazione del ramo), con lo stesso
   * identificativo di lancio del diario.
   */
  delivered: MergeRequest | null = null;
  /** L'ora con cui si calcola «da quanto»: avanza solo finché il turno è in corso. */
  now: number = Date.now();

  @ViewChild('scroller') private scroller?: ElementRef<HTMLElement>;

  private version = 0;
  private reading = false;
  private again = false;
  private follow = true;
  private scrollPending = false;
  private clock: any = null;
  private subscription = new Subscription();

  constructor(
    public dialogRef: MatDialogRef<AgentTurnDiaryDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: AgentTurnDiaryDialogData,
    private diaries: AgentTurnDiaryService,
    private sanitizer: DomSanitizer,
    private review: AgentReviewService,
    private serverMessages: MdServerMessagesService,
    private mailbox: AgentMailboxNotificationService,
  ) {}

  ngOnInit(): void {
    this.subscription.add(this.diaries.changed$.subscribe(change => {
      if (change.id === this.data.turnId && change.version > this.version) this.refresh();
    }));
    this.subscription.add(this.serverMessages.agentMergeRequested$.subscribe(() => this.lookForDelivery()));
    this.clock = setInterval(() => { if (this.running) this.now = Date.now(); }, 1000);
    this.refresh();
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
    if (this.clock) clearInterval(this.clock);
    if (this.stopArmedTimer) clearTimeout(this.stopArmedTimer);
  }

  ngAfterViewChecked(): void {
    if (!this.scrollPending || !this.scroller) return;
    this.scrollPending = false;
    const el = this.scroller.nativeElement;
    el.scrollTop = el.scrollHeight;
  }

  get running(): boolean {
    return !!this.diary && !this.diary.finishedAtUtc;
  }

  /** Fermato a mano (da qui o da chi l'ha lanciato): non è un turno «non riuscito». */
  get stopped(): boolean {
    return !!this.diary && !!this.diary.finishedAtUtc && this.diary.outcome === 'Cancelled';
  }

  get failed(): boolean {
    return !!this.diary && !!this.diary.finishedAtUtc && this.diary.outcome !== 'Completed' && !this.stopped;
  }

  /** «Ferma»: primo clic arma (e si disarma da solo dopo 4 secondi), secondo clic ferma. Un turno perso non si recupera. */
  stop(): void {
    if (!this.running || this.stopping) return;
    if (!this.stopArmed) {
      this.stopArmed = true;
      this.stopArmedTimer = setTimeout(() => this.stopArmed = false, 4000);
      return;
    }
    if (this.stopArmedTimer) clearTimeout(this.stopArmedTimer);
    this.stopArmed = false;
    this.stopping = true;
    this.stopError = null;
    this.diaries.stop(this.data.turnId).subscribe({
      // Fermato: lo dirà il diario, quando il motore si è fermato davvero. Fino ad allora resta «si sta fermando».
      next: () => {},
      error: err => {
        this.stopping = false;
        // 404 = è finito da solo nel frattempo: il diario lo sta già dicendo, non è un errore da mostrare.
        if (err?.status !== 404) this.stopError = typeof err?.error === 'string' ? err.error : (err?.message || String(err));
        this.refresh();
      },
    });
  }

  /** «1 min 05 s»: da quanto lavora, o quanto ha lavorato. */
  get elapsed(): string {
    if (!this.diary) return '';
    const end = this.diary.finishedAtUtc ? new Date(this.diary.finishedAtUtc).getTime() : this.now;
    const seconds = Math.max(0, Math.round((end - new Date(this.diary.startedAtUtc).getTime()) / 1000));
    const minutes = Math.floor(seconds / 60);
    const rest = seconds % 60;
    return minutes > 0 ? `${minutes} min ${rest < 10 ? '0' : ''}${rest} s` : `${rest} s`;
  }

  /** L'esito di uno strumento («↳ ok», «↳ errore») sta sotto lo strumento, rientrato. */
  isToolResult(entry: AgentTurnDiaryEntry): boolean {
    return entry.kind === 'tool' && entry.text.startsWith('↳');
  }

  isToolFailure(entry: AgentTurnDiaryEntry): boolean {
    return this.isToolResult(entry) && !/ok\s*$/i.test(entry.text);
  }

  trackBySeq(_: number, entry: AgentTurnDiaryEntry): number {
    return entry.seq;
  }

  /** Chi torna su a leggere non viene trascinato in fondo; tornando in fondo si riprende a seguire. */
  onScroll(): void {
    const el = this.scroller?.nativeElement;
    if (!el) return;
    this.follow = el.scrollHeight - el.scrollTop - el.clientHeight < 24;
  }

  close(): void {
    this.dialogRef.close();
  }

  /** «Rivedi e unisci»: la finestra dei messaggi, aperta sulla consegna di questo turno. */
  openDelivery(): void {
    if (!this.delivered) return;
    const id = this.delivered.id;
    this.dialogRef.close();
    this.mailbox.open(0, id);
  }

  /** Cerca, fra le consegne in attesa del progetto, quella nata da questo lancio. */
  private lookForDelivery(): void {
    const runId = (this.diary?.runId || '').toLowerCase();
    if (!runId || !this.diary?.finishedAtUtc) return;
    this.review.pending(this.diary.projectPath).subscribe({
      next: res => {
        this.delivered = (res.requests || []).find(r => r.status === 'pending' && (r.runId || '').toLowerCase() === runId) || null;
      },
      // Senza l'elenco delle consegne il diario resta leggibile: la consegna si trova comunque dal pulsante dei messaggi.
      error: () => {},
    });
  }

  /** Rilegge dalla versione che ha già. Un avviso arrivato durante la lettura fa rileggere una volta ancora. */
  private refresh(): void {
    if (this.reading) { this.again = true; return; }
    this.reading = true;
    this.diaries.read(this.data.turnId, this.version).subscribe({
      next: diary => {
        this.reading = false;
        this.loadError = null;
        this.merge(diary);
        if (this.again) { this.again = false; this.refresh(); }
      },
      error: err => {
        this.reading = false;
        this.again = false;
        this.loadError = typeof err?.error === 'string' ? err.error : (err?.message || String(err));
      },
    });
  }

  /** La risposta è markdown (titoli, elenchi, codice): la si rende come nella chat di MarkAgent, e la si ripulisce. */
  private render(text: string): string {
    const html = marked.parse(text || '', { async: false, gfm: true, breaks: true }) as string;
    return this.sanitizer.sanitize(SecurityContext.HTML, html) ?? '';
  }

  private merge(diary: AgentTurnDiary): void {
    for (const changed of diary.entries || []) {
      const at = this.entries.findIndex(e => e.seq === changed.seq);
      if (at >= 0) this.entries[at] = changed; else this.entries.push(changed);
      if (changed.kind === 'message') this.html[changed.seq] = this.render(changed.text);
    }
    if (diary.finishedAtUtc) this.stopping = false;
    const justKnownFinished = !!diary.finishedAtUtc && !this.diary?.finishedAtUtc;
    this.entries.sort((a, b) => a.seq - b.seq);
    this.version = Math.max(this.version, diary.version);
    this.diary = { ...diary, entries: [] };
    if (justKnownFinished) this.lookForDelivery();
    this.now = Date.now();
    if (this.follow) this.scrollPending = true;
  }
}
