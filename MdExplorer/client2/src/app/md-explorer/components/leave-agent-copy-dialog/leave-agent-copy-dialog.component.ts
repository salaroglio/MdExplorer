import { Component, Inject } from '@angular/core';
import {
  MatLegacyDialogRef as MatDialogRef,
  MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA,
} from '@angular/material/legacy-dialog';

export interface PendingInCopy {
  uncommitted: { repo: string; label: string; files: string[] }[];
  unpublished: { repo: string; label: string; commits: number }[];
}

export interface LeaveAgentCopyData {
  agent: string;
  pending: PendingInCopy;
  /**
   * 'leave' (default) = si esce dalla copia di un agente. 'start' = sta per partire un agente, e nella cartella
   * della persona c'è lavoro che lui non vedrebbe: stessa finestra, con in più «Avvia senza salvare».
   */
  mode?: 'leave' | 'start';
  /**
   * Solo nel lancio a mano con la copia isolata: si offre la casella «metti questo lavoro in un ramo nuovo e fai
   * partire l'agente da lì» (sprint 2026-11-25-Nuovo-Ramo-E-Lavoro-All-Agente, B3).
   */
  offerNewBranch?: boolean;
}

/** La risposta con la casella spuntata: il ramo da creare e il messaggio del commit del lavoro aperto. */
export interface NewBranchAnswer {
  branch: string;
  message: string;
}

/** Con questo valore la finestra dice: procedi senza salvare (solo in modalità 'start'). */
export const PROCEED_WITHOUT_SAVING = '\u0000proceed-without-saving';

/**
 * L'uscita dalla copia di un agente quando c'è lavoro da salvare: si vede cosa verrà committato e pubblicato,
 * e lo si autorizza con un gesto solo. Chiude con il messaggio del commit (autorizzato) o con null (si resta).
 */
@Component({
  selector: 'app-leave-agent-copy-dialog',
  templateUrl: './leave-agent-copy-dialog.component.html',
  styleUrls: ['./leave-agent-copy-dialog.component.scss'],
})
export class LeaveAgentCopyDialogComponent {
  message: string;
  /** La casella «nel ramo nuovo». */
  inNewBranch = false;
  branchName: string;

  constructor(
    public dialogRef: MatDialogRef<LeaveAgentCopyDialogComponent, string | NewBranchAnswer | null>,
    @Inject(MAT_DIALOG_DATA) public data: LeaveAgentCopyData,
  ) {
    this.message = '';
    const d = new Date();
    const pad = (n: number) => String(n).padStart(2, '0');
    // Dal lancio a mano arriva il nome del file (curatore.agent.md): nel ramo va il nome dell'agente.
    const agent = (data.agent || 'agente').replace(/(\.agent)?\.md$/i, '').toLowerCase().replace(/[^a-z0-9._-]+/g, '-').replace(/^-+|-+$/g, '');
    this.branchName = `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}-${agent || 'agente'}`;
  }

  /** La casella si mostra nel lancio a mano quando c'è lavoro senza commit. */
  get showNewBranch(): boolean {
    return this.starting && !!this.data.offerNewBranch && this.hasFiles;
  }

  /**
   * Il ramo nasce solo nel repository del progetto: lavoro aperto o commit non pubblicati dentro un submodule non
   * arriverebbero all'agente, quindi la casella si spegne e dice perché.
   */
  get submoduleWork(): string[] {
    const labels = [
      ...this.data.pending.uncommitted.filter(r => !!r.repo).map(r => r.label),
      ...this.data.pending.unpublished.filter(r => !!r.repo).map(r => r.label),
    ];
    return Array.from(new Set(labels));
  }

  get hasFiles(): boolean {
    return this.data.pending.uncommitted.length > 0;
  }

  get canAuthorize(): boolean {
    if (this.inNewBranch && !this.branchName.trim()) return false;
    return !this.hasFiles || this.message.trim().length > 0;
  }

  authorize(): void {
    if (!this.canAuthorize) return;
    if (this.inNewBranch && this.showNewBranch && !this.submoduleWork.length)
      this.dialogRef.close({ branch: this.branchName.trim(), message: this.message.trim() });
    else
      this.dialogRef.close(this.message.trim());
  }

  stay(): void {
    this.dialogRef.close(null);
  }

  get starting(): boolean {
    return this.data.mode === 'start';
  }

  /** Le chiavi di traduzione dei testi, che cambiano con il motivo per cui la finestra si apre. */
  get k(): string {
    return this.starting ? 'BEFORE_START' : 'LEAVE_COPY';
  }

  proceedWithoutSaving(): void {
    this.dialogRef.close(PROCEED_WITHOUT_SAVING);
  }
}
