import { Component, Inject } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { CodeGraphService, CodeLanguagePreview, CodeLanguageProposal, CodeRuleAnalysis } from '../../md-explorer/services/code-graph.service';

export interface CodeLanguageWizardData {
  projectPath: string;
  proposal: CodeLanguageProposal;
}

/** Un file del campione come lo mostra il wizard: i fatti trovati e la verifica di Mark. */
interface SampleRow {
  path: string;
  facts: string[];
  ok: boolean | null;
  note: string | null;
}

/**
 * «Migliora il grafo» per un linguaggio (D42 passo 2, 24/10/2026): MarkAgent ha scritto il dialetto delle pagine (o un
 * linguaggio a righe) e l'utente decide. Come il wizard delle regole, per chi non è tecnico: il problema, i controlli di
 * MdExplorer e di Mark, un sì o un no; alla fine si può annullare. Niente scelta della portata: un linguaggio vale per
 * tutti i file con le sue estensioni.
 */
@Component({
  selector: 'app-code-language-wizard',
  templateUrl: './code-language-wizard.component.html',
  styleUrls: ['../code-rule-wizard/code-rule-wizard.component.scss']
})
export class CodeLanguageWizardComponent {
  readonly steps = ['PROBLEM', 'CHECKS', 'CONFIRM', 'DONE'];
  step = 0;

  proposal: CodeLanguageProposal;
  preview: CodeLanguagePreview;
  sampleRows: SampleRow[];
  kindRows: { kind: string; count: number }[];
  unresolvedRows: { target: string; count: number }[];

  busy = false;
  error: string | null = null;
  appliedId: string | null = null;
  undone = false;
  analyses: CodeRuleAnalysis[] = [];

  constructor(
    private dialogRef: MatDialogRef<CodeLanguageWizardComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CodeLanguageWizardData,
    private codeGraph: CodeGraphService
  ) {
    this.proposal = data.proposal;
    this.preview = data.proposal.preview;
    this.sampleRows = (this.preview.sample ?? []).map(s => {
      const check = (this.proposal.sampleChecks ?? []).find(c => c.path === s.path);
      return { path: s.path, facts: s.facts ?? [], ok: check ? check.ok : null, note: check?.note ?? null };
    });
    this.kindRows = Object.keys(this.preview.symbolsByKind ?? {}).map(k => ({ kind: k, count: this.preview.symbolsByKind[k] }))
      .sort((a, b) => b.count - a.count);
    const unresolved = this.preview.pages?.unresolvedIncludes ?? {};
    this.unresolvedRows = Object.keys(unresolved).map(k => ({ target: k, count: unresolved[k] })).sort((a, b) => b.count - a.count);
  }

  get followableIncludes(): number {
    const p = this.preview.pages;
    return p ? p.includes - p.dynamicIncludes : 0;
  }

  get extensions(): string {
    return (this.preview.extensions ?? []).join(', ');
  }

  get folders(): string {
    return (this.preview.folders ?? []).map(f => f.folder).join(', ');
  }

  next(): void {
    if (this.step < 2) this.step++;
  }

  back(): void {
    if (this.step > 0 && this.step < 3) this.step--;
  }

  /** «Sì»: il linguaggio va nel progetto e le cartelle si rianalizzano in sfondo. */
  apply(): void {
    this.busy = true;
    this.error = null;
    this.codeGraph.applyLanguageProposal(this.data.projectPath, this.proposal.id).subscribe({
      next: r => {
        this.busy = false;
        this.appliedId = r.languageId;
        this.step = 3;
        this.codeGraph.rules(this.data.projectPath).subscribe({
          next: x => this.analyses = x.analyses ?? [],
          error: () => this.analyses = this.proposal.furtherAnalyses ?? []
        });
      },
      error: e => { this.busy = false; this.error = this.errorText(e); }
    });
  }

  /** «No»: la proposta si scarta; Mark può riproporla corretta. */
  discard(): void {
    this.busy = true;
    this.codeGraph.discardLanguageProposal(this.data.projectPath, this.proposal.id).subscribe({
      next: () => { this.busy = false; this.dialogRef.close('discarded'); },
      error: e => { this.busy = false; this.error = this.errorText(e); }
    });
  }

  undo(): void {
    if (!this.appliedId) return;
    this.busy = true;
    this.codeGraph.undoLanguage(this.data.projectPath, this.appliedId).subscribe({
      next: () => { this.busy = false; this.undone = true; },
      error: e => { this.busy = false; this.error = this.errorText(e); }
    });
  }

  close(): void {
    this.dialogRef.close(this.appliedId && !this.undone ? 'applied' : 'closed');
  }

  private errorText(e: any): string {
    return e?.error?.error ?? e?.message ?? String(e);
  }
}
