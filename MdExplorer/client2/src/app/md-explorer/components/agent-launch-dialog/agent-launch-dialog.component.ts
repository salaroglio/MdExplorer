import { EMPTY, Observable, of } from 'rxjs';
import { finalize, switchMap } from 'rxjs/operators';
import { AgentStartGuardService } from '../../services/agent-start-guard.service';
import { AiChatService } from '../../../services/ai-chat.service';
import { Component, Inject, OnInit } from '@angular/core';
import {
  MatLegacyDialog as MatDialog,
  MatLegacyDialogRef as MatDialogRef,
  MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA,
} from '@angular/material/legacy-dialog';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';

import { AgentEngineInfo, AgentLaunchService, AgentParam } from '../../services/agent-launch.service';
import { MailboxService } from '../../services/mailbox.service';
import { ProjectSettingsService } from '../../../projects/services/project-settings.service';
import { AgentScheduleService } from '../../services/agent-schedule.service';
import { AgentQueue, AgentQueueService } from '../../services/agent-queue.service';
import { AgentScheduleDialogComponent } from '../agent-schedule-dialog/agent-schedule-dialog.component';
import { ShowFileSystemComponent } from '../../../commons/components/show-file-system/show-file-system.component';
import { ShowFileMetadata } from '../../../commons/components/show-file-system/show-file-metadata';

export interface AgentLaunchDialogData {
  projectPath: string;
  agentFilePath: string;
  agentName: string;
  /**
   * Un incarico ricevuto da un altro agente, che il workflow fa avviare al responsabile (start: ask-owner). La
   * schermata lo mostra così com'è; chi avvia aggiunge solo le sue indicazioni, e sceglie motore e modello.
   */
  incoming?: { messageId: string; fromAgent: string; body: string; step?: string | null };
}

/**
 * Mask to prepare and launch the prompt of a *.agent.md agent.
 * Free text → "Normalize with Copilot" (mde-prompt-for-agents skill) → parameter
 * pickers (ParameterExtractor grammar) → launch headless.
 */
@Component({
  selector: 'app-agent-launch-dialog',
  templateUrl: './agent-launch-dialog.component.html',
  styleUrls: ['./agent-launch-dialog.component.scss'],
})
export class AgentLaunchDialogComponent implements OnInit {
  // Coda di lavoro dell'agente (§12.5/§12.6, Fase 6d): parcheggiati + federate in attesa.
  queue: AgentQueue | null = null;
  queueLoading = false;
  // What the user reads/edits in the textarea: the free text before normalization,
  // and ONLY the `## Task` body afterwards. The machine scaffolding (title + the
  // ```params declaration block) is kept out of sight in `headerPart` — it still
  // drives the pickers and is re-attached on launch/save/template.
  prompt = '';
  private headerPart = '';

  parameters: AgentParam[] = [];
  paramValues: { [name: string]: string } = {};

  isNormalizing = false;
  isLaunching = false;

  /**
   * Dove far lavorare l'agente in QUESTO lancio.
   *
   * Il default lo dà l'impostazione del progetto, ma i due gesti sono diversi: «lancia e guarda
   * cosa fa» su un ritocco vuole il progetto, con il risultato lì sotto gli occhi; un lavoro
   * vero vuole l'isolamento, perché nel progetto finirebbe mescolato al tuo sul tuo ramo — e
   * quando lo committi lo firmi tu.
   */
  useWorktree = false;
  /** Senza git non ci sono rami né posti di lavoro: la spunta non ha nulla da offrire. */
  canIsolate = false;

  /**
   * Il motore di QUESTO lancio (sprint 2026-09-29-Motore-LLM-Unico, D3/D12): parte dal runtime: della
   * scheda, altrimenti dal motore del progetto (quello del tab MarkAgent); la scelta fatta qui vince.
   */
  readonly engines = [
    { id: 'claude', label: 'Claude Code' },
    { id: 'copilot', label: 'Copilot' },
    { id: 'opencode', label: 'opencode' },
  ];
  engineInfo: AgentEngineInfo | null = null;
  engineChoice: string | null = null;
  modelText = '';
  /** I modelli del motore scelto, gli stessi della tendina di MarkAgent. Un campo, non un getter: sta in un *ngFor. */
  models: { id: string; name: string }[] = [];
  modelsLoading = false;
  /** Perché l'elenco non c'è: «nessun modello» e «non riesco a leggerli» non sono la stessa cosa. */
  modelsError: string | null = null;
  private modelsRequest = 0;
  aiError: string | null = null;

  // Splits a normalized prompt at its `## Task` heading. Header = everything before it
  // (title + params block); task = the body the user actually edits. When there is no
  // `## Task` (e.g. still free text), the whole thing is the editable body.
  private splitNormalized(full: string): { header: string; task: string } {
    const m = (full || '').match(/^([\s\S]*?)\r?\n#{1,6}[ \t]*Task[ \t]*\r?\n+([\s\S]*)$/i);
    if (m) return { header: m[1].replace(/\s+$/, ''), task: m[2].trim() };
    return { header: '', task: (full || '').trim() };
  }

  // Re-attaches the hidden header to the edited task body → the full normalized prompt
  // used for launching, saving the draft, and writing the shared template.
  private composeFull(): string {
    const task = (this.prompt || '').trim();
    if (!this.headerPart) return task;
    return this.headerPart.replace(/\s+$/, '') + '\n\n## Task\n\n' + task + '\n';
  }

  // Stores a full normalized prompt as (hidden header, visible task body).
  private applyNormalized(full: string): void {
    const { header, task } = this.splitNormalized(full);
    this.headerPart = header;
    this.prompt = task;
  }

  ngOnInit(): void {
    this.loadQueue();
    this.loadIsolationDefault();
    this.loadEngine();
  }

  private loadEngine(): void {
    this.agentLaunchService.engineInfo(this.data.projectPath, this.data.agentFilePath).subscribe({
      next: (info) => {
        this.engineInfo = info;
        this.engineChoice = info.card?.engine || info.project?.engine || null;
        this.modelText = this.defaultModelFor(this.engineChoice);
        this.loadModels(this.engineChoice);
      },
      // Senza l'informazione il lancio non passa un motore: decide il servizio (scheda, poi progetto).
      error: () => { this.engineInfo = null; this.engineChoice = null; },
    });
  }

  /** Il modello da cui parte un motore: quello della scheda se è il suo, quello del progetto se è il suo. */
  private defaultModelFor(engine: string | null): string {
    if (!engine || !this.engineInfo) return '';
    if (this.engineInfo.card?.engine === engine && this.engineInfo.card.model) return this.engineInfo.card.model;
    if (this.engineInfo.project?.engine === engine && this.engineInfo.project.model) return this.engineInfo.project.model;
    return '';
  }

  chooseEngine(engine: string): void {
    this.engineChoice = engine;
    this.modelText = this.defaultModelFor(engine);
    this.loadModels(engine);
  }

  /**
   * I modelli del motore: prima quelli salvati (istantaneo); se non ce ne sono ancora li si chiede al motore, come fa
   * MarkAgent. Una risposta arrivata dopo un cambio di motore si scarta.
   */
  private loadModels(engine: string | null): void {
    const request = ++this.modelsRequest;
    this.models = [];
    this.modelsError = null;
    if (!engine) { this.modelsLoading = false; return; }
    const cached = (): Observable<{ id: string; name: string }[]> => engine === 'claude' ? this.aiChat.getClaudeCodeChatModels()
      : engine === 'copilot' ? this.aiChat.getCopilotChatModels()
      : this.aiChat.getOpenCodeChatModels();
    const refresh = (): Observable<unknown> => engine === 'claude' ? this.aiChat.refreshClaudeCodeModels()
      : engine === 'copilot' ? this.aiChat.refreshCopilotCliModels()
      : this.aiChat.refreshOpenCodeModels();
    this.modelsLoading = true;
    cached().pipe(
      switchMap(list => list.length ? of(list) : refresh().pipe(switchMap(() => cached()))),
      finalize(() => { if (request === this.modelsRequest) this.modelsLoading = false; }),
    ).subscribe({
      next: list => {
        if (request !== this.modelsRequest) return;
        this.models = list.map(m => ({ id: m.id, name: m.name || m.id }));
        if (!this.models.length)
          this.modelsError = this.translate.instant('AGENT_LAUNCH.MODELS_NONE');
      },
      error: err => {
        if (request !== this.modelsRequest) return;
        this.modelsError = this.translate.instant('AGENT_LAUNCH.MODELS_ERROR',
          { error: err?.error?.error || err?.error?.message || err?.message || err });
      },
    });
  }

  /** Il modello di partenza (scheda o progetto) che il motore non elenca: si vede, con l'avviso, invece di sparire. */
  get modelNotListed(): boolean {
    return !!this.modelText && !this.modelsLoading && this.models.length > 0
      && !this.models.some(m => m.id === this.modelText);
  }

  /** Da dove viene il motore scelto, per la riga sotto il selettore. */
  get engineSource(): string {
    if (!this.engineChoice) return this.translate.instant('AGENT_LAUNCH.ENGINE_NONE');
    if (this.engineInfo?.card?.engine === this.engineChoice) return this.translate.instant('AGENT_LAUNCH.ENGINE_FROM_CARD');
    if (!this.engineInfo?.card?.engine && this.engineInfo?.project?.engine === this.engineChoice)
      return this.translate.instant('AGENT_LAUNCH.ENGINE_FROM_PROJECT');
    return this.translate.instant('AGENT_LAUNCH.ENGINE_CHOSEN');
  }

  /**
   * Il default della spunta viene dall'impostazione del progetto: la spunta decide QUESTO
   * lancio, l'impostazione dice come si lavora di solito.
   */
  private loadIsolationDefault(): void {
    this.projectSettingsService.getAgentWorktreesSetting(this.data.projectPath).subscribe({
      next: (res) => {
        this.useWorktree = !!res?.enabled;
        // Il default del backend è vero solo su un repo con un origin: se lì è falso e nessuno
        // ha scelto, l'isolamento non è nemmeno possibile e la spunta mentirebbe.
        this.canIsolate = !!res?.defaultValue || !!res?.enabled;
      },
      // Best-effort: se l'impostazione non si legge, si lancia dove si è sempre lanciato.
      error: () => { this.useWorktree = false; this.canIsolate = false; },
    });
  }

  loadQueue(): void {
    this.queueLoading = true;
    this.agentQueueService.queue(this.data.agentName, this.data.projectPath).subscribe({
      next: (q) => { this.queue = q; this.queueLoading = false; },
      error: () => { this.queueLoading = false; /* best-effort: la coda non deve rompere il lancio */ },
    });
  }

  get queueCount(): number {
    return (this.queue?.messages?.length || 0) + (this.queue?.federatedPending?.length || 0);
  }

  forceQueued(id: string): void {
    this.agentQueueService.force(id).subscribe({ next: () => this.loadQueue(), error: () => this.loadQueue() });
  }

  discardQueued(id: string): void {
    this.agentQueueService.discard(id).subscribe({ next: () => this.loadQueue(), error: () => this.loadQueue() });
  }

  constructor(
    public dialogRef: MatDialogRef<AgentLaunchDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: AgentLaunchDialogData,
    private agentLaunchService: AgentLaunchService,
    private startGuard: AgentStartGuardService,
    private projectSettingsService: ProjectSettingsService,
    private agentScheduleService: AgentScheduleService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private translate: TranslateService,
    private agentQueueService: AgentQueueService,
    private aiChat: AiChatService,
    private mailbox: MailboxService,
  ) {
    // Un incarico ricevuto non ha bozza né modello: il testo è quello dell'altro agente, il campo è per le indicazioni.
    if (data.incoming) return;
    // Precedence: the per-user local draft (UserDB) wins; if there is none, seed from
    // the shared template stored inside the .agent.md (travels with git).
    this.agentScheduleService.getDraft(data.projectPath, data.agentFilePath).subscribe({
      next: (r) => {
        if (r.draft?.prompt && !this.prompt) {
          this.applyNormalized(r.draft.prompt);
          try {
            this.paramValues = JSON.parse(r.draft.parameterValuesJson || '{}') || {};
          } catch {
            this.paramValues = {};
          }
          this.detectParams();
        } else if (!this.prompt) {
          this.seedFromTemplate();
        }
      },
      error: () => { this.seedFromTemplate(); },
    });
  }

  /** Seeds the dialog from the shared template in the .agent.md when no local draft exists. */
  private seedFromTemplate(): void {
    this.agentScheduleService.getTemplate(this.data.agentFilePath).subscribe({
      next: (r) => {
        if (r.template && !this.prompt) {
          this.applyNormalized(r.template);
          this.detectParams();
        }
      },
      error: () => { /* template is optional */ },
    });
  }

  private saveDraft(): void {
    if (!this.prompt || !this.prompt.trim()) return;
    this.agentScheduleService
      .saveDraft(this.data.projectPath, this.data.agentFilePath, this.composeFull(), this.paramValues)
      .subscribe({ error: (err) => console.warn('Draft save failed:', err) });
  }

  /**
   * "Save only for me": persists prompt + parameter values as a per-user local draft
   * (UserDB, AppData) and closes. Nothing is written to the shared .agent.md.
   */
  saveLocal(): void {
    if (!this.prompt || !this.prompt.trim()) return;
    this.agentScheduleService
      .saveDraft(this.data.projectPath, this.data.agentFilePath, this.composeFull(), this.paramValues)
      .subscribe({
        next: () => {
          this.snackBar.open(this.translate.instant('AGENT_LAUNCH.SAVED_LOCAL'), undefined, { duration: 3000 });
          this.dialogRef.close(null);
        },
        error: (err) => {
          this.aiError = err?.error?.error || this.translate.instant('AGENT_LAUNCH.LAUNCH_ERROR');
          console.warn('Draft save failed:', err);
        },
      });
  }

  /**
   * "Save as template (shared)": writes the prompt into the managed section at the end
   * of the .agent.md (goes to git, seen by everyone). Parameter values stay local — they
   * are machine-specific — so they are also saved as a local draft here for convenience.
   */
  saveAsTemplate(): void {
    if (!this.prompt || !this.prompt.trim()) return;
    this.aiError = null;
    this.agentScheduleService.saveTemplate(this.data.agentFilePath, this.composeFull()).subscribe({
      next: () => {
        // Keep the machine-specific parameter values as a local draft.
        this.agentScheduleService
          .saveDraft(this.data.projectPath, this.data.agentFilePath, this.composeFull(), this.paramValues)
          .subscribe({ error: (err) => console.warn('Draft save failed:', err) });
        this.snackBar.open(this.translate.instant('AGENT_LAUNCH.SAVED_TEMPLATE'), undefined, { duration: 3000 });
        this.dialogRef.close(null);
      },
      error: (err) => {
        this.aiError = err?.error?.error || this.translate.instant('AGENT_LAUNCH.LAUNCH_ERROR');
        console.warn('Template save failed:', err);
      },
    });
  }

  /** Substitutes the chosen values and opens the scheduling dialog with the ready prompt. */
  saveAsSchedule(): void {
    if (!this.canLaunch()) return;
    this.aiError = null;
    this.saveDraft();
    this.agentLaunchService.prepare(this.composeFull(), this.paramValues).subscribe({
      next: (r) => {
        if (!r.success || !r.preparedPrompt) {
          this.aiError = r.error || this.translate.instant('AGENT_LAUNCH.LAUNCH_ERROR');
          return;
        }
        this.dialogRef.close(null);
        this.dialog.open(AgentScheduleDialogComponent, {
          width: '760px',
          data: {
            projectPath: this.data.projectPath,
            agentFilePath: this.data.agentFilePath,
            agentName: this.data.agentName,
            preparedPrompt: r.preparedPrompt,
          },
        });
      },
      error: (err) => {
        this.aiError = err?.error?.error || this.translate.instant('AGENT_LAUNCH.LAUNCH_ERROR');
      },
    });
  }

  /**
   * «Normalize»: MarkAgent rewrites the prompt following the mde-prompt-for-agents skill, in the MarkAgent tab's own
   * session (a channel of the AI chat, as the AI commit message): the project's engine, whatever it is (sprint
   * 2026-09-29-Motore-LLM-Unico, F3). The server builds the prompt and cleans the answer.
   */
  async normalize(): Promise<void> {
    if (!this.prompt || !this.prompt.trim()) {
      return;
    }
    this.isNormalizing = true;
    this.aiError = null;
    try {
      const built = await firstValueFrom(this.agentLaunchService.normalizePrompt(this.data.projectPath, this.composeFull()));
      if (!built.success || !built.prompt) {
        this.aiError = built.error || this.translate.instant('AGENT_LAUNCH.NORMALIZE_ERROR');
        return;
      }
      const raw = await this.aiChat.askOnChannel(`agent-normalize-${Date.now()}`, built.prompt);
      const response = await firstValueFrom(this.agentLaunchService.normalizeClean(raw));
      if (response.success && response.normalizedPrompt) {
        this.applyNormalized(response.normalizedPrompt);
        this.setParameters(response.parameters || []);
        this.saveDraft();
      } else {
        this.aiError = response.error || this.translate.instant('AGENT_LAUNCH.NORMALIZE_ERROR');
      }
    } catch (err: any) {
      this.aiError = err?.error?.error || err?.message || this.translate.instant('AGENT_LAUNCH.NORMALIZE_ERROR');
      console.error('Error normalizing agent prompt:', err);
    } finally {
      this.isNormalizing = false;
    }
  }

  /** Re-detects parameters after manual edits to the prompt. */
  detectParams(): void {
    if (!this.prompt || !this.prompt.trim()) {
      this.setParameters([]);
      return;
    }
    this.agentLaunchService.extractParams(this.composeFull()).subscribe({
      next: (response) => this.setParameters(response.parameters || []),
      error: (err) => console.error('Error extracting agent params:', err),
    });
  }

  async openPicker(param: AgentParam): Promise<void> {
    const picker = param.picker;
    const data = new ShowFileMetadata();
    // 'root' = current project root; HTTP-backed listing, works in browser and Electron.
    data.start = 'root';

    if (picker === 'out-file') {
      data.title = this.translate.instant('AGENT_LAUNCH.PICK_OUT_FILE');
      data.typeOfSelection = 'Folders';
      data.saveAs = true;
      data.buttonText = this.translate.instant('COMMON.SAVE');
      const current = this.paramValues[param.name] || param.defaultValue || '';
      const m = current.match(/[\\/]([^\\/]+)$/);
      data.defaultFileName = m ? m[1] : current;
    } else if (picker === 'dir') {
      data.title = this.translate.instant('AGENT_LAUNCH.PICK_FOLDER');
      data.typeOfSelection = 'Folders';
      data.buttonText = this.translate.instant('COMMON.SELECT');
    } else {
      data.title = this.translate.instant('AGENT_LAUNCH.PICK_FILE');
      data.typeOfSelection = 'FoldersAndFiles';
      data.buttonText = this.translate.instant('COMMON.SELECT');
    }

    const ref = this.dialog.open(ShowFileSystemComponent, {
      width: '900px',
      height: '700px',
      data,
    });
    const result = await firstValueFrom(ref.afterClosed());
    if (result?.data) {
      this.paramValues[param.name] = result.data;
    }
  }

  launchNow(): void {
    if (this.data.incoming) {
      this.startIncoming();
      return;
    }
    if (!this.canLaunch()) {
      return;
    }
    this.isLaunching = true;
    this.aiError = null;

    // In un posto di lavoro isolato l'agente parte da ciò che è pubblicato: se nella cartella c'è lavoro non
    // salvato lo si dice prima, con la possibilità di committare e pubblicare. Nel progetto (non isolato) vede tutto.
    const ready = this.useWorktree === false
      ? of(true)
      : this.startGuard.beforeStart(this.data.projectPath, this.data.agentName);

    ready.pipe(
      switchMap(go => go
        ? this.agentLaunchService.launch(this.data.projectPath, this.data.agentFilePath, this.composeFull(), this.paramValues,
            this.useWorktree, this.engineChoice || undefined, this.modelText.trim() || undefined)
        : EMPTY),
      // Annullato: nessuna risposta arriva, e il pulsante deve tornare premibile.
      finalize(() => { this.isLaunching = false; }),
    )
      .subscribe({
        next: (response) => {
          this.isLaunching = false;
          if (response.success) {
            this.saveDraft();
            this.snackBar.open(
              this.translate.instant('AGENT_LAUNCH.STARTED', { agent: this.data.agentName }),
              undefined,
              { duration: 4000 },
            );
            this.dialogRef.close({ launched: true, runId: response.runId });
          } else {
            this.aiError = response.error || this.translate.instant('AGENT_LAUNCH.LAUNCH_ERROR');
          }
        },
        error: (err) => {
          this.isLaunching = false;
          // 409 = already running, 400 = validation — both carry {error} in the body.
          this.aiError = err?.error?.error || this.translate.instant('AGENT_LAUNCH.LAUNCH_ERROR');
          console.error('Error launching agent:', err);
        },
      });
  }

  /**
   * Avvia l'incarico ricevuto: torna in coda con le indicazioni, il motore e il modello scelti qui, e l'agente parte nel
   * suo posto di lavoro come per ogni incarico. Prima si dice se nella cartella c'è lavoro che l'agente non vedrebbe.
   */
  private startIncoming(): void {
    const incoming = this.data.incoming!;
    if (this.isLaunching) return;
    this.isLaunching = true;
    this.aiError = null;
    this.startGuard.beforeStart(this.data.projectPath, this.data.agentName).pipe(
      switchMap(go => go
        ? this.mailbox.startAssignment(incoming.messageId, {
            note: this.prompt.trim() || undefined,
            provider: this.engineChoice || undefined,
            model: this.modelText.trim() || undefined,
          })
        : EMPTY),
      finalize(() => { this.isLaunching = false; }),
    ).subscribe({
      next: () => {
        this.snackBar.open(this.translate.instant('AGENT_LAUNCH.INCOMING_STARTED', { agent: this.data.agentName }), undefined, { duration: 4000 });
        this.dialogRef.close({ started: true });
      },
      error: (err) => {
        this.aiError = err?.error?.error || this.translate.instant('AGENT_LAUNCH.LAUNCH_ERROR');
      },
    });
  }

  /** Saving (local or template) only needs a prompt — parameter values are optional. */
  canSave(): boolean {
    return !!this.prompt && !!this.prompt.trim() && !this.isNormalizing && !this.isLaunching;
  }

  canLaunch(): boolean {
    if (this.data.incoming) return !this.isLaunching;
    if (!this.prompt || !this.prompt.trim() || this.isNormalizing || this.isLaunching) {
      return false;
    }
    return this.parameters.every((p) => (this.paramValues[p.name] || '').trim().length > 0);
  }

  onCancel(): void {
    this.dialogRef.close(null);
  }

  private setParameters(params: AgentParam[]): void {
    this.parameters = params;
    const previous = this.paramValues;
    this.paramValues = {};
    for (const p of params) {
      this.paramValues[p.name] = previous[p.name] || p.defaultValue || '';
    }
  }
}
