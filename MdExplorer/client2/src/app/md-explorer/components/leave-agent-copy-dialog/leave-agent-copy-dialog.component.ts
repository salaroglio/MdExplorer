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

  constructor(
    public dialogRef: MatDialogRef<LeaveAgentCopyDialogComponent, string | null>,
    @Inject(MAT_DIALOG_DATA) public data: LeaveAgentCopyData,
  ) {
    this.message = '';
  }

  get hasFiles(): boolean {
    return this.data.pending.uncommitted.length > 0;
  }

  get canAuthorize(): boolean {
    return !this.hasFiles || this.message.trim().length > 0;
  }

  authorize(): void {
    if (this.canAuthorize) this.dialogRef.close(this.message.trim());
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
