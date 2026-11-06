import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AiChatService } from '../../../../services/ai-chat.service';
import { AiSelectionService } from '../../../../services/ai-selection.service';

export interface AiSelectionDialogData {
  documentPath: string;
  startLine: number;
  endLine: number;
  selectedText: string;
  connectionId: string;
}

/**
 * Extracts the markdown fragment from the AI reply: content between the FIRST
 * and the LAST fence line (```...). First→last handles fences nested inside the
 * fragment. Without fences the whole trimmed reply is used, flagged as
 * non-delimited so the UI shows a warning badge.
 */
export function extractMarkdownFragment(response: string): { fragment: string; delimited: boolean } {
  const lines = response.split('\n');
  const fenceIndexes: number[] = [];
  for (let i = 0; i < lines.length; i++) {
    if (/^\s*`{3,}/.test(lines[i])) {
      fenceIndexes.push(i);
    }
  }
  if (fenceIndexes.length >= 2) {
    const first = fenceIndexes[0];
    const last = fenceIndexes[fenceIndexes.length - 1];
    return { fragment: lines.slice(first + 1, last).join('\n'), delimited: true };
  }
  return { fragment: response.trim(), delimited: false };
}

/**
 * Dialog opened by the floating "Usa AI" button in the rendered document.
 * Shows the exact markdown source lines behind the selection, lets the user
 * ask an AI (whatever provider is configured on AiChatHub) for a modification,
 * previews the proposal and applies it with a deterministic replace.
 */
@Component({
  selector: 'app-ai-selection-dialog',
  templateUrl: './ai-selection-dialog.component.html',
  styleUrls: ['./ai-selection-dialog.component.scss']
})
export class AiSelectionDialogComponent implements OnInit, OnDestroy {

  fileName: string;
  fragment: string | null = null;
  fragmentError: string | null = null;

  instruction = '';
  isStreaming = false;
  streamingResponse = '';
  thinkingContent = '';
  errorMessage: string | null = null;

  proposal: string | null = null;
  proposalDelimited = true;

  conflict = false;
  isApplying = false;

  private readonly channelId = 'ai-selection-' + Date.now();
  private firstMessageSent = false;
  private destroy$ = new Subject<void>();

  constructor(
    private dialogRef: MatDialogRef<AiSelectionDialogComponent>,
    private aiSelectionService: AiSelectionService,
    private aiChatService: AiChatService,
    @Inject(MAT_DIALOG_DATA) public data: AiSelectionDialogData,
  ) {
    const normalized = (data.documentPath || '').replace(/\\/g, '/');
    this.fileName = normalized.substring(normalized.lastIndexOf('/') + 1);
  }

  ngOnInit(): void {
    this.loadFragment();

    this.aiChatService.getChannelStream$(this.channelId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(event => {
        switch (event.type) {
          case 'chunk':
            this.streamingResponse += event.data;
            break;
          case 'thinking':
            this.thinkingContent += event.data;
            break;
          case 'complete': {
            this.isStreaming = false;
            if (this.streamingResponse.trim()) {
              const extracted = extractMarkdownFragment(this.streamingResponse);
              this.proposal = extracted.fragment;
              this.proposalDelimited = extracted.delimited;
            }
            break;
          }
          case 'error':
            this.isStreaming = false;
            this.errorMessage = event.data;
            break;
        }
      });
  }

  loadFragment(): void {
    this.fragmentError = null;
    this.conflict = false;
    this.proposal = null;
    this.aiSelectionService.getFragment(this.data.documentPath, this.data.startLine, this.data.endLine, this.data.connectionId)
      .subscribe({
        next: response => this.fragment = response.fragment,
        error: err => this.fragmentError = err?.error?.error || err.message || 'Error loading fragment'
      });
  }

  send(): void {
    const instruction = this.instruction.trim();
    if (!instruction || this.isStreaming || this.fragment === null) {
      return;
    }
    this.errorMessage = null;
    this.streamingResponse = '';
    this.thinkingContent = '';
    this.proposal = null;
    this.isStreaming = true;

    const message = this.firstMessageSent
      ? this.buildFollowUpPrompt(instruction)
      : this.buildFirstPrompt(instruction);
    this.firstMessageSent = true;
    this.aiChatService.sendMessageToChannel(message, this.channelId);
    this.instruction = '';
  }

  stop(): void {
    this.aiChatService.cancelPrompt();
    this.isStreaming = false;
  }

  approve(): void {
    if (this.proposal === null || this.fragment === null || this.isApplying) {
      return;
    }
    this.isApplying = true;
    this.errorMessage = null;
    this.aiSelectionService.replaceSection({
      path: this.data.documentPath,
      startLine: this.data.startLine,
      endLine: this.data.endLine,
      expectedOriginalText: this.fragment,
      newText: this.proposal,
      connectionId: this.data.connectionId || undefined
    }).subscribe({
      next: () => {
        this.isApplying = false;
        this.dialogRef.close(true);
      },
      error: err => {
        this.isApplying = false;
        if (err?.status === 409) {
          this.conflict = true;
        } else {
          this.errorMessage = err?.error?.error || err.message || 'Error applying the change';
        }
      }
    });
  }

  dismiss(): void {
    this.dialogRef.close(false);
  }

  ngOnDestroy(): void {
    if (this.isStreaming) {
      this.aiChatService.cancelPrompt();
    }
    if (this.firstMessageSent) {
      this.aiChatService.clearChannelHistory(this.channelId);
    }
    this.destroy$.next();
    this.destroy$.complete();
  }

  private buildFirstPrompt(instruction: string): string {
    return [
      `Sei un editor di documenti Markdown. Questo è un frammento ESATTO del file "${this.fileName}" (righe ${this.data.startLine}-${this.data.endLine}):`,
      '',
      '<frammento>',
      this.fragment,
      '</frammento>',
      '',
      `Testo selezionato dall'utente all'interno del frammento: "${this.data.selectedText}"`,
      '',
      `Istruzione: ${instruction}`,
      '',
      'Rispondi ESCLUSIVAMENTE con il frammento Markdown modificato COMPLETO, racchiuso in un unico blocco ```markdown ... ```.',
      `Il tuo output sostituirà integralmente le righe ${this.data.startLine}-${this.data.endLine}: non aggiungere spiegazioni, saluti o testo fuori dal blocco.`,
      'Non modificare le parti del frammento non toccate dall\'istruzione. Non aggiungere code fence attorno a contenuto che non le aveva.',
      'Non leggere né modificare file: lavora solo sul frammento fornito.'
    ].join('\n');
  }

  private buildFollowUpPrompt(instruction: string): string {
    return [
      instruction,
      '',
      'Rispondi di nuovo ESCLUSIVAMENTE con il frammento Markdown modificato COMPLETO in un unico blocco ```markdown ... ```, senza testo fuori dal blocco.'
    ].join('\n');
  }
}
