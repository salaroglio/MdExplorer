import { Component, Inject, OnDestroy } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, switchMap } from 'rxjs/operators';
import {
  CodeGraphService, CodeRuleAnalysis, CodeRuleContext, CodeRulePreview, CodeRuleProposal, CodeRuleSiteResult
} from '../../md-explorer/services/code-graph.service';

export interface CodeRuleWizardData {
  projectPath: string;
  proposal: CodeRuleProposal;
}

/** Un collegamento del campione come lo mostra il wizard: dove, la stringa, il metodo trovato e la verifica di Mark. */
interface SampleRow {
  where: string;
  value: string;
  target: string;
  ok: boolean | null;
  note: string | null;
}

/**
 * «Migliora il grafo» (D42, 23/10/2026): il wizard che porta all'utente una regola di arricchimento proposta da
 * MarkAgent. Pensato per chi non è tecnico: il problema in parole semplici, dove applicarla (i contesti dal più grande
 * al più piccolo), i controlli già fatti da MdExplorer e da Mark, un sì o un no; alla fine si può sempre annullare.
 */
@Component({
  selector: 'app-code-rule-wizard',
  templateUrl: './code-rule-wizard.component.html',
  styleUrls: ['./code-rule-wizard.component.scss']
})
export class CodeRuleWizardComponent implements OnDestroy {
  readonly steps = ['PROBLEM', 'SCOPE', 'CHECKS', 'CONFIRM', 'DONE'];
  step = 0;

  proposal: CodeRuleProposal;
  /** I numeri con la portata scelta: all'inizio quelli di tutto il grafo, arrivati con la proposta. */
  preview: CodeRulePreview;
  contexts: CodeRuleContext[];
  wholeGraph = true;
  chosen = new Set<string>();
  previewLoading = false;

  busy = false;
  error: string | null = null;
  appliedRuleId: string | null = null;
  appliedEdges = 0;
  undone = false;
  analyses: CodeRuleAnalysis[] = [];

  sampleRows: SampleRow[] = [];
  missRows: { reason: string; count: number }[] = [];

  private scopeChanged$ = new Subject<string[]>();
  private sub: Subscription;

  constructor(
    private dialogRef: MatDialogRef<CodeRuleWizardComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CodeRuleWizardData,
    private codeGraph: CodeGraphService
  ) {
    this.proposal = data.proposal;
    this.contexts = data.proposal.preview.contexts ?? [];
    this.setPreview(data.proposal.preview);
    this.sub = this.scopeChanged$.pipe(
      debounceTime(300),
      switchMap(scope => {
        this.previewLoading = true;
        return this.codeGraph.previewRuleProposal(this.data.projectPath, this.proposal.id, scope);
      })
    ).subscribe({
      next: p => { this.previewLoading = false; this.setPreview(p); },
      error: e => { this.previewLoading = false; this.error = this.errorText(e); }
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  get scope(): string[] {
    return this.wholeGraph ? [] : Array.from(this.chosen);
  }

  /** Avanti dalla portata solo con almeno un contesto scelto e qualcosa da collegare. */
  get canGoOn(): boolean {
    if (this.step === 1) return (this.wholeGraph || this.chosen.size > 0) && !this.previewLoading && this.preview.edgeCount > 0;
    return true;
  }

  next(): void {
    if (this.step < 3 && this.canGoOn) this.step++;
  }

  back(): void {
    if (this.step > 0 && this.step < 4) this.step--;
  }

  setWholeGraph(whole: boolean): void {
    this.wholeGraph = whole;
    this.scopeChanged$.next(this.scope);
  }

  toggleContext(c: CodeRuleContext, on: boolean): void {
    if (on) this.chosen.add(c.prefix); else this.chosen.delete(c.prefix);
    this.scopeChanged$.next(this.scope);
  }

  sharePercent(p: CodeRulePreview): number {
    return Math.round((p?.resolvedShare ?? 0) * 100);
  }

  /** «Sì»: la regola con la portata scelta va nel progetto e i suoi collegamenti nel grafo. */
  apply(): void {
    this.busy = true;
    this.error = null;
    this.codeGraph.applyRuleProposal(this.data.projectPath, this.proposal.id, this.scope).subscribe({
      next: r => {
        this.busy = false;
        this.appliedRuleId = r.ruleId;
        this.appliedEdges = r.preview.edgeCount;
        this.step = 4;
        this.loadAnalyses();
      },
      error: e => { this.busy = false; this.error = this.errorText(e); }
    });
  }

  /** «No»: la proposta si scarta; Mark può riproporla corretta. */
  discard(): void {
    this.busy = true;
    this.codeGraph.discardRuleProposal(this.data.projectPath, this.proposal.id).subscribe({
      next: () => { this.busy = false; this.dialogRef.close('discarded'); },
      error: e => { this.busy = false; this.error = this.errorText(e); }
    });
  }

  undo(): void {
    if (!this.appliedRuleId) return;
    this.busy = true;
    this.codeGraph.removeRule(this.data.projectPath, this.appliedRuleId).subscribe({
      next: () => { this.busy = false; this.undone = true; },
      error: e => { this.busy = false; this.error = this.errorText(e); }
    });
  }

  close(): void {
    this.dialogRef.close(this.appliedRuleId && !this.undone ? 'applied' : 'closed');
  }

  private loadAnalyses(): void {
    this.codeGraph.rules(this.data.projectPath).subscribe({
      next: r => this.analyses = r.analyses ?? [],
      error: () => this.analyses = this.proposal.furtherAnalyses ?? []
    });
  }

  private setPreview(p: CodeRulePreview): void {
    this.preview = p;
    this.sampleRows = (p.sample ?? []).map(s => this.toRow(s));
    this.missRows = Object.keys(p.missCounts ?? {}).map(k => ({ reason: k, count: p.missCounts[k] })).sort((a, b) => b.count - a.count);
  }

  private toRow(s: CodeRuleSiteResult): SampleRow {
    const check = (this.proposal.sampleChecks ?? []).find(c => c.path === s.site.path && c.line === s.site.line);
    return {
      where: `${s.site.path}:${s.site.line}`,
      value: s.value,
      target: (s.targets ?? []).map(t => t.memberQualifiedName || `${t.ownerQualifiedName}.${t.memberName}`).join(', '),
      ok: check ? check.ok : null,
      note: check?.note ?? null
    };
  }

  private errorText(e: any): string {
    return e?.error?.error ?? e?.message ?? String(e);
  }
}
