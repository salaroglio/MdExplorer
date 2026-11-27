import { Component, OnDestroy, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ChangeHunk, RootChangeDetail, RootChangeSummary, RootReviewService } from '../../services/root-review.service';

/** Una riga del pezzo come si mostra: contesto, tolta o aggiunta, col numero di riga. */
interface HunkRow { kind: 'ctx' | 'del' | 'add'; line: number | null; text: string; }

/**
 * La revisione dei file delle radici cambiati da MarkAgent (sprint 2026-11-27-Modifiche-Da-Approvare-Sulle-Radici, F4, D7-D9),
 * nel riquadro grande accanto alla chat: a sinistra i file, a destra i pezzi di quello scelto con Tieni e Annulla. Il file
 * è già cambiato (modello «arretra», come VS Code): Annulla lo riporta indietro, Tieni toglie il pezzo dalla revisione.
 */
@Component({
  selector: 'app-root-review-panel',
  templateUrl: './root-review-panel.component.html',
  styleUrls: ['./root-review-panel.component.scss'],
})
export class RootReviewPanelComponent implements OnInit, OnDestroy {
  files: RootChangeSummary[] = [];
  selected: RootChangeSummary | null = null;
  detail: RootChangeDetail | null = null;
  rows: HunkRow[][] = [];
  busy = false;
  error: string | null = null;
  problems: string[] = [];
  private destroy$ = new Subject<void>();
  private static readonly Context = 2;

  constructor(public review: RootReviewService, private translate: TranslateService) {}

  ngOnInit(): void {
    this.review.pending$.pipe(takeUntil(this.destroy$)).subscribe(list => {
      this.files = list;
      const focus = this.review.focus$.value;
      const keep = this.selected && list.find(f => f.logicalPath === this.selected.logicalPath);
      const next = (focus && list.find(f => f.logicalPath === focus)) || keep || list[0] || null;
      if (focus) this.review.focus$.next(null);
      // anche lo stesso file si rilegge: dopo Tieni o Annulla i pezzi sono cambiati
      this.select(next);
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  trackFile = (_: number, f: RootChangeSummary) => f.logicalPath;

  async select(file: RootChangeSummary | null): Promise<void> {
    this.selected = file;
    this.detail = null;
    this.rows = [];
    this.error = null;
    if (!file) return;
    try {
      const detail = await this.review.detail(file.alias, file.relative);
      if (this.selected !== file) return;
      this.detail = detail;
      this.rows = detail.hunks.map(h => this.rowsOf(h, detail));
    } catch (e: any) {
      this.error = e?.error?.error || e?.message || String(e);
    }
  }

  /** Il pezzo con due righe di contesto prima e dopo, prese dal file di adesso. */
  private rowsOf(h: ChangeHunk, d: RootChangeDetail): HunkRow[] {
    const current = this.lines(d.currentText);
    const rows: HunkRow[] = [];
    for (let i = Math.max(0, h.currentStart - RootReviewPanelComponent.Context); i < h.currentStart; i++)
      rows.push({ kind: 'ctx', line: i + 1, text: current[i] });
    h.originalLines.forEach((t, i) => rows.push({ kind: 'del', line: h.originalStart + i + 1, text: this.strip(t) }));
    h.currentLines.forEach((t, i) => rows.push({ kind: 'add', line: h.currentStart + i + 1, text: this.strip(t) }));
    const after = h.currentStart + h.currentLines.length;
    for (let i = after; i < Math.min(current.length, after + RootReviewPanelComponent.Context); i++)
      rows.push({ kind: 'ctx', line: i + 1, text: current[i] });
    return rows;
  }

  private lines(text: string | null): string[] {
    if (!text) return [];
    const all = text.replace(/\r\n/g, '\n').split('\n');
    return all.length > 0 && all[all.length - 1] === '' ? all.slice(0, -1) : all;
  }

  private strip(line: string): string { return line.replace(/\r?\n$/, ''); }

  get wholeFileOnly(): boolean { return this.selected?.status !== 'modified'; }

  private async run(action: () => Promise<unknown>): Promise<void> {
    this.busy = true;
    this.error = null;
    try { await action(); }
    catch (e: any) {
      this.error = e?.error?.error || e?.message || String(e);
      await this.review.refresh();
    }
    finally { this.busy = false; }
  }

  keepHunk(h: ChangeHunk): Promise<void> { return this.run(() => this.review.keep(this.selected.alias, this.selected.relative, h)); }
  undoHunk(h: ChangeHunk): Promise<void> { return this.run(() => this.review.undo(this.selected.alias, this.selected.relative, h)); }
  keepFile(): Promise<void> { return this.run(() => this.review.keep(this.selected.alias, this.selected.relative)); }
  undoFile(): Promise<void> { return this.run(() => this.review.undo(this.selected.alias, this.selected.relative)); }

  all(keep: boolean): Promise<void> {
    return this.run(async () => { this.problems = await this.review.all(keep); });
  }

  statusLabel(f: RootChangeSummary): string {
    return this.translate.instant('ROOT_REVIEW.STATUS_' + f.status.toUpperCase());
  }
}
