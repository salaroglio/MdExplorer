import { Component, Inject, OnInit } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { TranslateService } from '@ngx-translate/core';
import { CodeGraphService, SeedAnalysis, SeedClosureFile, SeedClosureMiss, SeedImplementations, SeedPreview } from '../../../services/code-graph.service';

export interface SeedDialogData {
  projectRoot: string;
  /** Il file da cui partire, col percorso logico (@alias/… o del progetto). */
  seed: string;
}

/**
 * «Analizza questo programma» (F3c del piano 2026-11-10-Analisi-Di-Programmi-Singoli): l'anteprima della chiusura di un
 * file — i file per livello, da dove arrivano, che cosa se ne usa; i riferimenti non seguiti — con le potature
 * dell'utente, poi l'analisi dei soli file della chiusura. Tutto col parser: niente LLM, niente token. Se la chiusura è
 * cambiata da quella confermata, la finestra mostra che cosa entra ed esce e chiede conferma (S14).
 */
@Component({
  selector: 'app-seed-dialog',
  templateUrl: './seed-dialog.component.html',
  styleUrls: ['./seed-dialog.component.scss']
})
export class SeedDialogComponent implements OnInit {
  public loading = false;
  public applying = false;
  public errorMessage = '';
  public preview: SeedPreview | null = null;
  public levels: { depth: number; files: SeedClosureFile[] }[] = [];
  public missGroups: { kind: string; items: SeedClosureMiss[]; uses: number }[] = [];
  public openMisses: string | null = null;
  /** I file tolti dall'utente; dirty = tolti o rimessi dopo l'ultima anteprima. */
  public pruned = new Set<string>();
  /** F4c: le implementazioni scelte (classi da seguire quando un metodo di interfaccia ne ha più d'una). */
  public included = new Set<string>();
  public implChoices: SeedImplementations[] = [];
  public implAuto: SeedImplementations[] = [];
  public showAuto = false;
  public dirty = false;
  public confirmation: SeedAnalysis | null = null;
  public done: SeedAnalysis | null = null;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: SeedDialogData,
    private dialogRef: MatDialogRef<SeedDialogComponent, SeedAnalysis | null>,
    private codeGraph: CodeGraphService,
    private translate: TranslateService,
  ) { }

  /** Un seme già salvato riparte dalle sue esclusioni (F3d); uno nuovo da nessuna. */
  ngOnInit(): void {
    this.codeGraph.listSeeds(this.data.projectRoot).subscribe({
      next: list => {
        const saved = list.find(s => s.seed === this.data.seed);
        if (saved) { this.pruned = new Set(saved.pruned); this.prunedList = [...this.pruned].sort(); this.included = new Set(saved.included ?? []); }
        this.load();
      },
      error: () => this.load(),
    });
  }

  load(): void {
    this.loading = true;
    this.errorMessage = '';
    this.confirmation = null;
    this.codeGraph.previewSeed(this.data.projectRoot, this.data.seed, [...this.pruned], [...this.included]).subscribe({
      next: p => {
        this.loading = false;
        this.dirty = false;
        this.preview = p;
        const byDepth = new Map<number, SeedClosureFile[]>();
        for (const f of p.files) { byDepth.set(f.depth, [...(byDepth.get(f.depth) ?? []), f]); }
        this.levels = [...byDepth.entries()].sort((a, b) => a[0] - b[0]).map(([depth, files]) => ({ depth, files }));
        const byKind = new Map<string, SeedClosureMiss[]>();
        for (const m of p.misses.filter(x => x.kind !== 'pruned')) { byKind.set(m.kind, [...(byKind.get(m.kind) ?? []), m]); }
        this.implChoices = (p.implementations ?? []).filter(i => !i.auto && i.candidates.length > 1)
          .sort((a, b) => b.created.length - a.created.length || a.contract.localeCompare(b.contract));
        this.implAuto = (p.implementations ?? []).filter(i => i.auto);
        this.missGroups = ['frontier', 'ambiguous', 'unresolved'].filter(k => byKind.has(k))
          .map(kind => ({ kind, items: byKind.get(kind)!, uses: byKind.get(kind)!.reduce((n, m) => n + m.uses, 0) }));
      },
      error: err => { this.loading = false; this.errorMessage = err?.error?.error || err?.message || this.translate.instant('COMMON.ERROR'); }
    });
  }

  isSeed(f: SeedClosureFile): boolean { return f.depth === 0; }

  /** Una spunta tolta = file potato: lui e quello che portava escono alla prossima anteprima. */
  toggle(path: string, kept: boolean): void {
    if (kept) { this.pruned.delete(path); } else { this.pruned.add(path); }
    this.prunedList = [...this.pruned].sort();
    this.dirty = true;
  }

  restore(path: string): void { this.toggle(path, true); }

  /** F4c: una spunta su un'implementazione = seguila (entra con la sua catena alla prossima anteprima). */
  toggleImplementation(path: string, chosen: boolean): void {
    if (chosen) { this.included.add(path); } else { this.included.delete(path); }
    this.dirty = true;
  }

  /** «Repository.save» da «com.acme.api.Repository.save». */
  contractName(contract: string): string { const parts = contract.split('.'); return parts.slice(-2).join('.'); }

  /** Un campo, non un getter: un getter che crea una lista in un *ngFor fa lavorare a vuoto il rilevamento dei cambi. */
  public prunedList: string[] = [];

  name(path: string): string { return path.substring(path.lastIndexOf('/') + 1); }

  folder(path: string): string { const i = path.lastIndexOf('/'); return i > 0 ? path.substring(0, i) : ''; }

  /** Dove si legge il riferimento, senza il percorso intero: «main.toy:29». */
  shortFrom(from: string | null): string { return from ? this.name(from) : ''; }

  analyze(confirm: boolean): void {
    this.applying = true;
    this.errorMessage = '';
    this.codeGraph.applySeed(this.data.projectRoot, this.data.seed, [...this.pruned], confirm, [...this.included]).subscribe({
      next: r => {
        this.applying = false;
        if (r.needsConfirmation) { this.confirmation = r; return; }
        this.confirmation = null;
        this.done = r;
      },
      error: err => { this.applying = false; this.errorMessage = err?.error?.error || err?.message || this.translate.instant('COMMON.ERROR'); }
    });
  }

  close(): void { this.dialogRef.close(this.done); }
}
