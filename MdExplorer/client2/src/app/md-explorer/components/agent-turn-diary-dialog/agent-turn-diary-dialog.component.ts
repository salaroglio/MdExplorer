import { AfterViewChecked, Component, ElementRef, Inject, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA, MatLegacyDialogRef as MatDialogRef } from '@angular/material/legacy-dialog';
import { Subscription } from 'rxjs';
import { AgentTurnDiary, AgentTurnDiaryEntry, AgentTurnDiaryService } from '../../services/agent-turn-diary.service';

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
  /** Perché il diario non si legge (turno troppo vecchio, servizio che non risponde): lo si dice, niente pannello vuoto. */
  loadError: string | null = null;
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
  ) {}

  ngOnInit(): void {
    this.subscription.add(this.diaries.changed$.subscribe(change => {
      if (change.id === this.data.turnId && change.version > this.version) this.refresh();
    }));
    this.clock = setInterval(() => { if (this.running) this.now = Date.now(); }, 1000);
    this.refresh();
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
    if (this.clock) clearInterval(this.clock);
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

  get failed(): boolean {
    return !!this.diary && !!this.diary.finishedAtUtc && this.diary.outcome !== 'Completed';
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

  private merge(diary: AgentTurnDiary): void {
    for (const changed of diary.entries || []) {
      const at = this.entries.findIndex(e => e.seq === changed.seq);
      if (at >= 0) this.entries[at] = changed; else this.entries.push(changed);
    }
    this.entries.sort((a, b) => a.seq - b.seq);
    this.version = Math.max(this.version, diary.version);
    this.diary = { ...diary, entries: [] };
    this.now = Date.now();
    if (this.follow) this.scrollPending = true;
  }
}
