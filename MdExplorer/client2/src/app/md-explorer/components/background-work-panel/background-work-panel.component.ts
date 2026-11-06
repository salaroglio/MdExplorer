import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy } from '@angular/core';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { Observable, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { BackgroundRow, BackgroundWorkService } from '../../services/background-work.service';

/**
 * Il pannello «Costruzione della conoscenza» (punto 2d del 14/10/2026): fisso in basso a destra, un lavoro per riga,
 * riducibile a un'icona. Non è una MatSnackBar perché Material ne tiene una alla volta e ogni altro avviso la chiudeva.
 * Si riapre da solo quando compare una descrizione interrotta o fallita: è il modo in cui MdExplorer lo dice.
 */
@Component({
  selector: 'app-background-work-panel',
  templateUrl: './background-work-panel.component.html',
  styleUrls: ['./background-work-panel.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BackgroundWorkPanelComponent implements OnDestroy {
  rows$: Observable<BackgroundRow[]>;
  collapsed = false;
  busy = new Set<string>();
  private alerted = new Set<string>();
  private destroy$ = new Subject<void>();

  constructor(private work: BackgroundWorkService, private snackBar: MatSnackBar, private translate: TranslateService, private cdr: ChangeDetectorRef) {
    this.rows$ = this.work.rows$;
    this.work.rows$.pipe(takeUntil(this.destroy$)).subscribe(rows => {
      // una riga che chiede una decisione apre il pannello, una volta per riga
      for (const r of rows) {
        if ((r.state === 'interrupted' || r.state === 'failed') && !this.alerted.has(r.id)) {
          this.alerted.add(r.id);
          this.collapsed = false;
          this.cdr.markForCheck();
        }
      }
    });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  trackRow(_: number, r: BackgroundRow): string { return r.id; }

  /** La riga che riassume il pannello ridotto: la prima che chiede una decisione, sennò la prima in corso. */
  lead(rows: BackgroundRow[]): BackgroundRow {
    return rows.find(r => r.state === 'interrupted' || r.state === 'failed') ?? rows.find(r => r.state === 'running') ?? rows[0];
  }

  needsDecision(r: BackgroundRow): boolean {
    return r.kind === 'description' && !!r.folder && (r.state === 'interrupted' || r.state === 'failed' || r.state === 'stopped');
  }

  eta(ms: number | null | undefined): string {
    if (ms == null) { return ''; }
    const min = Math.round(ms / 60000);
    if (min < 1) { return '< 1 min'; }
    if (min < 60) { return `~${min} min`; }
    const h = Math.floor(min / 60);
    return `~${h} h ${min % 60} min`;
  }

  when(iso?: string): string { return iso ? new Date(iso).toLocaleString() : ''; }

  resume(r: BackgroundRow): void { this.act(r, this.work.resume(r)); }
  dismiss(r: BackgroundRow): void { this.act(r, this.work.dismiss(r)); }
  stop(r: BackgroundRow): void { this.act(r, this.work.stop(r)); }
  close(r: BackgroundRow): void { this.work.close(r); }

  private act(r: BackgroundRow, call: Observable<any>): void {
    this.busy.add(r.id);
    this.cdr.markForCheck();
    call.subscribe({
      next: () => { this.busy.delete(r.id); this.cdr.markForCheck(); },
      error: err => {
        this.busy.delete(r.id);
        this.cdr.markForCheck();
        this.snackBar.open(this.translate.instant('BACKGROUND_WORK.ACTION_FAILED', { error: err?.error?.error || err?.message || err }), 'OK', { duration: 8000 });
      },
    });
  }
}
