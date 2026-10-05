import { HttpClient } from '@angular/common/http';
import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { MAT_LEGACY_TOOLTIP_DEFAULT_OPTIONS } from '@angular/material/legacy-tooltip';
import { MatLegacyDialog as MatDialog } from '@angular/material/legacy-dialog';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { RenameFileComponent } from '../refactoring/rename-file/rename-file.component';
import { MdFileService } from '../../services/md-file.service';
import { FederationService, CityUser, ImpersonationStatus } from '../../services/federation.service';
import { RulesComponent } from '../../../signalR/dialogs/rules/rules.component';
import { MdFile } from '../../models/md-file';
import { GITService } from '../../../git/services/gitservice.service';
import { AppCurrentMetadataService } from '../../../services/app-current-metadata.service';
import { MatLegacyMenuTrigger as MatMenuTrigger } from '@angular/material/legacy-menu';
import { IBranch } from '../../../git/models/branch';
import { MatLegacyTabGroup as MatTabGroup } from '@angular/material/legacy-tabs';
import { ITag } from '../../../git/models/Tag';
import { ProjectsService } from '../../services/projects.service';
import { ReviewContextService } from '../../services/review-context.service';
import { ChangeKind, RepoActionResult, RepoChanges, SafePushResult, WorkingChange, WorkingChangesService, WorkingChangesView, UpstreamStatus } from '../../services/working-changes.service';
import { Router } from '@angular/router';
import { WaitingDialogService } from '../../../commons/waitingdialog/waiting-dialog.service';
import { WaitingDialogInfo } from '../../../commons/waitingdialog/waiting-dialog/models/WaitingDialogInfo';
import { GitMessagesComponent } from '../../../git/components/git-messages/git-messages.component';
import { CommitMessageDialogComponent } from '../../../git/dialogs/commit-message-dialog/commit-message-dialog.component';
import { AgentRegistryDialogComponent } from '../agent-registry-dialog/agent-registry-dialog.component';
import { AgentMemoryDialogComponent } from '../agent-memory-dialog/agent-memory-dialog.component';
import { AgentMailboxNotificationService } from '../../../services/agent-mailbox-notification.service';
import { AgentCityStateService } from '../../services/agent-city-state.service';
import { AgentActivityService, RunningAgent } from '../../services/agent-activity.service';
import { GitHistoryDialogComponent } from '../../../git/dialogs/git-history-dialog/git-history-dialog.component';
import { GitBranchDialogComponent } from '../../../git/dialogs/git-branch-dialog/git-branch-dialog.component';
import { GitDialogRepo } from '../../../git/components/git-repo-picker/git-repo-picker.component';
import { GitSetupRemoteGenericDialogComponent } from '../../../git/dialogs/git-setup-remote-generic-dialog/git-setup-remote-generic-dialog.component';
import { GitAddSubmoduleDialogComponent } from '../../../git/dialogs/git-add-submodule-dialog/git-add-submodule-dialog.component';
import { BookmarksService } from '../../services/bookmarks.service';
import { DocumentRefreshService } from '../../services/document-refresh.service';
import { MdServerMessagesService } from '../../../signalR/services/server-messages.service';
import { Bookmark } from '../../services/Types/Bookmark';
import { MdNavigationService } from '../../services/md-navigation.service';
import { Observable, Subscription, forkJoin } from 'rxjs';
import { map } from 'rxjs/operators';
import { FileNameAndAuthor } from '../../../git/models/DataToPull';
import { TocGenerationService } from '../../services/toc-generation.service';
import { TocProgressService } from '../../services/toc-progress.service';
import { ThemeService } from '../../../services/theme.service';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../commons/components/confirm-dialog/confirm-dialog.component';
import { TranslateService } from '@ngx-translate/core';
import _ from 'lodash';


/** I tre pannelli git della toolbar: stessa forma, una riga per repository. */
type GitPanel = 'commit' | 'pull' | 'push' | 'upstream';

@Component({
  selector: 'app-toolbar',
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.scss'],
  providers: [
    // Un tooltip di Material cattura il mouse: passandoci sopra il puntatore esce dall'area del
    // pannello git e il pannello si chiude prima di arrivare al pulsante (provato l'01/10/2026).
    // Nella toolbar i tooltip si leggono e basta: il mouse li attraversa.
    {
      provide: MAT_LEGACY_TOOLTIP_DEFAULT_OPTIONS,
      useValue: { showDelay: 0, hideDelay: 0, touchendHideDelay: 1500, disableTooltipInteractivity: true },
    },
  ],
})
export class ToolbarComponent implements OnInit, OnDestroy {
  // Esponiamo console per il template
  public console = console;

  public currentBranch: string;
  @ViewChild('hoverMenu', { static: false }) hoverMenuTrigger: MatMenuTrigger;
  @ViewChild('branchMenuTrigger', { static: false }) matMenuTrigger: MatMenuTrigger;
  @ViewChild(MatTabGroup, { static: false }) tabGroup: MatTabGroup;

  TitleToShow: string;
  absolutePath: string;
  relativePath: string;
  connectionId: string;
  somethingIsChangedInTheBranch: boolean;
  somethingIsToPull: boolean;
  somethingIsToPush: boolean;
  howManyFilesAreToCommit: number;
  howManyCommitAreToPush: number;
  /** Commit della radice da pubblicare, dai contatori di git (locali, poi dopo il fetch). */
  private rootCommitsToPush = 0;
  /** Commit dei submodule da pubblicare, dalla vista per repository. */
  private submoduleCommitsToPush = 0;
  howManyFilesAreToPull: number;
  branches: IBranch[];
  // Fase 7h: worktree degli agenti del progetto, per il sottomenu "Worktree".
  worktreeList: { agent: string; path: string }[] = [];
  // Impersonazione utente (test città): identità effettiva + lista padroni.
  identity: ImpersonationStatus | null = null;
  cityUsers: CityUser[] = [];
  /** Master switch del progetto: governa la visibilità dei comandi della città. */
  cityEnabled: boolean = false;
  private citySubscriptions = new Subscription();
  /** The viewer shows a slide deck: the toolbar offers its PDF export. */
  slideDeckShown = false;
  private slideDeckSubscription: Subscription;
  taglist: ITag[];
  currentMdFile: MdFile
  public connectionIsActive: boolean = true;
  public isCheckingConnection: boolean = false;
  public filesAndAuthors: FileNameAndAuthor[];
  subscriptionserverSelectedMdFile: Subscription;

  /**
   * Gli aggregati della finestrella, e di CHI sono.
   *
   * In revisione i numeri sono quelli dell'agente e il commit agisce nel suo posto di lavoro:
   * altrimenti guarderesti il lavoro di uno e committeresti quello di un altro. La lista dei
   * file non sta piu' qui — sta nel tab, dove si puo' vedere il diff prima di decidere.
   */
  public reviewAgent: string | null = null;
  public changesView: WorkingChangesView | null = null;
  /** I repository che hanno davvero qualcosa da committare: le righe del pannello. */
  public reposToCommit: RepoChanges[] = [];
  /** Quelli che hanno qualcosa da scaricare: commit sul remoto, o un submodule da allineare. */
  public reposToPull: RepoChanges[] = [];
  /** Quelli che hanno commit non ancora pubblicati. */
  public reposToPush: RepoChanges[] = [];
  /** Quelli il cui remoto non ha risposto all'ultima interrogazione. */
  public reposWithRemoteProblem: RepoChanges[] = [];
  public howManyAreToPull = 0;
  /** Il progetto stesso ha commit da scaricare: solo allora ha senso l'elenco di chi ha cambiato cosa. */
  public rootIsBehind = false;

  /**
   * I tre pannelli si aprono al passaggio del mouse e si chiudono poco dopo che il mouse e'
   * uscito: il tempo di tornarci sopra. Il pulsante della toolbar non agisce e non fissa niente.
   * Prima il clic sul pulsante del commit committava nella radice: con il lavoro sparso fra i
   * repository lasciava fuori i file dei submodule senza dirlo.
   */
  public hoveredPanel: GitPanel | null = null;
  private panelCloseTimer: ReturnType<typeof setTimeout> | null = null;
  private static readonly PANEL_CLOSE_DELAY_MS = 400;

  /** I remoti dei submodule si interrogano a parte: il fetch del progetto non dice niente su di loro. */
  public isFetchingRemotes = false;
  private lastRemotesFetch = 0;
  private remotesAskedFor: string | null = null;
  public isLoadingChangedFiles: boolean = false;
  public hasRemoteConfigured: boolean = true; // Default true to hide menu initially
  public currentRemoteUrl: string = '';
  public isGitRepository: boolean = true; // False for non-Git projects (hides Git UI elements)
  public authenticationMissing: boolean = false; // True when no credentials are configured
  public authenticationFailed: boolean = false; // True when credentials exist but auth failed (VPN, token expired)
  public authenticationFailureReason: string = ''; // Detailed reason for auth failure

  //@Output() toggleSidenav = new EventEmitter<void>();
  constructor(
    public dialog: MatDialog,
    private monitorMDService: MdServerMessagesService,
    private http: HttpClient,
    private _snackBar: MatSnackBar,
    public mdFileService: MdFileService,
    private gitservice: GITService,
    private appSettings: AppCurrentMetadataService,
    private projectService: ProjectsService,
    private reviewContext: ReviewContextService,
    private workingChanges: WorkingChangesService,
    private federationService: FederationService,
    private router: Router,
    private waitingDialogService: WaitingDialogService,
    private bookmarksService: BookmarksService,
    private navService: MdNavigationService,
    private tocService: TocGenerationService,
    private tocProgressService: TocProgressService,
    private translate: TranslateService,
    private themeService: ThemeService,
    private documentRefreshService: DocumentRefreshService,
    private mailboxNotifications: AgentMailboxNotificationService,
    private agentCityState: AgentCityStateService,
    private agentActivity: AgentActivityService

  ) {
    this.TitleToShow = "MdExplorer";
    this.connectionIsActive = true;
  }

  /** Gli agenti al lavoro adesso nel progetto aperto: l'indicatore della toolbar compare solo se ce ne sono. */
  public agentsRunning: RunningAgent[] = [];
  /** L'ora con cui si calcola «da quanto»: avanza solo finché qualcuno lavora. */
  private agentsRunningNow: number = Date.now();
  private agentsRunningClock: any = null;

  /** «account-manager, responsabile-tecnico»: i nomi nel suggerimento dell'indicatore. */
  agentsRunningNames(): string {
    return this.agentsRunning.map(a => a.agentName).join(', ');
  }

  /** Da quanti minuti lavora un agente, per l'elenco dell'indicatore. */
  agentRunningMinutes(agent: RunningAgent): number {
    return Math.max(0, Math.floor((this.agentsRunningNow - new Date(agent.startedAt).getTime()) / 60000));
  }

  private setAgentsRunning(running: RunningAgent[]): void {
    this.agentsRunning = running;
    this.agentsRunningNow = Date.now();
    if (running.length > 0 && !this.agentsRunningClock) {
      this.agentsRunningClock = setInterval(() => this.agentsRunningNow = Date.now(), 15000);
    } else if (running.length === 0 && this.agentsRunningClock) {
      clearInterval(this.agentsRunningClock);
      this.agentsRunningClock = null;
    }
  }

  /** Non-letti della inbox dell'umano (§13 Fase 4a): badge sulla campanella. */
  public mailboxUnread: number = 0;

  /** Apre la "città degli agenti" del progetto corrente (registry + trust, §6). */
  openAgentRegistry(): void {
    const projectPath = this.projectService.currentProjects$.value?.path || '';
    this.dialog.open(AgentRegistryDialogComponent, {
      width: '720px',
      maxHeight: '80vh',
      data: { projectPath },
    });
  }

  /** Apre la vista/curatela della memoria degli agenti del progetto (§11 Fase 5d). */
  openAgentMemory(): void {
    const projectPath = this.projectService.currentProjects$.value?.path || '';
    this.dialog.open(AgentMemoryDialogComponent, {
      width: '720px',
      maxHeight: '80vh',
      data: { projectPath },
    });
  }

  /** Apre il centro notifiche (messaggi degli agenti verso l'umano, §13 Fase 4a). */
  openMailbox(): void {
    this.mailboxNotifications.open();
  }

  ngOnInit(): void {
    this.slideDeckSubscription = this.documentRefreshService.slideDeckShown$
      .subscribe(shown => this.slideDeckShown = shown);

    // Entrare o uscire dalla revisione cambia di chi sono i numeri della finestrella.
    this.citySubscriptions.add(this.reviewContext.agent$.subscribe(agent => {
      this.reviewAgent = agent;
      this.changesView = null;
    }));

    // I comandi della città esistono solo dove la città è accesa: seguiamo il progetto
    // aperto (BehaviorSubject, quindi il valore corrente arriva subito) e lo stato
    // condiviso, che le impostazioni aggiornano senza bisogno di ricaricare la toolbar.
    this.citySubscriptions.add(
      this.projectService.currentProjects$.subscribe(project =>
        this.agentCityState.refresh(project?.path || '')));
    this.citySubscriptions.add(
      this.agentCityState.enabled$.subscribe(enabled => this.cityEnabled = enabled));

    // Chi lavora adesso: vale anche a città spenta, perché un agente si lancia a mano in ogni progetto.
    this.citySubscriptions.add(
      this.projectService.currentProjects$.subscribe(project =>
        this.agentActivity.follow(project?.path || '')));
    this.citySubscriptions.add(
      this.agentActivity.running$.subscribe(running => this.setAgentsRunning(running)));

    // Get connectionId from SignalR service for export notifications
    this.connectionId = this.monitorMDService.connectionId;
    // If connectionId is not yet available, get it when ready
    if (!this.connectionId) {
      this.monitorMDService.getConnectionId((id) => {
        this.connectionId = id;
      }, this);
    }

    this.monitorMDService.addMdProcessedListener(this.markdownFileIsProcessed, this);
    this.monitorMDService.addPdfIsReadyListener(this.showPdfIsready, this); //TODO: da spostare in SignalR
    this.monitorMDService.addMdRule1Listener(this.showRule1IsBroken, this);//TODO: da spostare in SignalR
    this.monitorMDService.addYamlAutoGeneratedListener(this.showYamlAutoGenerated, this);
    // get current branch name and if the branch has something to commit
    // Il servizio git e' un singleton che continua a emettere anche a toolbar distrutta (polling):
    // senza rilasciare la sottoscrizione, sulla pagina dei progetti scattava l'errore «nessun progetto».
    this.citySubscriptions.add(this.gitservice.currentBranch$.subscribe(branch => {
      this.currentBranch = branch.name;
      this.rootCommitsToPush = branch.howManyCommitAreToPush;
      this.applyPushCount();
      this.connectionIsActive = true;
      // Se c'e' qualcosa da committare NON lo decide piu' questo conteggio: escludeva i
      // submodule, quindi con del lavoro non salvato dentro uno di essi il pulsante restava
      // spento. Lo decide la vista per repository, che e' anche cio' che il pannello mostra:
      // una fonte sola, e i due numeri non possono piu' contraddirsi.
      this.loadChangedFiles();
      // Anche qui: lo stato locale arriva sempre, pure quando il remoto di origin non risponde.
      this.askUpstreamNowAndThen();
    }));

    this.citySubscriptions.add(this.gitservice.commmitsToPull$.subscribe(_ => {
      this.rootCommitsToPush = _.howManyCommitAreToPush;
      this.applyPushCount();
      this.howManyFilesAreToPull = _.howManyFilesAreToPull;
      this.connectionIsActive = _.connectionIsActive;
      this.isCheckingConnection = false;
      this.filesAndAuthors = _.whatFilesWillBeChanged;
      // Se c'e' da scaricare lo dice la vista per repository, come per il commit: il polling ha
      // appena interrogato il remoto del progetto, quindi la si rilegge. I remoti dei submodule
      // si interrogano una volta per progetto aperto, e poi quando si apre il pannello.
      this.loadChangedFiles();
      if (_.connectionIsActive) this.askRemotesNowAndThen();
      // La sorgente è un altro remoto: la si interroga comunque vada la connessione a origin.
      this.askUpstreamNowAndThen();
    }));
    
    // Mailbox non-letti (§13 Fase 4a): il badge segue il conteggio autoritativo.
    this.citySubscriptions.add(this.mailboxNotifications.unread$.subscribe(n => this.mailboxUnread = n));

    // Set initial project path if available
    const currentProject = this.projectService.currentProjects$.value;
    if (currentProject && currentProject.path) {
      this.gitservice.setProjectPath(currentProject.path);
      this.mailboxNotifications.setProject(currentProject.path);
    }

    // Subscribe to project changes to update Git service
    this.citySubscriptions.add(this.projectService.currentProjects$.subscribe((project: any) => {
      if (project && project.path) {
        // Reset Git state immediately when switching projects
        this.resetGitState();
        // Then set new project path and trigger poll
        this.gitservice.setProjectPath(project.path);
        // Ricarica il badge non-letti per il nuovo progetto
        this.mailboxNotifications.setProject(project.path);
        // Identità effettiva (banner impersonazione)
        this.loadIdentity();

      }
    }));

    this.checkConnection();
    this.reportCredentialMove();

    // manage resize fullscreen
    document.onfullscreenchange = (event) => {

      if (document.fullscreenElement) {
        this.screenType = "close_fullscreen";
      } else {
        this.screenType = "fullscreen";
      }
    };

    this.gitservice.getBranchList().subscribe(branches => {
      this.branches = branches;
    });



    // something is selected from treeview/sidenav
    this.mdFileService.selectedMdFileFromSideNav.subscribe(_ => {
      if (_ != null) {
        this.currentMdFile = _;
        this.mdFileService.navigationArray = [];
        this.absolutePath = _.fullPath;
        this.relativePath = _.relativePath;
      }
    });
    // something has changed on filesystem
    this.subscriptionserverSelectedMdFile = this.mdFileService.serverSelectedMdFile.subscribe(val => {

      var current = val[0];
      if (current != undefined) {
        let index = this.mdFileService.navigationArray.length;
        if (index > 0) {
          //if (current.fullPath == this.mdFileService.navigationArray[index - 1].fullPath) {
          if (current == this.mdFileService.navigationArray[index - 1]) {
            //return;
          }
        }

        this.navService.setNewNavigation(current);
        this.absolutePath = current.fullPath;
        this.relativePath = current.relativePath;
        this.currentMdFile = current;
      }

    });
  }


  ngOnDestroy(): void {
    console.log("ngOnDestroy toolbar");
    this.cancelPanelClose();
    this.subscriptionserverSelectedMdFile.unsubscribe();
    this.citySubscriptions.unsubscribe();
    this.slideDeckSubscription?.unsubscribe();
    if (this.agentsRunningClock) clearInterval(this.agentsRunningClock);
  }



  toggleSidenav() {
    let test = !this.appSettings.showSidenav.value;
    this.appSettings.showSidenav.next(test);
  }

  openRules(data: any): void {
    const dialogRef = this.dialog.open(RulesComponent, {
      width: 'auto',
      maxWidth: '90vw',
      disableClose: false,
      panelClass: 'subtle-dialog-panel',
      data: data
    });
    dialogRef.afterClosed().subscribe(_ => {
      if (_ && _.refactoringSourceActionId != undefined) {
        // User chose to apply suggestion
        this._snackBar.open(this.translate.instant('TOOLBAR.FILE_RENAMED'), '', {
          duration: 2500,
          horizontalPosition: 'right',
          verticalPosition: 'bottom',
          panelClass: ['success-snackbar']
        });
        
        this.dialog.open(RenameFileComponent, {
          width: '600px',
          data: _
        });
      } else if (_ === null) {
        // User chose to keep current filename
        this._snackBar.open(this.translate.instant('TOOLBAR.FILENAME_UNCHANGED'), '', {
          duration: 2000,
          horizontalPosition: 'right',
          verticalPosition: 'bottom',
          panelClass: ['subtle-snackbar']
        });
      }
    });
  }

  /**
   * Reset all Git-related state variables to initial/empty values.
   * Called when switching projects to ensure clean state.
   */
  private resetGitState(): void {
    // Reset branch and commit counters
    this.currentBranch = "";
    this.somethingIsChangedInTheBranch = false;
    this.somethingIsToPull = false;
    this.somethingIsToPush = false;
    this.howManyFilesAreToCommit = 0;
    this.howManyCommitAreToPush = 0;
    this.rootCommitsToPush = 0;
    this.submoduleCommitsToPush = 0;
    this.howManyFilesAreToPull = 0;
    this.howManyAreToPull = 0;
    this.reposToCommit = [];
    this.reposToPull = [];
    this.reposToPush = [];
    this.reposWithRemoteProblem = [];
    this.changesView = null;
    this.closePanels();
    this.remotesAskedFor = null;

    // Reset arrays
    this.filesAndAuthors = [];
    this.branches = [];
    this.taglist = [];

    // Reset remote configuration flags
    this.hasRemoteConfigured = false;  // Will show "checking..." until poll completes
    this.currentRemoteUrl = '';
    this.authenticationMissing = false;
    this.authenticationFailed = false;
    this.authenticationFailureReason = '';

    // Reset connection state
    this.connectionIsActive = false;
    this.isCheckingConnection = true;

    // Assume Git repository by default (will be updated after check)
    this.isGitRepository = true;
  }

  checkConnection(): void {
    this.isCheckingConnection = true;

    const projectPath = this.getProjectPath(true);
    if (!projectPath) {
      this.isCheckingConnection = false;
      return;
    }

    // Update the Git service with current project path
    this.gitservice.setProjectPath(projectPath);
    // checkConnection parte sempre da un'azione dell'utente: il polling può tornare a chiedere al remoto.
    this.gitservice.resumeRemotePolling();

    // Check remote status first
    this.gitservice.checkRemoteStatus(projectPath).subscribe(
      remoteStatus => {
        console.log('Remote status:', remoteStatus);

        // Track authentication status details
        this.authenticationMissing = remoteStatus.authenticationMissing || false;
        this.authenticationFailed = remoteStatus.authenticationFailed || false;
        this.authenticationFailureReason = remoteStatus.authenticationFailureReason || '';

        // Dallo sprint «un solo meccanismo»: l'unica via è il git nativo col suo credential
        // manager. O il remoto c'è e git si autentica, o si apre «Collega a un repository remoto».
        // Set hasRemoteConfigured to control "Setup Remote" visibility
        // false = show "Setup Remote", true = hide "Setup Remote"
        this.hasRemoteConfigured = remoteStatus.hasRemote && remoteStatus.canAuthenticate;
        this.currentRemoteUrl = remoteStatus.remoteUrl || '';

        // Log authentication status for debugging
        if (remoteStatus.hasRemote && this.authenticationMissing) {
          console.warn('⚠️ No credentials configured for remote. Show "Configure Git Account" button.');
          this.isCheckingConnection = false;
          this.connectionIsActive = false;
          // Branch list and history are LOCAL git operations: keep them available even
          // though remote credentials are missing (the alarm icon stays visible too).
          this.loadLocalBranchStatus(projectPath, remoteStatus.isGitRepository);
        } else if (remoteStatus.hasRemote && this.authenticationFailed) {
          console.warn('❌ Authentication failed (VPN/network issue). Show connection warning.');
          this.isCheckingConnection = false;
          this.connectionIsActive = false;
          // Remote unreachable/unauthenticated, but branches and history are local:
          // keep them available (the alarm icon stays visible too).
          this.loadLocalBranchStatus(projectPath, remoteStatus.isGitRepository);
        } else if (remoteStatus.hasRemote && remoteStatus.canAuthenticate) {
          console.log('✅ Remote configured and authentication successful using:', remoteStatus.authenticationMethod);

          // Reset auth failure flags on successful connection
          this.authenticationMissing = false;
          this.authenticationFailed = false;
          this.authenticationFailureReason = '';
          this.connectionIsActive = true;


          // Authentication successful - credentials are now cached
          // Now fetch Git data (will use cached credentials, no auth request)
          if (remoteStatus.isGitRepository) {
            this.isGitRepository = true;
            forkJoin([
              this.gitservice.modernGetBranchStatus(projectPath),
              this.gitservice.modernGetDataToPull(projectPath)
            ]).subscribe(
              ([branch, pullData]) => {
                console.log('📊 Git data retrieved using cached credentials');
                // Update observables with the received data
                this.gitservice.currentBranch$.next(branch);
                this.gitservice.commmitsToPull$.next(pullData);
                this.isCheckingConnection = false;
              },
              error => {
                console.error('Error fetching Git data:', error);
                this.isCheckingConnection = false;
                this.connectionIsActive = false;
              }
            );
          } else {
            this.isCheckingConnection = false;
            this.connectionIsActive = false;
          }
        } else if (remoteStatus.isGitRepository && !remoteStatus.hasRemote) {
          // Git repository exists but no remote configured
          console.log('📁 Git repository without remote - showing "Setup Remote" menu');
          this.isGitRepository = true;
          this.isCheckingConnection = false;
          this.connectionIsActive = true;
          // No remote configured: still load the local branch status so branch/history show.
          this.loadLocalBranchStatus(projectPath, true);
        } else {
          // Not a Git repository - reset all Git state
          console.log('📁 Not a Git repository - resetting Git state');
          this.isGitRepository = false;
          this.isCheckingConnection = false;
          this.connectionIsActive = false;
          this.hasRemoteConfigured = true; // Hide setup menu if not Git repo
        }
      },
      error => {
        console.error('Error checking remote status:', error);
        // On error, treat as non-Git repository
        this.isGitRepository = false;
        this.isCheckingConnection = false;
        this.connectionIsActive = false;
        // On error, assume remote is configured to hide the menu
        this.hasRemoteConfigured = true;
      }
    );
  }

  /**
   * Loads the LOCAL branch status (current branch + local change counts) and pushes it to
   * currentBranch$. Branch list and commit history are purely local git operations, so they
   * must stay available in the toolbar even when the remote is unreachable or unauthenticated.
   * Only the remote-dependent data (pull/push) is skipped in those cases.
   */
  private loadLocalBranchStatus(projectPath: string, isGitRepository: boolean): void {
    if (!isGitRepository) {
      return;
    }
    this.isGitRepository = true;
    this.gitservice.modernGetBranchStatus(projectPath).subscribe(
      branch => this.gitservice.currentBranch$.next(branch),
      error => console.error('Error fetching local branch status:', error)
    );
  }

  private showRule1IsBroken(data: any, objectThis: ToolbarComponent) {
    objectThis.openRules(data);
  }

  private sendExportRequest(objectThis: ToolbarComponent) {
    const url = '../api/mdexport/' + objectThis.relativePath + '?ConnectionId=' + objectThis.connectionId;
    return objectThis.http.get(url)
      .subscribe(data => { console.log(data) });
  }

  private showPdfIsready(data: any, objectThis: ToolbarComponent) {
    let snackRef = objectThis._snackBar.open("seconds: " + data.executionTimeInSeconds, objectThis.translate.instant('TOOLBAR.OPEN_FOLDER'), { duration: 5000, verticalPosition: 'top' });
    snackRef.onAction().subscribe(() => {
      const url = '../api/AppSettings/OpenChromePdf?path=' + data.path;
      return objectThis.http.get(url)
        .subscribe(data => { console.log(data) });
    });
  }

  private showYamlAutoGenerated(data: any, objectThis: ToolbarComponent) {
    objectThis._snackBar.open(data.message, 'OK', {
      duration: 4000,
      verticalPosition: 'top',
      panelClass: ['warning-snackbar']
    });
  }

  private markdownFileIsProcessed(data: MdFile, objectThis: ToolbarComponent) {
    objectThis.currentMdFile = data;
    objectThis.mdFileService.navigationArray.push(data);
    objectThis.mdFileService.setSelectedMdFileFromServer(data);
  }

  OpenEditor() {
    const url = '../api/AppSettings/OpenFile?path=' + this.absolutePath;
    this.http.get<any>(url).subscribe({
      next: data => {
        // Docker mode: backend can't spawn the host's editor process, so it
        // returns a "vscode://file/..." (or jetbrains://) URL. Hand it to the
        // browser, which forwards it to the OS, which launches the editor on
        // the host. On native Windows/Linux this branch is never taken.
        if (data && data.openUrl) {
          window.location.href = data.openUrl;
          return;
        }
        console.log(data);
      },
      error: err => {
        // Il backend sa PERCHÉ non ha aperto niente («quel CLI non è installato», «questo
        // progetto non ha un ambiente agentico»): senza questo ramo il messaggio finiva nella
        // console e la matita sembrava semplicemente rotta.
        const message = err?.error?.error || err?.message || String(err);
        console.error('[Toolbar] apertura nell\'editor fallita:', message);
        this._snackBar.open(message, 'OK', { duration: 8000, verticalPosition: 'top' });
      }
    });
  }

  Export() {
    if (!this.relativePath) {
      this._snackBar.open(this.translate.instant('TOOLBAR.SELECT_DOC_FIRST'), 'OK', { duration: 3000, verticalPosition: 'top' });
      return;
    }
    this._snackBar.open(this.translate.instant('TOOLBAR.EXPORT_QUEUED'), null, { duration: 2000, verticalPosition: 'top' });
    this.sendExportRequest(this);
  }

  /**
   * Open the current document in its own standalone window ("detach").
   *
   * The backend already serves each document as a complete, self-contained HTML
   * page at /api/mdexplorer/{path}; the detached window just points a bare
   * Electron BrowserWindow (or a browser tab on web) at that URL — no Angular
   * shell, no second backend, only one extra renderer process.
   *
   * The URL is ABSOLUTE (window.location.origin) because the bare window has no
   * Angular base-href to resolve the relative '../api/...' against.
   *
   * connectionId IS required: the backend resolves the project root (and thus
   * the file) from the DatabaseManager context keyed by connectionId — without
   * it GetProjectPath() returns empty and the page renders blank. We reuse this
   * window's connectionId; `source=detached` tells the backend to skip the
   * SignalR notifications so the main window's state is not disturbed.
   * `detached=true` tells common.js / mde-exec-blocks.js to visibly disable the
   * features that depend on the (now-absent) Angular parent (Run, path picker).
   */
  detachDocument(): void {
    if (!this.relativePath) {
      this._snackBar.open(this.translate.instant('TOOLBAR.SELECT_DOC_FIRST'), 'OK', { duration: 3000, verticalPosition: 'top' });
      return;
    }
    const cleanPath = this.relativePath.replace(/^[\/\\]+/, '');
    const time = new Date().getTime() / 1000;
    const theme = this.themeService.getResolvedTheme();
    const connectionId = this.connectionId ?? this.monitorMDService.connectionId ?? '';
    const url = `${window.location.origin}/api/mdexplorer/${cleanPath}?time=${time}&ConnectionId=${connectionId}&source=detached&theme=${theme}&detached=true`;

    const electronAPI = (window as any).electronAPI;
    if (electronAPI?.detachDocument) {
      // Desktop: spawn a bare Electron window reusing the running backend.
      electronAPI.detachDocument(url);
    } else {
      // Web fallback: a new browser window/tab.
      window.open(url, '_blank');
    }
  }

  /**
   * The slide deck on screen as a PDF. On the desktop, Electron asks where to save it (next to
   * the markdown file by default) and prints reveal.js's print view with the slides'
   * backgrounds. In a browser there is no such printer: the print view opens in a new tab,
   * to print with the browser.
   */
  async exportSlidesPdf(): Promise<void> {
    if (!this.relativePath) {
      return;
    }
    const cleanPath = this.relativePath.replace(/^[\/\\]+/, '');
    const theme = this.themeService.getResolvedTheme();
    const connectionId = this.connectionId ?? this.monitorMDService.connectionId ?? '';
    // source=detached: the server resolves the project from the connectionId without pushing
    // "document processed" events to this window.
    const url = `${window.location.origin}/api/mdexplorer/${cleanPath}?ConnectionId=${connectionId}&source=detached&theme=${theme}`;

    const electronAPI = (window as any).electronAPI;
    if (!electronAPI?.exportSlidesPdf) {
      window.open(`${url}&print-pdf`, '_blank');
      this._snackBar.open(this.translate.instant('TOOLBAR.SLIDES_PDF_BROWSER_HINT'), 'OK', { duration: 8000, verticalPosition: 'top' });
      return;
    }

    const suggestedPath = this.absolutePath ? this.absolutePath.replace(/\.md$/i, '') + '.pdf' : undefined;
    const result = await electronAPI.exportSlidesPdf(url, suggestedPath);
    if (result?.success) {
      this._snackBar.open(this.translate.instant('TOOLBAR.SLIDES_PDF_SAVED', { path: result.filePath }), 'OK', { duration: 6000, verticalPosition: 'top' });
    } else if (!result?.canceled) {
      this._snackBar.open(this.translate.instant('TOOLBAR.SLIDES_PDF_FAILED', { error: result?.error ?? '' }), 'OK', { verticalPosition: 'top' });
    }
  }

  /** An HTML export is running: the button waits for it (a big project takes a while). */
  staticSiteExporting = false;

  /**
   * The slide deck on screen, and everything its links reach (decks, documents, HTML pages, their
   * files), as a static web site in a zip to open without MdExplorer (sprint
   * docs-internal/Sprints/2026-09-30-Slide-Export-HTML.md). The backend builds the zip and sends it
   * as a download: Electron asks where to save it with its own dialog, a browser saves it as it
   * saves any download. What could not be carried is in _mde/resoconto.html inside the zip.
   */
  async exportStaticSite(): Promise<void> {
    if (!this.relativePath) {
      return;
    }
    const connectionId = this.connectionId ?? this.monitorMDService.connectionId ?? '';
    const relativePath = this.relativePath.replace(/^[\/\\]+/, '');

    this.staticSiteExporting = true;
    try {
      const response = await fetch(`${window.location.origin}/api/MdStaticSite/Export?ConnectionId=${encodeURIComponent(connectionId)}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ relativePath, theme: this.themeService.getResolvedTheme() }),
      });
      if (!response.ok) {
        const body = await response.json().catch(() => ({}));
        throw new Error(body.error ?? `HTTP ${response.status}`);
      }
      const zip = await response.blob();
      const link = document.createElement('a');
      link.href = URL.createObjectURL(zip);
      link.download = relativePath.split(/[\/\\]/).pop()!.replace(/\.md$/i, '') + '.zip';
      link.click();
      setTimeout(() => URL.revokeObjectURL(link.href), 60000);

      const issues = Number(response.headers.get('X-Mde-Issues') ?? 0);
      const params = { pages: response.headers.get('X-Mde-Pages'), files: response.headers.get('X-Mde-Files'), issues };
      this._snackBar.open(
        this.translate.instant(issues ? 'TOOLBAR.STATIC_SITE_SAVED_WITH_ISSUES' : 'TOOLBAR.STATIC_SITE_SAVED', params),
        'OK', { duration: issues ? undefined : 8000, verticalPosition: 'top' });
    } catch (err: any) {
      this._snackBar.open(this.translate.instant('TOOLBAR.STATIC_SITE_FAILED', { error: err?.message ?? '' }), 'OK', { verticalPosition: 'top' });
    } finally {
      this.staticSiteExporting = false;
    }
  }

  /**
   * Reload the currently open document in the main viewer. The actual iframe
   * reload lives in MainContentComponent.refreshCurrentFile(); we only signal it
   * through DocumentRefreshService so the app-bar stays decoupled from the viewer.
   */
  refreshDocument(): void {
    if (!this.relativePath) {
      this._snackBar.open(this.translate.instant('TOOLBAR.SELECT_DOC_FIRST'), 'OK', { duration: 3000, verticalPosition: 'top' });
      return;
    }
    this.documentRefreshService.requestRefresh();
  }


  public screenType = "fullscreen";

  FullScreenToggle(): void {
    if (!document.fullscreenElement) {
      document.documentElement.requestFullscreen();
    } else {
      document.exitFullscreen();
    }

  }

  /**
   * Handles Git operation responses for both legacy and modern services
   */
  public handleGitResponse(responseFromPull: any, operation: string): void {
    // Handle connection issues
    if (responseFromPull.isConnectionMissing) {
      const dialogRef = this.dialog.open(GitMessagesComponent, {
        width: '300px',
        data: {
          message: 'Missing connection',
          description: 'Please verify your vpn or network settings'
        }
      });
      return;
    }

    // Note: Modern Git uses SSH authentication, no manual credentials needed

    // Handle conflicts
    if (responseFromPull.thereAreConflicts) {
      const dialogRef = this.dialog.open(GitMessagesComponent, {
        width: '300px',
        data: {
          message: 'Conflicts appear',
          description: responseFromPull.errorMessage
        }
      });
      return;
    }

    // Prima i numeri LOCALI, poi il remoto. Se si aggiornassero solo dentro checkConnection()
    // resterebbero appesi alla rete (probe di autenticazione + fetch da origin): dopo un commit
    // il contatore «da committare» continuerebbe a mostrare i file appena salvati finche' il
    // remoto non risponde, o fino al giro di polling successivo — 60 secondi.
    this.refreshLocalGitCounters();

    // Always refresh connection status after successful operations
    // Tree refresh is handled via SignalR gitPullRefreshed event in md-file.service + md-tree
    this.checkConnection();
  }

  /**
   * Rilegge i contatori che stanno gia' sul disco: `git status` e i ref locali. Nessuna
   * chiamata al remoto, quindi risponde subito e non puo' restare in attesa di una rete lenta
   * o assente. Cio' che dipende davvero dal remoto (quanto c'e' da scaricare) lo aggiorna
   * checkConnection() per conto suo, quando arriva.
   */
  private refreshLocalGitCounters(): void {
    const projectPath = this.getProjectPath(true);
    if (!projectPath) return;

    this.gitservice.modernGetBranchStatus(projectPath).subscribe(
      // La sottoscrizione a currentBranch$ ricarica anche la vista per repository: una fonte
      // sola, cosi' il numero sul pulsante e le righe del pannello non si contraddicono.
      branch => this.gitservice.currentBranch$.next(branch),
      // Stato locale non disponibile: almeno i file da committare si rileggono lo stesso.
      () => this.loadChangedFiles()
    );
  }

  /**
   * Validates and returns the current project path.
   * Shows error if no project is selected — unless `silent`: i refresh automatici (polling,
   * rilettura dopo un evento) non sono un'azione dell'utente e non devono protestare.
   */
  private getProjectPath(silent = false): string | null {
    const currentProject = this.projectService.currentProjects$.value;
    
    if (!currentProject || !currentProject.path) {
      if (silent) return null;
      console.error('No current project path available');
      this._snackBar.open(this.translate.instant('TOOLBAR.NO_PROJECT_SELECTED'), 'OK', {
        duration: 3000,
        verticalPosition: 'top',
        panelClass: ['error-snackbar']
      });
      return null;
    }
    
    return currentProject.path;
  }

  openBranch(branch: IBranch): void {
    this.gitservice.checkoutSelectedBranch(branch).subscribe(_ => {
      this.currentBranch = _.name;
      var mdFile = new MdFile("Welcome to MDExplorer", '/../welcome.html', 0, false);
      mdFile.relativePath = '/../../welcome.html';
      this.mdFileService.setSelectedMdFileFromSideNav(mdFile);
      this.projectService.setNewFolderProject(_.fullPath);

    });
    this.matMenuTrigger?.closeMenu();
  }

  // ---- Impersonazione utente (test città) ----

  private showError(err: any): void {
    const msg = err?.error?.error || err?.message || 'Operazione fallita.';
    this._snackBar.open(`⚠️ ${msg}`, 'OK', { duration: 8000, verticalPosition: 'top' });
  }

  private get currentProjectPath(): string {
    return this.projectService.currentProjects$.value?.path || '';
  }

  loadIdentity(): void {
    const p = this.currentProjectPath;
    if (!p) { this.identity = null; return; }
    this.federationService.impersonationStatus(p).subscribe({
      next: (s) => this.identity = s,
      error: () => this.identity = null,
    });
  }

  loadCityUsers(): void {
    const p = this.currentProjectPath;
    if (!p) { this.cityUsers = []; return; }
    this.federationService.cityUsers(p).subscribe({
      next: (r) => this.cityUsers = r.users || [],
      error: () => this.cityUsers = [],
    });
  }

  toggleTestMode(enabled: boolean): void {
    this.federationService.setTestMode(enabled).subscribe({
      next: () => this.loadIdentity(),
      error: (err) => this.showError(err),
    });
  }

  actAs(email: string): void {
    const p = this.currentProjectPath;
    if (!p) { return; }
    this.federationService.impersonate(p, email).subscribe({
      next: (s) => { this.identity = s; this.matMenuTrigger?.closeMenu(); },
      error: (err) => this.showError(err),
    });
  }

  backToMe(): void {
    const p = this.currentProjectPath;
    if (!p) { return; }
    this.federationService.stopImpersonation(p).subscribe({
      next: (s) => this.identity = s,
      error: (err) => this.showError(err),
    });
  }

  // Fase 7h — carica i worktree degli agenti quando il menu branch si apre.
  loadWorktrees(): void {
    const cid = this.connectionId || this.monitorMDService.connectionId;
    if (!cid) { this.worktreeList = []; return; }
    this.mdFileService.getAgentWorktrees(cid).subscribe({
      next: list => this.worktreeList = list,
      error: () => this.worktreeList = []
    });
  }

  /**
   * Si entra nel lavoro di un agente: da qui l'etichetta del ramo, la striscia sul documento, le differenze e
   * il commit parlano di lui — lo stesso stato in cui porta «Ci metto mano». Il pannello di sinistra passa ai
   * suoi file, e il documento aperto si rilegge dalla sua copia.
   */
  openWorktree(agent: string): void {
    this.reviewContext.enterAgent(agent);
    this.reviewContext.showChanges();
    this.mdFileService.viewWorktree(agent);
    this.matMenuTrigger?.closeMenu();
  }

  openHistory(): void {
    const projectPath = this.getProjectPath();
    if (!projectPath) return;

    const currentProject = this.projectService.currentProjects$.value;
    const projectName = currentProject?.name || 'Current Project';

    const dialogRef = this.dialog.open(GitHistoryDialogComponent, {
      width: '900px',
      height: '700px',
      data: {
        projectPath: projectPath,
        projectName: projectName,
        repos: this.dialogRepos()
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      // Handle any result if needed
      console.log('History dialog closed');
    });

    this.matMenuTrigger?.closeMenu();
  }

  /**
   * Il progetto e i suoi submodule, per il selettore dei dialoghi di cronologia e rami: ognuno
   * ha la SUA storia e i SUOI rami. In revisione del lavoro di un agente no: li' cronologia e
   * rami restano quelli del progetto, e il ramo del posto di lavoro lo decide il run.
   */
  private dialogRepos(): GitDialogRepo[] {
    if (this.reviewAgent) return [];
    return this.toDialogRepos(this.changesView);
  }

  private toDialogRepos(view: WorkingChangesView | null): GitDialogRepo[] {
    if (!view?.repos?.length) return [];
    return view.repos
      .filter(r => !r.notInitialized)
      .map(r => ({
        path: this.absolutePathIn(view.rootPath, r),
        label: r.label,
        branch: r.branch,
        detached: r.detached,
        isRoot: r.depth === 0,
        recordedCommit: r.recordedCommit || null,
      }));
  }

  openBranchDialog(): void {
    const projectPath = this.getProjectPath();
    if (!projectPath) return;

    const currentProject = this.projectService.currentProjects$.value;
    const projectName = currentProject?.name || 'Current Project';

    const dialogRef = this.dialog.open(GitBranchDialogComponent, {
      width: '600px',
      data: {
        projectPath: projectPath,
        projectName: projectName,
        repos: this.dialogRepos(),
        // Un cambio di ramo del progetto sposta anche i submodule: il selettore li rilegge.
        reloadRepos: () => this.workingChanges.list(projectPath).pipe(map(view => this.toDialogRepos(view)))
      }
    });

    dialogRef.afterClosed().subscribe(() => {
      // Un cambio di ramo — del progetto o di un submodule — cambia cosa c'e' da committare,
      // da scaricare e da pubblicare: i tre pannelli si rileggono.
      this.refreshLocalGitCounters();
    });

    this.matMenuTrigger?.closeMenu();
  }

  openAddSubmoduleDialog(): void {
    const projectPath = this.getProjectPath();
    if (!projectPath) return;

    const dialogRef = this.dialog.open(GitAddSubmoduleDialogComponent, {
      width: '600px',
      data: {
        projectPath: projectPath,
        connectionId: this.connectionId
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === true) {
        // Tree refresh arrives via SignalR 'gitBranchSwitched'; refresh git status here
        this.checkConnection();
      }
    });

    this.matMenuTrigger?.closeMenu();
  }

  openSetupRemote(): void {
    const projectPath = this.getProjectPath();
    if (!projectPath) return;

    const currentProject = this.projectService.currentProjects$.value;
    const projectName = projectPath.split(/[/\\]/).pop() || 'repository';

    const dialogRef = this.dialog.open(GitSetupRemoteGenericDialogComponent, {
      width: '650px',
      data: {
        projectPath: projectPath,
        projectName: projectName,
        prefilledRemoteUrl: this.currentRemoteUrl  // Pre-fill URL if available (from remote-status)
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === true) {
        // Remote was successfully configured, update status
        this.checkConnection();
      }
      console.log('Setup remote dialog closed');
    });

    this.matMenuTrigger?.closeMenu();
  }

  /**
   * Il trasloco delle credenziali dal DB di MdExplorer al credential manager di git: se
   * qualcosa non si è potuto spostare, l'utente lo deve sapere, una volta, con il perché.
   */
  private reportCredentialMove(): void {
    this.gitservice.getCredentialMoveReport().subscribe(report => {
      if (!report || report.failed === 0) { return; }
      const first = report.entries.find(e => !e.moved);
      console.warn('[Toolbar] credenziali git non spostate nel credential manager:', report.entries.filter(e => !e.moved));
      this._snackBar.open(
        this.translate.instant('TOOLBAR.CREDENTIAL_MOVE_FAILED', { count: report.failed, reason: first?.reason || '' }),
        'OK', { duration: 15000, verticalPosition: 'top', panelClass: ['warning-snackbar'] });
    });
  }

  openGitInitWizard(): void {
    const projectPath = this.getProjectPath();
    if (!projectPath) {
      this._snackBar.open(this.translate.instant('TOOLBAR.NO_PROJECT_PATH'), 'OK', {
        duration: 3000,
        verticalPosition: 'top'
      });
      return;
    }

    // Import the component dynamically
    import('../../../git/dialogs/git-init-wizard/git-init-wizard-dialog.component').then(m => {
      const dialogRef = this.dialog.open(m.GitInitWizardDialogComponent, {
        width: '600px',
        data: {
          repositoryPath: projectPath
        }
      });

      dialogRef.afterClosed().subscribe(initialized => {
        if (initialized === true) {
          console.log('[Toolbar] Git repository initialized, refreshing status');
          // Refresh Git status after initialization
          this.checkConnection();
        }
      });
    });
  }

  bookmarkToggle(): void {
    if (!this.currentMdFile) {
      this._snackBar.open(this.translate.instant('TOOLBAR.SELECT_FILE_FIRST'), 'OK', {
        duration: 2000,
        verticalPosition: 'top'
      });
      return;
    }
    
    const currentProject = this.projectService.currentProjects$.value;
    if (!currentProject || !currentProject.id) {
      this._snackBar.open(this.translate.instant('TOOLBAR.SELECT_PROJECT_FIRST'), 'OK', {
        duration: 2000,
        verticalPosition: 'top'
      });
      return;
    }
    
    let bookmark: Bookmark = new Bookmark(this.currentMdFile);
    bookmark.projectId = currentProject.id;
    this.bookmarksService.toggleBookmark(bookmark);
  }

  openReactEditor(): void {
    // Navigate to the route defined in MdExplorerModule
    // Assumes MdExplorerModule is loaded under '/main' and 'navigation' is a parent route segment
    this.router.navigate(['/main/navigation/react-editor']);
  }

  openAiChat(): void {
    // Navigate to AI chat within the application
    this.router.navigate(['/main/navigation/ai-chat']);
  }

  /**
   * Load the list of changed files for the commit panel hover
   */
  loadChangedFiles(): void {
    const projectPath = this.getProjectPath(true);
    if (!projectPath) return;

    // «Carico…» solo la prima volta: a ogni rilettura le righe restano dove sono, altrimenti il
    // pannello aperto lampeggerebbe sotto il mouse.
    this.isLoadingChangedFiles = !this.changesView;
    // Stessa fonte del tab — git — cosi' i numeri della finestrella e la lista che vedi
    // cliccando «vedi le differenze» non possono raccontare due storie diverse.
    this.workingChanges.list(projectPath, this.reviewAgent).subscribe({
      next: view => {
        this.changesView = view;
        this.applyChangeSummary(view);
        this.isLoadingChangedFiles = false;
      },
      error: err => {
        this.changesView = err?.error?.problem ? err.error : null;
        this.applyChangeSummary(this.changesView);
        this.isLoadingChangedFiles = false;
      },
    });
  }

  /**
   * Un pulsante si accende se ALMENO UN repository ha qualcosa di quel tipo — la radice o un
   * submodule, indifferentemente. Tutti e tre leggono la STESSA vista, cosi' non possono
   * raccontare tre storie diverse.
   */
  private applyChangeSummary(view: WorkingChangesView | null): void {
    const repos = view?.repos || [];
    const submodules = repos.filter(r => r.depth > 0);
    const roots = repos.filter(r => r.depth === 0);

    // Si salva e si pubblica dal basso: i submodule sopra il progetto.
    const bottomUp = [...submodules, ...roots];
    this.reposToCommit = bottomUp.filter(r => this.repoHasWork(r));
    this.howManyFilesAreToCommit = this.reposToCommit.reduce(
      (n, r) => n + this.toCommit(r).length + (r.pointersToRegister?.length || 0), 0);
    this.somethingIsChangedInTheBranch = this.reposToCommit.length > 0;

    // Si scarica dall'alto: il progetto sopra i submodule. In revisione niente: nel posto di
    // lavoro di un agente gli scaricamenti li decide il run.
    this.reposToPull = this.reviewAgent ? [] : repos.filter(r => this.repoHasIncoming(r));
    this.howManyAreToPull = this.reposToPull.reduce((n, r) => n + (r.behind || 0) + (this.needsAlign(r) ? 1 : 0), 0);
    this.somethingIsToPull = this.reposToPull.length > 0;
    this.reposWithRemoteProblem = repos.filter(r => !!r.remoteProblem);
    this.rootIsBehind = roots.some(r => (r.behind || 0) > 0);

    this.reposToPush = bottomUp.filter(r => (r.ahead || 0) > 0);
    // I submodule contano per «da pushare» quanto la radice: «Pubblica tutto» porta anche loro.
    this.submoduleCommitsToPush = submodules.reduce((n, r) => n + (r.ahead || 0), 0);
    if (roots.length) this.rootCommitsToPush = roots[0].ahead || 0;
    this.applyPushCount();
  }

  /**
   * «Da pushare» = commit della radice + commit dei submodule. Prima contava solo la radice: dopo
   * un commit dentro un submodule il pulsante non compariva (visto il 29/09/2026).
   */
  private applyPushCount(): void {
    this.howManyCommitAreToPush = (this.rootCommitsToPush || 0) + this.submoduleCommitsToPush;
    this.somethingIsToPush = this.howManyCommitAreToPush > 0;
  }

  /**
   * Cio' che e' DA COMMITTARE: solo quello che git status vede. `files` contiene anche i commit non
   * ancora pubblicati, e dopo un commit teneva acceso «da committare» sui file appena salvati.
   */
  private toCommit(repo: RepoChanges): WorkingChange[] {
    return repo.uncommitted ?? repo.files;
  }

  /**
   * Qualcosa da fare QUI: file da salvare, la versione nuova di un submodule da registrare,
   * oppure un'unione rimasta a meta' — che si chiude proprio con un commit.
   *
   * Un submodule piu' avanti del registrato NON e' lavoro del submodule: e' lavoro di chi lo
   * contiene, ed e' li' che compare. Prima la riga si accendeva sul submodule, col suo
   * «Committa» che dentro il submodule non aveva niente da committare.
   */
  repoHasWork(repo: RepoChanges): boolean {
    return this.toCommit(repo).length > 0 || (repo.pointersToRegister?.length || 0) > 0 || !!repo.mergeInProgress;
  }

  /**
   * Il submodule e' piu' indietro della versione che il progetto registra (o non e' mai stato
   * scaricato): va ALLINEATO. Non e' una versione nuova da registrare — git usa la stessa sigla
   * nei due versi, e committando il progetto adesso si registrerebbe quella vecchia.
   */
  needsAlign(repo: RepoChanges): boolean {
    return repo.depth > 0 && (repo.notInitialized || repo.relation === 'behind' || repo.relation === 'unknown');
  }

  repoHasIncoming(repo: RepoChanges): boolean {
    return (repo.behind || 0) > 0 || this.needsAlign(repo);
  }

  /** Quanti file di un tipo in un repository: i numeri della riga. */
  countIn(repo: RepoChanges, change: ChangeKind): number {
    return this.toCommit(repo).filter(f => f.change === change).length;
  }

  /**
   * Cosa scrivere sul pulsante. Il numero da solo mentirebbe quando il lavoro e' sparso: quei
   * file non si chiudono con un commit, ma con uno per repository. Il numero di repository e'
   * l'informazione che manca, quindi si dice.
   */
  toCommitLabel(): string {
    return this.panelLabel('COMMIT', this.howManyFilesAreToCommit, this.reposToCommit.length);
  }

  /** La sorgente del progetto (remoto `upstream`): il pulsante compare solo se ha aggiornamenti. */
  upstream: UpstreamStatus | null = null;

  /** Rilegge cosa ha di nuovo la sorgente; `fetch` la interroga, altrimenti vale l'ultima risposta. */
  private loadUpstream(fetch: boolean): void {
    const projectPath = this.getProjectPath(true);
    if (!projectPath || this.reviewAgent) { this.upstream = null; return; }
    this.workingChanges.upstreamStatus(projectPath, fetch).subscribe({
      next: status => this.upstream = status?.hasUpstream ? status : null,
      // Stato ignoto: il pulsante resta spento, e lo si dichiara.
      error: err => { console.warn('[Upstream] sorgente non leggibile, pulsante spento:', err); this.upstream = null; },
    });
  }

  /** «Scarica gli aggiornamenti»: dalla sorgente nella cartella, e poi su origin. */
  pullUpstream(): void {
    const projectPath = this.getProjectPath();
    if (!projectPath) return;
    this.runRepoAction('GITFLOW.UPSTREAM_PULLING', '',
      this.workingChanges.pullUpstream(projectPath, this.currentConnectionId()), true);
  }

  toPullLabel(): string {
    return this.panelLabel('PULL', this.howManyAreToPull, this.reposToPull.length);
  }

  toPushLabel(): string {
    return this.panelLabel('PUSH', this.howManyCommitAreToPush, Math.max(1, this.reposToPush.length));
  }

  private panelLabel(kind: 'COMMIT' | 'PULL' | 'PUSH', count: number, repos: number): string {
    return repos > 1
      ? this.translate.instant(`GITFLOW.LABEL_${kind}_SPREAD`, { count, repos })
      : this.translate.instant(`GITFLOW.LABEL_${kind}`, { count });
  }

  // ---- apertura dei pannelli ----

  isPanelOpen(panel: GitPanel): boolean {
    return this.hoveredPanel === panel;
  }

  hoverPanel(panel: GitPanel): void {
    this.cancelPanelClose();
    // Rientrare nel pannello gia' aperto non lo rilegge: resta com'e' sotto il mouse.
    if (this.hoveredPanel === panel) return;
    this.hoveredPanel = panel;
    // Cio' che il pannello mostra si rilegge quando lo si apre. Per «da scaricare» serve anche
    // chiedere ai remoti dei submodule, che nessun altro interroga.
    if (panel === 'pull') this.refreshRemotes(false);
    else this.loadChangedFiles();
  }

  /**
   * Il pannello non si chiude nell'istante in cui il mouse esce: scendendo dal pulsante in
   * diagonale si passa per un attimo fuori, e il pannello spariva prima di arrivarci.
   */
  leavePanel(panel: GitPanel): void {
    if (this.hoveredPanel !== panel) return;
    this.cancelPanelClose();
    this.panelCloseTimer = setTimeout(() => {
      this.panelCloseTimer = null;
      if (this.hoveredPanel === panel) this.hoveredPanel = null;
    }, ToolbarComponent.PANEL_CLOSE_DELAY_MS);
  }

  private cancelPanelClose(): void {
    if (this.panelCloseTimer === null) return;
    clearTimeout(this.panelCloseTimer);
    this.panelCloseTimer = null;
  }

  private closePanels(): void {
    this.cancelPanelClose();
    this.hoveredPanel = null;
  }

  // ---- i remoti dei submodule ----

  /**
   * Chiamato a ogni giro di polling riuscito: interroga i remoti dei submodule appena si apre un
   * progetto, e poi ogni cinque minuti. Senza la seconda parte, chi tiene aperto MdExplorer tutto
   * il giorno non verrebbe mai a sapere che un submodule ha una versione nuova: il pulsante «da
   * pullare» non si accenderebbe, e senza pulsante il pannello non si puo' nemmeno aprire.
   */
  private askRemotesNowAndThen(): void {
    const projectPath = this.getProjectPath(true);
    if (!projectPath) return;
    const firstTime = this.remotesAskedFor !== projectPath;
    if (!firstTime && Date.now() - this.lastRemotesFetch < 5 * 60 * 1000) return;
    this.remotesAskedFor = projectPath;
    this.refreshRemotes(true);
  }

  /**
   * Chiede a ogni remoto cosa c'e' di nuovo, poi rilegge la vista. Non sta nel polling: sono
   * tanti processi git verso remoti diversi, e senza credenziale ognuno aprirebbe un login.
   * Parte all'apertura del progetto, quando si apre «da scaricare» e dopo ogni scaricamento.
   */
  refreshRemotes(force: boolean): void {
    const projectPath = this.getProjectPath(true);
    if (!projectPath || this.reviewAgent) { this.loadChangedFiles(); return; }
    // Senza una connessione buona al remoto del progetto non si prova nemmeno con gli altri.
    if (!this.connectionIsActive || this.authenticationMissing || this.authenticationFailed) {
      this.loadChangedFiles();
      return;
    }
    const now = Date.now();
    if (this.isFetchingRemotes || (!force && now - this.lastRemotesFetch < 30000)) {
      this.loadChangedFiles();
      return;
    }

    this.isFetchingRemotes = true;
    this.lastRemotesFetch = now;
    this.workingChanges.fetchAll(projectPath).subscribe({
      next: () => { this.isFetchingRemotes = false; this.loadChangedFiles(); },
      error: () => { this.isFetchingRemotes = false; this.loadChangedFiles(); },
    });
    // Dopo un'azione sui remoti si rilegge anche la sorgente.
    this.upstreamAskedFor = projectPath;
    this.lastUpstreamFetch = now;
    this.loadUpstream(true);
  }

  private upstreamAskedFor: string | null = null;
  private lastUpstreamFetch = 0;

  /**
   * Interroga la sorgente del progetto (remoto `upstream`) all'apertura e poi ogni cinque minuti. Non dipende
   * dallo stato della connessione a `origin`: sono due remoti diversi, e in un progetto demo `origin` è una
   * cartella sul disco mentre la sorgente è in rete. Legata a `origin`, bastava che quella connessione non
   * risultasse attiva perché gli aggiornamenti della sorgente non venissero mai cercati.
   */
  private askUpstreamNowAndThen(): void {
    const projectPath = this.getProjectPath(true);
    if (!projectPath || this.reviewAgent) return;
    const firstTime = this.upstreamAskedFor !== projectPath;
    if (!firstTime && Date.now() - this.lastUpstreamFetch < 5 * 60 * 1000) return;
    this.upstreamAskedFor = projectPath;
    this.lastUpstreamFetch = Date.now();
    this.loadUpstream(true);
  }

  // ---- le azioni per riga ----

  /** Pubblica UN repository. Il progetto resta spento finche' punta a un submodule non pubblicato. */
  pushRepo(repo: RepoChanges, event: MouseEvent): void {
    event.stopPropagation();
    if (repo.pushBlocker) return;   // il pulsante e' gia' disabilitato: qui e' solo una rete
    const projectPath = this.getProjectPath();
    if (!projectPath) return;
    this.runRepoAction('GITFLOW.PUSHING', repo.label,
      this.workingChanges.pushRepo(projectPath, this.reviewAgent, repo.path), false);
  }

  /** Sulla radice: scarica il progetto e allinea i submodule. Su un submodule: «Aggiorna all'ultima». */
  pullRepo(repo: RepoChanges, event: MouseEvent): void {
    event.stopPropagation();
    if (repo.pullBlocker) return;
    const projectPath = this.getProjectPath();
    if (!projectPath) return;
    this.runRepoAction(repo.depth === 0 ? 'GITFLOW.PULLING' : 'GITFLOW.UPDATING', repo.label,
      this.workingChanges.pullRepo(projectPath, repo.path, this.currentConnectionId()), true);
  }

  /** Porta un submodule alla versione che il progetto registra, solo in avanti. */
  alignRepo(repo: RepoChanges, event: MouseEvent): void {
    event.stopPropagation();
    if (repo.alignBlocker) return;
    const projectPath = this.getProjectPath();
    if (!projectPath) return;
    this.runRepoAction('GITFLOW.ALIGNING', repo.label,
      this.workingChanges.alignRepo(projectPath, repo.path, this.currentConnectionId()), true);
  }

  /** Scarica il progetto, se c'e' da scaricare, e allinea i submodule. Non cambia versione a nessuno. */
  pullEverything(): void {
    const projectPath = this.getProjectPath();
    if (!projectPath) return;
    this.runRepoAction('GITFLOW.PULLING_ALL', '',
      this.workingChanges.pullAll(projectPath, this.currentConnectionId()), true);
  }

  /** L'uscita da un'unione rimasta a meta': si torna a prima dello scaricamento. */
  abortMerge(repo: RepoChanges, event: MouseEvent): void {
    event.stopPropagation();
    const projectPath = this.getProjectPath();
    if (!projectPath) return;
    this.runRepoAction('GITFLOW.ABORTING_MERGE', repo.label,
      this.workingChanges.abortMerge(projectPath, repo.path, this.currentConnectionId()), true);
  }

  private currentConnectionId(): string {
    return this.connectionId || this.monitorMDService.connectionId || '';
  }

  /**
   * Esegue un'azione su un repository e racconta com'e' andata. Un rifiuto NON e' un errore da
   * nascondere: e' il motivo che la riga mostrava gia', ripetuto da chi ha rifiutato davvero.
   */
  private runRepoAction(waitingKey: string, repoLabel: string, action: Observable<RepoActionResult>, touchesRemote: boolean): void {
    const info = new WaitingDialogInfo();
    info.message = this.translate.instant(waitingKey, { repo: repoLabel });
    this.waitingDialogService.showMessageBox(info);

    const done = (result: RepoActionResult | null, fallback: string) => {
      this.waitingDialogService.closeMessageBox();
      this.reportRepoAction(result, fallback);
      // Prima i numeri locali, subito; poi cio' che dipende dal remoto.
      this.refreshLocalGitCounters();
      if (touchesRemote) this.refreshRemotes(true);
    };

    action.subscribe({
      next: result => done(result, ''),
      error: err => done(err?.error?.refused !== undefined ? err.error : null, err?.error?.error || err?.message || ''),
    });
  }

  private reportRepoAction(result: RepoActionResult | null, fallback: string): void {
    if (!result) {
      this._snackBar.open(this.translate.instant('GITFLOW.ACTION_FAILED', { error: fallback }), 'OK',
        { duration: 10000, verticalPosition: 'top', panelClass: ['error-snackbar'] });
      return;
    }
    if (result.refused) {
      this._snackBar.open(result.refused, 'OK', { duration: 14000, verticalPosition: 'top' });
      return;
    }
    const text = [result.message, ...(result.warnings || [])].filter(x => !!x).join(' ');
    this._snackBar.open(text, 'OK', {
      // Con un avviso da leggere resta finche' non lo si chiude: dice cosa NON e' stato spostato.
      duration: result.success && !(result.warnings || []).length ? 6000 : undefined,
      verticalPosition: 'top',
      panelClass: result.success ? [] : ['error-snackbar'],
    });
  }

  trackByRepo = (_: number, r: RepoChanges) => r.path;

  /**
   * Pubblica tutto, in un ordine che non puo' rompere il repository per gli altri: i submodule
   * prima, il progetto per ultimo. Se un submodule non e' pubblicabile ci si ferma PRIMA di
   * toccare qualsiasi remoto, e si dice perche'.
   */
  pushEverything(): void {
    const projectPath = this.getProjectPath();
    if (!projectPath) return;

    const info = new WaitingDialogInfo();
    info.message = this.translate.instant('TOOLBAR.PUSHING_ALL');
    this.waitingDialogService.showMessageBox(info);

    this.workingChanges.pushAll(projectPath, this.reviewAgent).subscribe({
      // Dopo il push si rileggono ANCHE i contatori della radice (refreshLocalGitCounters, che ricarica
      // pure la vista): rileggere solo la vista lasciava acceso «da pushare» con il numero di prima.
      next: result => {
        this.waitingDialogService.closeMessageBox();
        this.reportPush(result);
        this.refreshLocalGitCounters();
        this.refreshRemotes(true);
      },
      error: err => {
        this.waitingDialogService.closeMessageBox();
        const result: SafePushResult = err?.error?.refused !== undefined ? err.error : null;
        if (result) { this.reportPush(result); this.refreshLocalGitCounters(); return; }
        this._snackBar.open(
          this.translate.instant('TOOLBAR.PUSH_FAILED', { error: err?.message || '' }), 'OK',
          { duration: 8000, verticalPosition: 'top', panelClass: ['error-snackbar'] });
      },
    });
  }

  /**
   * Cosa dire dopo. Un rifiuto NON e' un errore da nascondere: e' una condizione che l'utente puo'
   * risolvere, e il messaggio dice gia' come.
   */
  private reportPush(result: SafePushResult): void {
    if (result.refused) {
      this._snackBar.open(result.refused, 'OK', { duration: 12000, verticalPosition: 'top' });
      return;
    }
    if (!result.success) {
      const failed = (result.steps || []).find(s => !s.ok);
      this._snackBar.open(
        this.translate.instant('TOOLBAR.PUSH_STOPPED', { repo: failed?.label || '?', why: failed?.outcome || '' }),
        'OK', { duration: 12000, verticalPosition: 'top', panelClass: ['error-snackbar'] });
      return;
    }
    // Cio' che resta indietro non e' un errore: e' roba non committata, che pubblicare non
    // porterebbe via. Ma senza dirlo si crede di aver pubblicato tutto.
    const left = result.leftBehind || [];
    const message = left.length
      ? this.translate.instant('TOOLBAR.PUSH_DONE_LEFT', { count: result.steps.length, left: left.join(', ') })
      : this.translate.instant('TOOLBAR.PUSH_DONE', { count: result.steps.length });
    this._snackBar.open(message, 'OK', { duration: 8000, verticalPosition: 'top' });
  }

  /** Dove sta questo repository sul disco: la radice del contesto, piu' il suo percorso. */
  repoAbsolutePath(repo: RepoChanges): string {
    return this.absolutePathIn(this.changesView?.rootPath || '', repo);
  }

  private absolutePathIn(root: string, repo: RepoChanges): string {
    if (!repo.path) return root;
    const sep = root.includes('\\') ? '\\' : '/';
    return root.replace(/[\\/]+$/, '') + sep + repo.path.replace(/\//g, sep);
  }

  /**
   * Committa in QUESTO repository. Nessun endpoint nuovo: `CommitAsync` prende un percorso e
   * committa li', e un submodule e' un repository git a tutti gli effetti.
   *
   * Dopo, la vista si rilegge — e il progetto padre si accende da solo, perche' committare nel
   * figlio ne sposta il puntatore. Non e' un effetto collaterale da nascondere: e' il lavoro
   * nuovo che si e' appena creato, ed e' il modo in cui si impara come funziona git.
   */
  commitRepo(repo: RepoChanges, event: MouseEvent): void {
    event.stopPropagation();
    if (repo.commitBlocker) return;   // il pulsante e' gia' disabilitato: qui e' solo una rete

    const target = this.repoAbsolutePath(repo);
    if (!target) return;

    const dialogRef = this.dialog.open(CommitMessageDialogComponent, {
      width: '500px',
      data: { defaultMessage: 'Update from MdExplorer', projectPath: target },
    });

    dialogRef.afterClosed().subscribe(commitMessage => {
      if (commitMessage === null || commitMessage === undefined) return;

      const info = new WaitingDialogInfo();
      info.message = 'Please wait... committing changes';
      this.waitingDialogService.showMessageBox(info);

      this.gitservice.modernCommit(target, commitMessage).subscribe(
        response => {
          // La ricarica della vista la fa gia' handleGitResponse (refreshLocalGitCounters).
          this.handleGitResponse(response, 'commit');
          this.waitingDialogService.closeMessageBox();
        },
        error => {
          this.waitingDialogService.closeMessageBox();
          const errorMessage = error.error?.errorMessage || error.message || '';
          this._snackBar.open(this.translate.instant('TOOLBAR.COMMIT_FAILED', { error: errorMessage }), 'OK', {
            duration: 5000, verticalPosition: 'top', panelClass: ['error-snackbar'],
          });
        }
      );
    });
  }

  /** Porta al tab delle differenze, dove si guarda file per file e si scarta. */
  seeTheDifferences(): void {
    this.closePanels();
    this.reviewContext.showChanges();
  }

  isTocDirectoryFile(): boolean {
    return this.currentMdFile?.name?.endsWith('.md.directory') || false;
  }

  /**
   * The panel shows a text file clicked in the md-tree (a .json, a .cs, …), not a markdown
   * document: Word export and the markdown editor do not apply to it.
   */
  isTextFileShown(): boolean {
    const name = this.currentMdFile?.name?.toLowerCase();
    return !!name && !name.endsWith('.md') && !name.endsWith('.md.directory');
  }

  refreshTocDirectory(): void {
    if (!this.currentMdFile || !this.isTocDirectoryFile()) {
      return;
    }

    // The .md.directory file is not a folder, so fullPath is a regular absolute
    // file path; its parent directory is the folder we need to regenerate.
    const fileFullPath = this.currentMdFile.fullPath || '';
    const lastSep = Math.max(fileFullPath.lastIndexOf('\\'), fileFullPath.lastIndexOf('/'));
    const folderFullPath = lastSep > 0 ? fileFullPath.substring(0, lastSep) : fileFullPath;

    // Display path for the progress dialog: relative form when available.
    let displayPath = this.currentMdFile.relativePath || '';
    if (displayPath.startsWith('\\')) {
      displayPath = displayPath.substring(1);
    }
    const lastSepRel = Math.max(displayPath.lastIndexOf('/'), displayPath.lastIndexOf('\\'));
    if (lastSepRel > 0) {
      displayPath = displayPath.substring(0, lastSepRel);
    }
    this.tocProgressService.showProgress(displayPath);

    this.tocService.generateToc(folderFullPath).subscribe({
      next: (result) => {
        // Progress dialog will be closed by SignalR event
        if (result.success) {
          this._snackBar.open(this.translate.instant('TOOLBAR.TOC_UPDATED'), 'OK', {
            duration: 3000,
            horizontalPosition: 'right',
            verticalPosition: 'bottom' 
          });
          // Reload the current file to show updated content
          this.mdFileService.setSelectedMdFileFromSideNav(this.currentMdFile);
        } else {
          this.tocProgressService.hideProgress();
          this._snackBar.open(this.translate.instant('TOOLBAR.TOC_UPDATE_FAILED'), 'OK', {
            duration: 5000,
            horizontalPosition: 'right',
            verticalPosition: 'bottom'
          });
        }
      },
      error: (err) => {
        console.error('Error refreshing TOC:', err);
        this.tocProgressService.hideProgress();
        this._snackBar.open(this.translate.instant('TOOLBAR.TOC_UPDATE_ERROR'), 'OK', {
          duration: 5000,
          horizontalPosition: 'right',
          verticalPosition: 'bottom'
        });
      }
    });
  }
}
