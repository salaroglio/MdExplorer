import { Component, Inject, OnInit } from '@angular/core';
import {
  MatLegacyDialogRef as MatDialogRef,
  MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA,
} from '@angular/material/legacy-dialog';
import { firstValueFrom } from 'rxjs';

import { AgentMemoryService, MemoryFact } from '../../services/agent-memory.service';

export interface AgentMemoryDialogData {
  projectPath: string;
}

/**
 * La vista UMANA della memoria degli agenti (§11 Fase 5d): elenca i fatti appresi di
 * tutti i cittadini + del grafo condiviso, e permette la curatela — cambiare la
 * confidence o rimuovere un fatto. La memoria del singolo agente resta isolata lato
 * agente (RunToken); qui l'umano ha la vista d'insieme, come "editor" della città.
 */
@Component({
  selector: 'app-agent-memory-dialog',
  templateUrl: './agent-memory-dialog.component.html',
  styleUrls: ['./agent-memory-dialog.component.scss'],
})
export class AgentMemoryDialogComponent implements OnInit {
  facts: MemoryFact[] = [];
  loading = false;
  error: string | null = null;
  diary: string | null = null;

  constructor(
    public dialogRef: MatDialogRef<AgentMemoryDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: AgentMemoryDialogData,
    private memory: AgentMemoryService,
  ) {}

  ngOnInit(): void {
    this.reload();
  }

  async reload(): Promise<void> {
    this.loading = true;
    this.error = null;
    this.diary = null;
    try {
      const res = await firstValueFrom(this.memory.listFacts(this.data.projectPath));
      this.facts = res?.facts || [];
    } catch (e: any) {
      this.error = e?.error?.error || 'Impossibile leggere la memoria (Fuseki abilitato?).';
      this.facts = [];
    } finally {
      this.loading = false;
    }
  }

  async saveConfidence(fact: MemoryFact): Promise<void> {
    try {
      await firstValueFrom(this.memory.setConfidence(this.data.projectPath, fact.graph, fact.factUri, fact.confidence));
    } catch (e: any) {
      this.error = e?.error?.error || 'Aggiornamento confidence fallito.';
      await this.reload();
    }
  }

  async deleteFact(fact: MemoryFact): Promise<void> {
    try {
      await firstValueFrom(this.memory.deleteFact(this.data.projectPath, fact.graph, fact.factUri));
      this.facts = this.facts.filter((f) => f.factUri !== fact.factUri);
    } catch (e: any) {
      this.error = e?.error?.error || 'Rimozione fallita.';
    }
  }

  async toggleDiary(): Promise<void> {
    if (this.diary !== null) { this.diary = null; return; }
    try {
      this.diary = await firstValueFrom(this.memory.getDiary(this.data.projectPath));
    } catch (e: any) {
      this.error = e?.error?.error || 'Diario non disponibile.';
    }
  }

  close(): void {
    this.dialogRef.close();
  }
}
