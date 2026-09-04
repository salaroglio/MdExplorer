import { Injectable, Injector } from '@angular/core';
import * as signalR from "@microsoft/signalr";
import { GITService } from '../../git/services/gitservice.service';
import { ConnectionLostProvider } from '../../signalR/dialogs/connection-lost/connection-lost.provider';
import { ParsingProjectProvider } from '../../signalR/dialogs/parsing-project/parsing-project.provider';
import { PlantumlWorkingProvider } from '../../signalR/dialogs/plantuml-working/plantuml-working.provider';
import { connect } from 'net';
import { OpeningApplicationProvider } from '../dialogs/opening-application/opening-application.provider';
import { Subject, ReplaySubject } from 'rxjs';

interface linkSignalREvent_Component {
  key: string
  object: any;
  callback: (data: any, objectThis: any) => any
}

@Injectable({
  providedIn: 'root'
})
export class MdServerMessagesService {

  linkEventCompArray: linkSignalREvent_Component[];
  public connectionId: string;

  /** Emits the connectionId once available (and on every reconnection). ReplaySubject(1) so late subscribers get the last value immediately. */
  public connectionId$ = new ReplaySubject<string>(1);

  // Observable for Git branch switch events.
  // changedFiles: repo-relative paths ('/' separators) of the operation's commit diff —
  // used to reload the open document, NOT to decide whether to refresh the tree.
  public gitBranchSwitched$ = new Subject<{ fileCount: number, changedFiles?: string[], message: string }>();

  // Observable for Git pull refresh events (same payload contract as gitBranchSwitched$)
  public gitPullRefreshed$ = new Subject<{ fileCount: number, changedFiles?: string[], message: string }>();

  // Observable for RAG indexing progress events
  public ragIndexingProgress$ = new Subject<{ status: string, processed: number, total: number, message: string }>();

  // Observable for App Store publish progress (backend → Nexus upload)
  public publishProgress$ = new Subject<{ appId: string, percent: number, phase: string }>();

  // Observable for the Mark folder-summarizer job progress (MarkActionsController)
  public markFolderProgress$ = new Subject<any>();

  // Observable for the "Ask to MarkAgent" diagram explanation stream
  // (MarkDiagramController). Phases: start | chunk | done | error.
  public markDiagramExplain$ = new Subject<{
    phase: 'start' | 'status' | 'chunk' | 'done' | 'error',
    box?: string,
    text?: string,
    sentences?: number,
    message?: string,
    followUp?: boolean,
  }>();

  // Observable for *.agent.md headless runs (AgentRunJobService): started/completed/failed
  public agentJobProgress$ = new Subject<{
    runId: string,
    scheduleId?: string,
    agentName: string,
    agentFilePath: string,
    triggerSource: string,
    phase: 'started' | 'completed' | 'failed' | 'cancelled',
    error?: string,
    outputTail?: string
  }>();

  // Observable for agent→user mailbox messages (§13 Fase 4a). Emitted by
  // AgentMessageDispatcher when a citizen escalates to the human: drives the toast
  // + the unread badge on the toolbar bell.
  public agentMessageReceived$ = new Subject<{
    conversationId: string,
    messageId: string,
    fromAgent: string,
    projectPath: string,
    bodyPreview: string,
    topics?: string[],
    createdAt: string
  }>();

  // Observable for federated intervention requests (§12.6): another city asks an agent
  // of THIS one to act — needs the human gate (approve/reject) before any run.
  public federationRequestReceived$ = new Subject<{
    id: string,
    federationId: string,
    projectPath: string,
    fromOwner: string,
    fromAgent: string,
    targetAgent: string,
    scope: string,
    createdAt: string
  }>();

  // Delega interna: un agente ha instradato lavoro su un ambito il cui responsabile e'
  // l'umano locale. Non e' un permesso da concedere (il gate custodisce la fiducia fra umani
  // diversi): e' consapevolezza che la mappa di ownership e' stata esercitata.
  public agentDelegationRouted$ = new Subject<{
    projectPath: string,
    scope: string,
    fromAgent: string,
    toAgent: string,
    conversationId: string
  }>();

  // Un agente ha consegnato e CHIEDE di fondere: il tab di revisione si accende.
  public agentMergeRequested$ = new Subject<{
    projectPath: string,
    agentName: string,
    branch: string,
    files: number
  }>();

  // Fase 7e — un agente ha toccato il submodule (codice) nel suo worktree: awareness (no diff).
  public submoduleTouchedByAgent$ = new Subject<{
    projectPath: string,
    submodule: string,
    agent: string,
    at: string
  }>();

  // Observable for KG drift events (emitted by FileSystemWatcherManager when a .md
  // diverges from the // sourceDocHash header of its adjacent .kg.cypher).
  public kgStale$ = new Subject<{
    sourceMdPath: string,
    kgFilePath: string,
    storedSourceDocHash: string,
    currentSourceDocHash: string,
    reason: 'header-missing' | 'hash-mismatch'
  }>();

  // I submodule del progetto: popolati all'apertura, non solo su clone e pull.
  // Il fallimento arriva qui perché prima finiva appeso al messaggio di successo del clone —
  // il clone risultava riuscito, la cartella del codice restava vuota e nessuno lo leggeva.
  public submoduleInit$ = new Subject<{
    phase: 'started' | 'completed' | 'failed',
    projectPath: string,
    submodules: string[],
    error?: string
  }>();

  // Observable for Screenshot Annotation Wizard (from iframe Ctrl+V)
  public screenshotAnnotationRequest$ = new Subject<{
    success: boolean,
    imageBase64?: string,
    mimeType?: string,
    documentPath?: string,
    errorMessage?: string,
    platformHint?: string
  }>();

  // Observable streams for runnable fenced code blocks (MdExecutionController)
  public executionOutput$ = new Subject<{ blockId: string, stream: string, chunk: string }>();
  public executionCompleted$ = new Subject<{ blockId: string, exitCode: number, durationMs: number, timedOut: boolean }>();
  public executionError$ = new Subject<{ blockId: string, message: string }>();

  // Observable streams for long-running "services" (MdServicesController)
  public serviceStarted$ = new Subject<any>();
  public serviceOutput$ = new Subject<{ serviceId: string, blockId: string, stream: string, chunk: string }>();
  public serviceStopped$ = new Subject<any>();

  // ── File/folder/indexing events (md-tree & co.) ──
  // Each SignalR event below is registered EXACTLY ONCE on the hub connection
  // (in startConnection) and fanned out through these Subjects. Components must
  // subscribe with takeUntil(destroy$). The legacy add*Listener wrappers used to
  // call hubConnection.on() on every invocation: SignalR ACCUMULATES handlers,
  // so every component re-creation (e.g. leaving/re-entering a project) added a
  // duplicate handler bound to a dead component — events got processed N times.
  public markdownFileChanged$ = new Subject<any>();          // 'markdownfileischanged'
  public markdownFileCreated$ = new Subject<any>();          // 'markdownFileCreated'
  public markdownFileDeleted$ = new Subject<any>();          // 'markdownFileDeleted'
  public folderCreated$ = new Subject<any>();                // 'folderCreated'
  public folderDeleted$ = new Subject<any>();                // 'folderDeleted'
  public folderRenamed$ = new Subject<any>();                // 'folderRenamed'
  public fileSystemStorm$ = new Subject<any[]>();            // 'fileSystemStorm'
  public fileIndexed$ = new Subject<any>();                  // 'fileIndexed'
  public folderIndexingStart$ = new Subject<any>();          // 'folderIndexingStart'
  public folderIndexingComplete$ = new Subject<any>();       // 'folderIndexingComplete'
  public parsingProjectStart$ = new Subject<any>();          // 'parsingProjectStart'
  public parsingProjectStop$ = new Subject<any>();           // 'parsingProjectStop'
  public knowledgeProgress$ = new Subject<any>();            // 'knowledgeProgress'
  // FSW buffer overflow / internal error: events were LOST server-side.
  // Consumers must do a full reload — incremental state can no longer be trusted.
  public fileSystemWatcherError$ = new Subject<{ reason: string, message?: string }>();

  constructor(
    private parsingProjectProvider: ParsingProjectProvider,
    private plantumlWorkingProvider: PlantumlWorkingProvider,
    private connectionLostProvider: ConnectionLostProvider,
    private openingApplicationProvider: OpeningApplicationProvider,
    private gitService: GITService,
    private injector: Injector) {
    this.startConnection();
    console.log('MonitorMDService constructor');
    this.linkEventCompArray = [];

  }

  private hubConnection: signalR.HubConnection
  private rule1IsRegistered: any;
  private connectionIsLost: boolean = false;
  private consoleIsClosed: boolean = false;
  

  public startConnection = () => {
    if (this.hubConnection == null) {
      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl('../signalr/monitormd')
        .build();
      this.hubConnection.on('markdownfileisprocessed', (data) => {
        this.processCallBack(data, 'markdownfileisprocessed');
      });
      this.hubConnection.on('yamlAutoGenerated', (data) => {
        this.processCallBack(data, 'yamlAutoGenerated');
      });
      this.hubConnection.on('documentNavigated', (data) => {
        this.processCallBack(data, 'documentNavigated');
      });
      // parsingProjectStart/Stop NON aprono più la MatDialog modale "Building knowledge"
      // (era il blocker UX: backdrop modale → utente inibito per tutta l'indicizzazione).
      // Dopo il refactor del 2026-05-23 la pipeline è davvero async (Task.Yield in
      // IndexingPipelineService.RunAsync) e mostriamo il progresso con una snackbar
      // custom (IndexingProgressSnackComponent) montata da md-tree.component.ts via
      // addParsingProjectStartListener (sotto).
      this.hubConnection.on('openingApplication', (data) => {
        this.openingApplicationProvider.show(data);
      });
      this.hubConnection.on('plantumlWorkStart', (data) => {
        this.plantumlWorkingProvider.show(data);
      });
      this.hubConnection.on('plantumlWorkStop', (data) => {
        this.plantumlWorkingProvider.hide(data);
      });
      this.hubConnection.on('indexingFolder', (folder) => {
        this.parsingProjectProvider.folder$.next(folder);
      });

      // Git branch switch event (client-specific, from ModernGitController)
      this.hubConnection.on('gitBranchSwitched', (data) => {
        console.log('✅ SignalR event received: gitBranchSwitched', data);
        this.gitBranchSwitched$.next(data);
      });

      // Git pull refresh event (from ModernGitToolbarController)
      this.hubConnection.on('gitPullRefreshed', (data) => {
        console.log('✅ SignalR event received: gitPullRefreshed', data);
        this.gitPullRefreshed$.next(data);
      });

      // Screenshot Annotation Wizard event (from iframe Ctrl+V via backend clipboard read)
      this.hubConnection.on('openScreenshotAnnotationWizard', (data) => {
        console.log('📷 SignalR event received: openScreenshotAnnotationWizard', data);
        this.screenshotAnnotationRequest$.next(data);
      });

      // RAG indexing progress event (from RagController background task)
      this.hubConnection.on('ragIndexingProgress', (data) => {
        this.ragIndexingProgress$.next(data);
      });

      // App Store publish progress (backend → Nexus upload)
      this.hubConnection.on('publishProgress', (data) => {
        this.publishProgress$.next(data);
      });

      // Mark folder-summarizer job progress
      this.hubConnection.on('markFolderProgress', (data) => {
        this.markFolderProgress$.next(data);
      });

      // "Ask to MarkAgent" diagram explanation, streamed chunk by chunk
      this.hubConnection.on('markDiagramExplain', (data) => {
        this.markDiagramExplain$.next(data);
      });

      // *.agent.md headless run progress (manual launch, schedule, hook)
      this.hubConnection.on('agentJobProgress', (data) => {
        this.agentJobProgress$.next(data);
      });

      // Agent→user mailbox message (§13 Fase 4a): a citizen escalated to the human
      this.hubConnection.on('agentMessageReceived', (data) => {
        console.log('🔔 SignalR event received: agentMessageReceived', data);
        this.agentMessageReceived$.next(data);
      });

      // Federated intervention request (§12.6): needs the human gate
      this.hubConnection.on('federationRequestReceived', (data) => {
        console.log('🌐 SignalR event received: federationRequestReceived', data);
        this.federationRequestReceived$.next(data);
      });

      // Delega interna instradata dalla mappa di ownership, rimasta in locale.
      this.hubConnection.on('agentDelegationRouted', (data) => {
        console.log('🔀 SignalR event received: agentDelegationRouted', data);
        this.agentDelegationRouted$.next(data);
      });

      // Richiesta di merge aperta da un agente: la revisione ha qualcosa da mostrare.
      this.hubConnection.on('agentMergeRequested', (data) => {
        console.log('🔀 SignalR event received: agentMergeRequested', data);
        this.agentMergeRequested$.next(data);
      });

      // Fase 7e — awareness del tocco submodule da parte di un agente (gate del push umano).
      this.hubConnection.on('submoduleTouchedByAgent', (data) => {
        console.log('🧩 SignalR event received: submoduleTouchedByAgent', data);
        this.submoduleTouchedByAgent$.next(data);
      });

      // KG drift detection — .md edited but .kg.cypher is out of sync
      this.hubConnection.on('kgStale', (data) => {
        console.warn('⚠️ SignalR event received: kgStale', data);
        this.kgStale$.next(data);
      });

      // Submodule del progetto (apertura, clone, pull)
      this.hubConnection.on('submoduleInitStarted', (d) => {
        this.submoduleInit$.next({ phase: 'started', projectPath: d?.projectPath, submodules: d?.submodules || [] });
      });
      this.hubConnection.on('submoduleInitCompleted', (d) => {
        this.submoduleInit$.next({ phase: 'completed', projectPath: d?.projectPath, submodules: d?.submodules || [] });
      });
      this.hubConnection.on('submoduleInitFailed', (d) => {
        console.error('❌ SignalR: submoduleInitFailed', d);
        this.submoduleInit$.next({ phase: 'failed', projectPath: d?.projectPath, submodules: d?.submodules || [], error: d?.error });
      });

      // Runnable fenced code blocks — streaming output from MdExecutionController
      this.hubConnection.on('execution.output', (data) => {
        this.executionOutput$.next(data);
      });
      this.hubConnection.on('execution.completed', (data) => {
        this.executionCompleted$.next(data);
      });
      this.hubConnection.on('execution.error', (data) => {
        this.executionError$.next(data);
      });

      // Long-running services — lifecycle + streaming output from MdServicesController
      this.hubConnection.on('service.started', (data) => {
        this.serviceStarted$.next(data);
      });
      this.hubConnection.on('service.output', (data) => {
        this.serviceOutput$.next(data);
      });
      this.hubConnection.on('service.stopped', (data) => {
        this.serviceStopped$.next(data);
      });

      // File/folder/indexing events — registered ONCE here, fanned out via Subjects.
      // NEVER register these again elsewhere: SignalR accumulates handlers.
      this.hubConnection.on('markdownfileischanged', (data) => {
        this.markdownFileChanged$.next(data);
      });
      this.hubConnection.on('markdownFileCreated', (data) => {
        console.log('📄 [SignalR] markdownFileCreated:', data?.fullPath || data?.FullPath);
        this.markdownFileCreated$.next(data);
      });
      this.hubConnection.on('markdownFileDeleted', (data) => {
        console.log('🗑️ [SignalR] markdownFileDeleted:', data?.fullPath || data?.FullPath);
        this.markdownFileDeleted$.next(data);
      });
      this.hubConnection.on('folderCreated', (data) => {
        console.log('📁 [SignalR] folderCreated:', data?.fullPath || data?.FullPath);
        this.folderCreated$.next(data);
      });
      this.hubConnection.on('folderDeleted', (data) => {
        console.log('🗑️ [SignalR] folderDeleted:', data?.fullPath || data?.FullPath);
        this.folderDeleted$.next(data);
      });
      this.hubConnection.on('folderRenamed', (data) => {
        console.log('✏️ [SignalR] folderRenamed:', data?.oldFullPath || data?.OldFullPath, '→', data?.fullPath || data?.FullPath);
        this.folderRenamed$.next(data);
      });
      this.hubConnection.on('fileSystemStorm', (data: any[]) => {
        console.log(`⚡ [SignalR] fileSystemStorm: ${data?.length || 0} changes after storm`);
        this.fileSystemStorm$.next(data);
      });
      this.hubConnection.on('fileIndexed', (data) => {
        this.fileIndexed$.next(data);
      });
      this.hubConnection.on('folderIndexingStart', (data) => {
        this.folderIndexingStart$.next(data);
      });
      this.hubConnection.on('folderIndexingComplete', (data) => {
        this.folderIndexingComplete$.next(data);
      });
      this.hubConnection.on('parsingProjectStart', (data) => {
        this.parsingProjectStart$.next(data);
      });
      this.hubConnection.on('parsingProjectStop', (data) => {
        this.parsingProjectStop$.next(data);
      });
      this.hubConnection.on('knowledgeProgress', (data) => {
        this.knowledgeProgress$.next(data);
      });
      this.hubConnection.on('fileSystemWatcherError', (data) => {
        console.error('⚠️ [SignalR] fileSystemWatcherError — events were lost server-side:', data);
        this.fileSystemWatcherError$.next(data);
      });

      this.hubConnection.on('consoleClosed', (data) => {
        console.log('consoleClosed');
        this.consoleIsClosed = true;
        this.connectionLostProvider.showConsoleClosed();
      });
      this.hubConnection.onclose((data) => {
        console.warn('🔴 [DIAG] SignalR CLOSED at:', new Date().toISOString(), 'consoleIsClosed:', this.consoleIsClosed);
        if (!this.consoleIsClosed) {
          this.connectionLostProvider.show(this);
          this.connectionIsLost = true;
        }
      });

    }

    if (this.hubConnection.state == "Disconnected") {
      const wasReconnection = this.connectionIsLost; // Capture before reset
      this.hubConnection
        .start()
        .then(() => {
          console.log('Connection started');
          console.warn('🔴 [DIAG] SignalR STARTED. wasReconnection:', wasReconnection, 'at:', new Date().toISOString());
          this.connectionIsLost = false;
          this.getCurrentConnectionId(this, wasReconnection);
        }

        )
        .catch(err => {
          console.log('Error while starting connection: ' + err);
        }
        );
    }

    

  }




  public addRefactoringFileEvent(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.hubConnection.on('refactoringFileEvent', (data) => {
      callback(data, objectThis);
    });
  }


  /**
   * @deprecated Subscribe to markdownFileChanged$ with takeUntil(destroy$) instead.
   * This wrapper delegates to the Subject (hub handler registered once in
   * startConnection), but the callback itself is never unsubscribed: do NOT call
   * it from components that get destroyed/recreated.
   */
  public addMarkdownFileListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.markdownFileChanged$.subscribe((data) => {
      callback(data, objectThis);
    });
  }

  private processCallBack(data: any, signalREvent: string) {
    this.linkEventCompArray.forEach(_ => {
      if (_.key == signalREvent) {
        _.callback(data, _.object);
      }
    })
  }



  public addMdProcessedListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    let check = this.linkEventCompArray.find(_ =>
      _.key == 'markdownfileisprocessed' && _.object.constructor.name === objectThis.constructor.name);
    if (check == undefined) {
      this.linkEventCompArray.push({ key: 'markdownfileisprocessed', object: objectThis, callback: callback });
    }
  }

  public addMdRule1Listener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    // giusto per evitare di effettuare l'instanziazione un centinaio di volte l'evento
    console.log('addMdRule1Listener');
    if (this.rule1IsRegistered == undefined) {
      this.rule1IsRegistered = objectThis;
      this.hubConnection.on('markdownbreakrule1', (data) => {
        callback(data, objectThis);
      });
    }
  }

  public addPdfIsReadyListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.hubConnection.on('pdfisready', (data) => {
      callback(data, objectThis);
    });
  }

  public addYamlAutoGeneratedListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    let check = this.linkEventCompArray.find(_ =>
      _.key == 'yamlAutoGenerated' && _.object.constructor.name === objectThis.constructor.name);
    if (check == undefined) {
      this.linkEventCompArray.push({ key: 'yamlAutoGenerated', object: objectThis, callback: callback });
    }
  }

  public addDocumentNavigatedListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    let check = this.linkEventCompArray.find(_ =>
      _.key == 'documentNavigated' && _.object.constructor.name === objectThis.constructor.name);
    if (check == undefined) {
      this.linkEventCompArray.push({ key: 'documentNavigated', object: objectThis, callback: callback });
    }
  }

  // Listener per forzare l'aggiornamento di file rinominati (Rule #1 fix)
  private rule1ForceUpdateRegistered: any;
  public addRule1ForceUpdateListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    console.log('addRule1ForceUpdateListener');
    if (this.rule1ForceUpdateRegistered == undefined) {
      this.rule1ForceUpdateRegistered = objectThis;
      // Non abbiamo bisogno di un evento SignalR reale, useremo questo per il pattern locale
    }
  }
  
  // Metodo per triggerare l'evento di force update localmente
  public triggerRule1ForceUpdate(filePath: string): void {
    if (this.rule1ForceUpdateRegistered?.handleRule1ForceUpdate) {
      this.rule1ForceUpdateRegistered.handleRule1ForceUpdate(filePath);
    }
  }

  /** @deprecated Subscribe to fileIndexed$ with takeUntil(destroy$) instead. */
  public addFileIndexedListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.fileIndexed$.subscribe((data) => {
      callback(data, objectThis);
    });
  }

  /** @deprecated Subscribe to folderIndexingStart$ with takeUntil(destroy$) instead. */
  public addFolderIndexingStartListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.folderIndexingStart$.subscribe((data) => {
      callback(data, objectThis);
    });
  }

  /** @deprecated Subscribe to folderIndexingComplete$ with takeUntil(destroy$) instead. */
  public addFolderIndexingCompleteListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.folderIndexingComplete$.subscribe((data) => {
      callback(data, objectThis);
    });
  }

  /** @deprecated Subscribe to parsingProjectStart$ with takeUntil(destroy$) instead. */
  public addParsingProjectStartListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.parsingProjectStart$.subscribe((data) => {
      callback(data, objectThis);
    });
  }

  /** @deprecated Subscribe to markdownFileCreated$ with takeUntil(destroy$) instead. */
  public addMarkdownFileCreatedListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.markdownFileCreated$.subscribe((data) => {
      callback(data, objectThis);
    });
  }

  /** @deprecated Subscribe to markdownFileDeleted$ with takeUntil(destroy$) instead. */
  public addMarkdownFileDeletedListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.markdownFileDeleted$.subscribe((data) => {
      callback(data, objectThis);
    });
  }

  /** @deprecated Subscribe to parsingProjectStop$ with takeUntil(destroy$) instead. */
  public addParsingProjectStopListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.parsingProjectStop$.subscribe((data) => {
      callback(data, objectThis);
    });
  }

  /**
   * "Building knowledge" progress event — emesso da IndexingPipelineService
   * dopo ogni cartella nella fase ParseLinks. Payload:
   *   { processed: number, total: number, percent: 0..100 }
   * @deprecated Subscribe to knowledgeProgress$ with takeUntil(destroy$) instead.
   */
  public addKnowledgeProgressListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.knowledgeProgress$.subscribe((data) => {
      callback(data, objectThis);
    });
  }

  public addConnectionIdListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.hubConnection.on('getconnectionid', (data) => {
      callback(data, objectThis);
    });
  }

  public getConnectionId(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.hubConnection.invoke('GetConnectionId')
      .then(function (connectionId) {        
        objectThis.connectionId = connectionId;
        callback(connectionId, objectThis);
      });
  }

  public getCurrentConnectionId(objectThis: MdServerMessagesService, isReconnection: boolean = false): void {
    this.hubConnection.invoke('GetConnectionId')
      .then(function (connectionId) {
        objectThis.connectionId = connectionId;
        objectThis.connectionId$.next(connectionId);

        // Notify Electron that connectionId is ready (for URL handler feature)
        if ((window as any).electronAPI?.notifyConnectionIdReady) {
          console.log('[SignalR] Notifying Electron of connectionId:', connectionId);
          (window as any).electronAPI.notifyConnectionIdReady(connectionId);
        }

        // If this was a reconnection, re-register the current project with the new connectionId
        // This is necessary because when SignalR disconnects, the backend cleans up
        // FileSystemWatcher and DatabaseContext for the old connectionId
        if (isReconnection) {
          console.log('[SignalR] Reconnection detected, re-registering project...');
          objectThis.reregisterCurrentProject();
        }
      });
  }

  private reregisterCurrentProject(): void {
    // Use dynamic import to avoid circular dependency issues
    import('../../md-explorer/services/projects.service').then(module => {
      const projectsService = this.injector.get(module.ProjectsService);
      projectsService.reregisterCurrentProject();
    }).catch(err => {
      console.error('[SignalR] Failed to re-register project:', err);
    });
  }

  // TOC Generation listeners
  public addTocGenerationProgressListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.hubConnection.on('TocGenerationProgress', (data) => {
      console.log('[SignalR] TOC Generation Progress:', data);
      callback(data, objectThis);
    });
  }

  public addTocGenerationCompleteListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.hubConnection.on('TocGenerationComplete', (data) => {
      console.log('[SignalR] TOC Generation Complete:', data);
      callback(data, objectThis);
    });
  }

  // AI File Operation listener
  public addAiFileOperationListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.hubConnection.on('aiFileOperation', (data) => {
      console.log('[SignalR] AI File Operation:', data);
      callback(data, objectThis);
    });
  }

  // URL Handler listeners
  public addUrlHandlerOpenDocumentListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.hubConnection.on('urlHandlerOpenDocument', (data) => {
      console.log('[SignalR] URL Handler Open Document:', data);
      callback(data, objectThis);
    });
  }

  public addUrlHandlerOpenConfigProjectDialogListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    // Listen for new configproject event
    this.hubConnection.on('urlHandlerOpenConfigProjectDialog', (data) => {
      console.log('[SignalR] URL Handler Open ConfigProject Dialog:', data);
      callback(data, objectThis);
    });
    // Also listen for legacy clone event for backward compatibility
    this.hubConnection.on('urlHandlerOpenCloneDialog', (data) => {
      console.log('[SignalR] URL Handler Open Clone Dialog (legacy):', data);
      callback(data, objectThis);
    });
  }

  public addUrlHandlerErrorListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.hubConnection.on('urlHandlerError', (data) => {
      console.log('[SignalR] URL Handler Error:', data);
      callback(data, objectThis);
    });
  }

  // Bulk Export listeners
  public addBulkExportProgressListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.hubConnection.on('BulkExportProgress', (data) => {
      callback(data, objectThis);
    });
  }

  public addBulkExportCompleteListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.hubConnection.on('BulkExportComplete', (data) => {
      callback(data, objectThis);
    });
  }

  /** @deprecated Subscribe to folderCreated$ with takeUntil(destroy$) instead. */
  public addFolderCreatedListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.folderCreated$.subscribe((data) => {
      callback(data, objectThis);
    });
  }

  /** @deprecated Subscribe to folderDeleted$ with takeUntil(destroy$) instead. */
  public addFolderDeletedListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.folderDeleted$.subscribe((data) => {
      callback(data, objectThis);
    });
  }

  /** @deprecated Subscribe to folderRenamed$ with takeUntil(destroy$) instead. */
  public addFolderRenamedListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.folderRenamed$.subscribe((data) => {
      callback(data, objectThis);
    });
  }

  /** @deprecated Subscribe to fileSystemStorm$ with takeUntil(destroy$) instead. */
  public addFileSystemStormListener(callback: (data: any[], objectThis: any) => any, objectThis: any): void {
    this.fileSystemStorm$.subscribe((data) => {
      callback(data, objectThis);
    });
  }

}
