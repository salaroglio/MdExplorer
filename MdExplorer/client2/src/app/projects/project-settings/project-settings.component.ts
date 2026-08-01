import { Component, Inject, OnInit, OnDestroy } from '@angular/core';
import { MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA, MatLegacyDialogRef as MatDialogRef, MatLegacyDialog as MatDialog } from '@angular/material/legacy-dialog';
import { Subscription } from 'rxjs';
import { ProjectSettingsService } from '../services/project-settings.service';
import { CompatibilityModeService } from '../../services/compatibility-mode.service';
import { IdeConfigurationService } from '../services/ide-configuration.service';
import { MdServerMessagesService } from '../../signalR/services/server-messages.service';
import { ProjectsService } from '../../md-explorer/services/projects.service';
import { ExternalAppsService, MdeAppDefinition } from '../../md-explorer/services/external-apps.service';
import { MdeTreeNode, MdeAppsConfig } from '../../md-explorer/models/mde-apps-tree.models';
import { MdFileService } from '../../md-explorer/services/md-file.service';
import { CatalogPickerDialogComponent } from '../dialogs/catalog-picker/catalog-picker.component';
import { StoreCatalogApp } from '../../md-explorer/models/app-store.models';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-project-settings',
  templateUrl: './project-settings.component.html',
  styleUrls: ['./project-settings.component.scss']
})
export class ProjectSettingsComponent implements OnInit, OnDestroy {
  rule1Enabled: boolean = false;
  linkIndexingEnabled: boolean = true;
  plantUmlKeepOriginalColorsEnabled: boolean = false;
  copilotCliAutoSelectEnabled: boolean = true;
  excludeSubmodulesEnabled: boolean = true;

  // Agent City / Federation (§12.4) — activation lives in .development.yml (shared via git).
  agentCityEnabled: boolean = false;
  agentCityOwnershipDoc: string = '';
  agentCityHasRoomSecret: boolean = false;
  agentUseWorktrees: boolean = false;
  agentAutoMergeDeliverables: boolean = false;
  /** Senza git non esistono worktree né merge: le due opzioni restano spente e non toccabili. */
  projectIsGit: boolean = false;

  // Relay della federazione (per progetto). La chiave non arriva mai dal server:
  // relayApiKey è solo il campo di INSERIMENTO, vuoto se non si sta cambiando nulla.
  relayUrl: string = '';
  relayUrlSource: string = 'None';
  relayHasApiKey: boolean = false;
  relayApiKeySource: string = 'None';
  relayApiKey: string = '';
  relayTesting: boolean = false;
  relayTestMessage: string = '';
  relayTestSuccess: boolean | null = null;
  indexAllTextFilesEnabled: boolean = false;
  textFileExtensions: string = '';
  textFileExtensionsDefault: string = '';
  reindexingText: boolean = false;
  githubModeEnabled: boolean = false;
  stickyScrollEnabled: boolean = true;
  selectedIde: string = 'vscode';
  private lastSavedIde: string = 'vscode';
  vscodePath: string = '';
  intellijPath: string = '';
  projectId: string;
  projectName: string;
  projectPath: string;
  loading: boolean = false;
  saving: boolean = false;

  // Apps tab
  appsTree: MdeTreeNode[] = [];
  appDefinitions: MdeAppDefinition[] = [];
  appsLoading = true;
  appsSaving = false;
  editingCategoryId: string | null = null;
  editingCategoryName = '';
  appsSavedMessage = '';

  // Knowledge Graph (Neo4j) settings
  kgLoading: boolean = false;
  kgSaving: boolean = false;
  kgEnabled: boolean = false;
  kgUri: string = 'bolt://localhost:7687';
  kgDatabase: string = 'neo4j';
  kgUsername: string = 'neo4j';
  kgPassword: string = '';
  kgHasPassword: boolean = false;
  kgSyncOnTocGeneration: boolean = true;
  kgSyncOnKgFileSave: boolean = true;
  kgTesting: boolean = false;
  kgTestMessage: string = '';
  kgTestSuccess: boolean = false;
  kgSyncing: boolean = false;
  kgResetting: boolean = false;
  kgSyncMessage: string = '';
  kgSyncErrors: any[] = [];
  kgStateLoaded: boolean = false;
  kgStateTotals: any = null;
  kgPerNamespace: any[] = [];

  // Apache Jena Fuseki settings (specchio del blocco Neo4j)
  fsLoading: boolean = false;
  fsSaving: boolean = false;
  fsEnabled: boolean = false;
  fsUri: string = 'http://localhost:3030';
  fsDataset: string = '';           // default dal nome progetto sanitizzato, popolato dal GET
  fsDefaultDataset: string = '';    // suggerimento del server
  fsUsername: string = '';
  fsPassword: string = '';
  fsHasPassword: boolean = false;
  fsSyncOnTocGeneration: boolean = true;
  fsSyncOnKgFileSave: boolean = true;
  fsTesting: boolean = false;
  fsTestMessage: string = '';
  fsTestSuccess: boolean = false;

  // Atlassian (Jira/Confluence) settings
  // Shared (jiraBaseUrl/projectKeys/confluence) -> .development.yml.
  // Token -> UserDB encrypted. Email lives in UserDB (personal).
  atlLoading: boolean = false;
  atlSaving: boolean = false;
  atlEnabled: boolean = false;
  atlBaseUrl: string = '';
  atlProjectKeys: string = '';     // comma-separated in the UI, split on save
  // Confluence shares the Atlassian site & token. Base URL is derived as
  // {jiraBaseUrl}/wiki; the override is only for the rare different-site case.
  atlConfluenceBaseUrl: string = '';          // optional override (empty = derived)
  atlConfluenceBaseUrlEffective: string = '';  // read-only, what will actually be used
  atlConfluenceSpaceKeys: string = '';         // comma-separated in the UI
  atlEmail: string = '';
  atlToken: string = '';
  atlHasToken: boolean = false;
  atlTesting: boolean = false;
  atlTestMessage: string = '';
  atlTestSuccess: boolean = false;

  // RAG settings
  ragEnabled: boolean = false;
  ragModelInstalled: boolean = false;
  ragModelLoaded: boolean = false;
  ragChunksCount: number = 0;
  ragEmbeddedCount: number = 0;
  ragReindexing: boolean = false;
  ragMessage: string = '';
  ragProgress: number = 0;
  ragTotal: number = 0;
  ragProcessed: number = 0;
  private ragProgressSub: Subscription;

  // Full project reindex (incremental indexing escape hatch)
  projectReindexing: boolean = false;
  private projectReindexSub: Subscription;

  constructor(
    public dialogRef: MatDialogRef<ProjectSettingsComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private projectSettingsService: ProjectSettingsService,
    private compatibilityService: CompatibilityModeService,
    private ideConfigService: IdeConfigurationService,
    private serverMessages: MdServerMessagesService,
    private projectsService: ProjectsService,
    private externalAppsService: ExternalAppsService,
    private mdFileService: MdFileService,
    private dialog: MatDialog,
    private translate: TranslateService
  ) {
    this.projectId = data.projectId;
    this.projectName = data.projectName;
    this.projectPath = data.projectPath;
  }

  ngOnInit(): void {
    this.loadSettings();
    this.loadAppsConfig();
    this.loadKgSettings();
    this.loadFusekiSettings();
    this.loadAtlassianSettings();
    this.loadAgentCity();

    this.ragProgressSub = this.serverMessages.ragIndexingProgress$.subscribe(data => {
      this.ragProcessed = data.processed;
      this.ragTotal = data.total;
      this.ragProgress = data.total > 0 ? (data.processed / data.total) * 100 : 0;
      this.ragMessage = data.message;

      if (data.status === 'completed' || data.status === 'error') {
        this.ragReindexing = false;
        this.refreshRagStatus();
      }
    });

    // Full project reindex: progress shows in the global indexing snackbar
    // (parsingProjectStart/knowledgeProgress); here we only track the button state.
    this.projectReindexSub = this.serverMessages.parsingProjectStop$.subscribe(() => {
      this.projectReindexing = false;
    });
  }

  ngOnDestroy(): void {
    this.ragProgressSub?.unsubscribe();
    this.projectReindexSub?.unsubscribe();
  }

  onProjectReindex(): void {
    this.projectReindexing = true;
    this.projectSettingsService.reindexProject(this.serverMessages.connectionId ?? '').subscribe({
      next: () => {
        console.log('[ProjectSettings] Full project reindex started');
      },
      error: (error) => {
        console.error('[ProjectSettings] Error starting project reindex:', error);
        this.projectReindexing = false;
      }
    });
  }

  loadSettings(): void {
    this.loading = true;
    let rule1Loaded = false;
    let linkIndexingLoaded = false;
    let compatibilityLoaded = false;
    let ideConfigLoaded = false;
    let ragLoaded = false;
    let stickyScrollLoaded = false;
    let plantUmlKeepOriginalColorsLoaded = false;
    let copilotCliAutoSelectLoaded = false;
    let excludeSubmodulesLoaded = false;
    let textIndexingLoaded = false;

    const checkIfDone = () => {
      if (rule1Loaded && linkIndexingLoaded && compatibilityLoaded && ideConfigLoaded && ragLoaded && stickyScrollLoaded && plantUmlKeepOriginalColorsLoaded && copilotCliAutoSelectLoaded && excludeSubmodulesLoaded && textIndexingLoaded) {
        this.loading = false;
      }
    };

    // Load Rule 1 setting
    this.projectSettingsService.getRule1Setting().subscribe({
      next: (response) => {
        this.rule1Enabled = response.enabled;
        rule1Loaded = true;
        checkIfDone();
      },
      error: (error) => {
        console.error('Error loading Rule 1 setting:', error);
        rule1Loaded = true;
        checkIfDone();
      }
    });

    // Load Link Indexing setting
    this.projectSettingsService.getLinkIndexingSetting(this.projectPath).subscribe({
      next: (response) => {
        this.linkIndexingEnabled = response.enabled;
        linkIndexingLoaded = true;
        checkIfDone();
      },
      error: (error) => {
        console.error('Error loading Link Indexing setting:', error);
        linkIndexingLoaded = true;
        checkIfDone();
      }
    });

    // Load PlantUML Keep Original Colors setting
    this.projectSettingsService.getPlantUmlKeepOriginalColorsSetting(this.projectPath).subscribe({
      next: (response) => {
        this.plantUmlKeepOriginalColorsEnabled = response.enabled;
        plantUmlKeepOriginalColorsLoaded = true;
        checkIfDone();
      },
      error: (error) => {
        console.error('Error loading PlantUML Keep Original Colors setting:', error);
        plantUmlKeepOriginalColorsLoaded = true;
        checkIfDone();
      }
    });

    // Load Copilot CLI Auto-Select setting
    this.projectSettingsService.getCopilotCliAutoSelectSetting(this.projectPath).subscribe({
      next: (response) => {
        this.copilotCliAutoSelectEnabled = response.enabled;
        copilotCliAutoSelectLoaded = true;
        checkIfDone();
      },
      error: (error) => {
        console.error('Error loading Copilot CLI Auto-Select setting:', error);
        copilotCliAutoSelectLoaded = true;
        checkIfDone();
      }
    });

    // Load Exclude Git Submodules setting
    this.projectSettingsService.getExcludeSubmodulesSetting(this.projectPath).subscribe({
      next: (response) => {
        this.excludeSubmodulesEnabled = response.enabled;
        excludeSubmodulesLoaded = true;
        checkIfDone();
      },
      error: (error) => {
        console.error('Error loading Exclude Submodules setting:', error);
        excludeSubmodulesLoaded = true;
        checkIfDone();
      }
    });

    // Load Text Indexing (non-markdown text files) setting
    this.projectSettingsService.getTextIndexingSetting(this.projectPath).subscribe({
      next: (response) => {
        this.indexAllTextFilesEnabled = response.enabled;
        this.textFileExtensions = response.extensions || '';
        this.textFileExtensionsDefault = response.defaultExtensions || '';
        textIndexingLoaded = true;
        checkIfDone();
      },
      error: (error) => {
        console.error('Error loading Text Indexing setting:', error);
        textIndexingLoaded = true;
        checkIfDone();
      }
    });

    // Load compatibility mode for this specific project
    this.compatibilityService.getCurrentMode(this.projectPath).subscribe({
      next: (response) => {
        console.log('Compatibility mode loaded for project:', this.projectPath, response);
        this.githubModeEnabled = response.mode === 'github';
        compatibilityLoaded = true;
        checkIfDone();
      },
      error: (error) => {
        console.error('Error loading compatibility mode:', error);
        compatibilityLoaded = true;
        checkIfDone();
      }
    });

    // Load IDE configuration for this specific project
    this.ideConfigService.getIdeConfiguration(this.projectPath).subscribe({
      next: (response) => {
        console.log('IDE configuration loaded for project:', this.projectPath, response);
        this.selectedIde = response.selectedIde || 'vscode';
        this.lastSavedIde = this.selectedIde;
        this.vscodePath = response.vscodePath || '';
        this.intellijPath = response.intellijPath || '';
        ideConfigLoaded = true;
        checkIfDone();
      },
      error: (error) => {
        console.error('Error loading IDE configuration:', error);
        ideConfigLoaded = true;
        checkIfDone();
      }
    });

    // Load Sticky Scroll setting
    this.projectSettingsService.getStickyScrollSetting().subscribe({
      next: (response) => {
        this.stickyScrollEnabled = response.enabled;
        stickyScrollLoaded = true;
        checkIfDone();
      },
      error: (error) => {
        console.error('Error loading Sticky Scroll setting:', error);
        stickyScrollLoaded = true;
        checkIfDone();
      }
    });

    // Load RAG status
    this.projectSettingsService.getRagStatus().subscribe({
      next: (response) => {
        this.ragEnabled = response.enabled;
        this.ragModelInstalled = response.modelInstalled;
        this.ragModelLoaded = response.modelLoaded;
        this.ragChunksCount = response.chunksCount || 0;
        this.ragEmbeddedCount = response.embeddedCount || 0;

        // Recover indexing state if reindex is in progress
        if (response.isIndexing && response.indexingProgress) {
          this.ragReindexing = true;
          this.ragProcessed = response.indexingProgress.processed || 0;
          this.ragTotal = response.indexingProgress.total || 0;
          this.ragProgress = this.ragTotal > 0 ? (this.ragProcessed / this.ragTotal) * 100 : 0;
          this.ragMessage = response.indexingProgress.message || this.translate.instant('PROJECT_SETTINGS.INDEXING');
        }

        ragLoaded = true;
        checkIfDone();
      },
      error: (error) => {
        console.error('Error loading RAG status:', error);
        ragLoaded = true;
        checkIfDone();
      }
    });
  }

  onRule1Change(): void {
    this.saving = true;
    this.projectSettingsService.setRule1Setting(this.rule1Enabled).subscribe({
      next: () => {
        console.log('Rule 1 setting saved successfully');
        this.saving = false;
      },
      error: (error) => {
        console.error('Error saving Rule 1 setting:', error);
        this.saving = false;
        // Revert the change on error
        this.rule1Enabled = !this.rule1Enabled;
      }
    });
  }

  onStickyScrollChange(): void {
    this.saving = true;
    this.projectSettingsService.setStickyScrollSetting(this.stickyScrollEnabled).subscribe({
      next: () => {
        console.log('Sticky Scroll setting saved successfully');
        this.saving = false;
      },
      error: (error) => {
        console.error('Error saving Sticky Scroll setting:', error);
        this.saving = false;
        this.stickyScrollEnabled = !this.stickyScrollEnabled;
      }
    });
  }

  onLinkIndexingChange(): void {
    this.saving = true;
    this.projectSettingsService.setLinkIndexingSetting(this.linkIndexingEnabled, this.projectPath).subscribe({
      next: () => {
        console.log('Link Indexing setting saved successfully');
        this.saving = false;
      },
      error: (error) => {
        console.error('Error saving Link Indexing setting:', error);
        this.saving = false;
        this.linkIndexingEnabled = !this.linkIndexingEnabled;
      }
    });
  }

  onPlantUmlKeepOriginalColorsChange(): void {
    this.saving = true;
    this.projectSettingsService.setPlantUmlKeepOriginalColorsSetting(this.plantUmlKeepOriginalColorsEnabled, this.projectPath).subscribe({
      next: () => {
        document.body.classList.toggle('plantuml-keep-original', this.plantUmlKeepOriginalColorsEnabled);
        this.saving = false;
      },
      error: (error) => {
        console.error('Error saving PlantUML Keep Original Colors setting:', error);
        this.saving = false;
        this.plantUmlKeepOriginalColorsEnabled = !this.plantUmlKeepOriginalColorsEnabled;
      }
    });
  }

  onCopilotCliAutoSelectChange(): void {
    this.saving = true;
    this.projectSettingsService.setCopilotCliAutoSelectSetting(this.copilotCliAutoSelectEnabled, this.projectPath).subscribe({
      next: () => {
        this.saving = false;
      },
      error: (error) => {
        console.error('Error saving Copilot CLI Auto-Select setting:', error);
        this.saving = false;
        this.copilotCliAutoSelectEnabled = !this.copilotCliAutoSelectEnabled;
      }
    });
  }

  onExcludeSubmodulesChange(): void {
    this.saving = true;
    this.projectSettingsService.setExcludeSubmodulesSetting(this.excludeSubmodulesEnabled, this.projectPath).subscribe({
      next: () => {
        this.saving = false;
      },
      error: (error) => {
        console.error('Error saving Exclude Submodules setting:', error);
        this.saving = false;
        this.excludeSubmodulesEnabled = !this.excludeSubmodulesEnabled;
      }
    });
  }

  loadAgentCity(): void {
    if (!this.projectPath) return;
    this.projectSettingsService.getAgentCity(this.projectPath).subscribe({
      next: (res) => {
        this.agentCityEnabled = !!res?.enabled;
        this.agentCityOwnershipDoc = res?.ownershipDoc || '';
        this.agentCityHasRoomSecret = !!res?.hasRoomSecret;
        this.agentUseWorktrees = !!res?.useAgentWorktrees;
        this.agentAutoMergeDeliverables = !!res?.autoMergeAgentDeliverables;
        this.projectIsGit = !!(res as any)?.isGitRepository;
      },
      error: (err) => console.error('Error loading Agent City settings:', err)
    });
    this.loadRelaySettings();
  }

  loadRelaySettings(): void {
    if (!this.projectPath) return;
    this.projectSettingsService.getRelaySettings(this.projectPath).subscribe({
      next: (res) => {
        this.relayUrl = res?.relayUrl || '';
        this.relayUrlSource = res?.relayUrlSource || 'None';
        this.relayHasApiKey = !!res?.hasApiKey;
        this.relayApiKeySource = res?.apiKeySource || 'None';
        this.relayTestSuccess = res?.lastTestSuccess ?? null;
      },
      error: (err) => console.error('Error loading relay settings:', err)
    });
  }

  onRelaySettingsChange(): void {
    if (!this.projectPath) return;
    this.saving = true;
    this.projectSettingsService.setRelaySettings(this.projectPath, {
      relayUrl: this.relayUrl?.trim() || undefined,
      // Campo vuoto = "non sto cambiando la chiave": il server la lascia com'è.
      apiKey: this.relayApiKey?.trim() || undefined,
    }).subscribe({
      next: (res) => {
        this.saving = false;
        this.relayApiKey = '';            // mai tenere il segreto nel campo dopo il salvataggio
        this.relayUrl = res?.relayUrl || '';
        this.relayUrlSource = res?.relayUrlSource || 'None';
        this.relayHasApiKey = !!res?.hasApiKey;
        this.relayApiKeySource = res?.apiKeySource || 'None';
        this.relayTestMessage = '';
        this.relayTestSuccess = null;
      },
      error: (err) => {
        console.error('Error saving relay settings:', err);
        this.saving = false;
        this.relayTestSuccess = false;
        this.relayTestMessage = err?.error?.message || 'Salvataggio fallito.';
        this.loadRelaySettings();          // riallinea allo stato persistito
      }
    });
  }

  clearRelayApiKey(): void {
    if (!this.projectPath) return;
    this.saving = true;
    this.projectSettingsService.setRelaySettings(this.projectPath, {
      relayUrl: this.relayUrl?.trim() || undefined,
      clearApiKey: true,
    }).subscribe({
      next: (res) => {
        this.saving = false;
        this.relayApiKey = '';
        this.relayHasApiKey = !!res?.hasApiKey;
        this.relayApiKeySource = res?.apiKeySource || 'None';
        this.relayTestMessage = '';
        this.relayTestSuccess = null;
      },
      error: (err) => {
        console.error('Error clearing relay api key:', err);
        this.saving = false;
        this.loadRelaySettings();
      }
    });
  }

  testRelay(): void {
    if (!this.projectPath) return;
    this.relayTesting = true;
    this.relayTestMessage = '';
    this.projectSettingsService.testRelaySettings(this.projectPath).subscribe({
      next: (res) => {
        this.relayTesting = false;
        this.relayTestSuccess = !!res?.success;
        this.relayTestMessage = res?.message || '';
      },
      error: (err) => {
        this.relayTesting = false;
        this.relayTestSuccess = false;
        this.relayTestMessage = err?.error?.message || 'Verifica fallita.';
      }
    });
  }

  onAgentCityChange(): void {
    this.saving = true;
    this.projectSettingsService.setAgentCity(this.projectPath, {
      enabled: this.agentCityEnabled,
      ownershipDoc: this.agentCityOwnershipDoc?.trim() || undefined,
      // Inviati sempre espliciti: il server preserva i valori solo quando il campo è assente,
      // quindi mandarli evita ambiguità fra "non deciso" e "spento apposta".
      useAgentWorktrees: this.agentUseWorktrees,
      autoMergeAgentDeliverables: this.agentAutoMergeDeliverables,
    }).subscribe({
      next: (res) => {
        this.saving = false;
        this.agentCityHasRoomSecret = !!res?.hasRoomSecret;
        this.agentCityOwnershipDoc = res?.ownershipDoc || '';
        this.agentUseWorktrees = !!res?.useAgentWorktrees;
        this.agentAutoMergeDeliverables = !!res?.autoMergeAgentDeliverables;
      },
      error: (err) => {
        console.error('Error saving Agent City settings:', err);
        this.saving = false;
        // Il salvataggio è fallito: riallinea la UI allo stato PERSISTITO rileggendolo
        // dal server. Niente inversione cieca del checkbox: questo handler è agganciato
        // anche al blur del campo ownership-doc, e lì il toggle non è stato toccato.
        this.loadAgentCity();
      }
    });
  }

  onTextIndexingChange(): void {
    this.saving = true;
    this.projectSettingsService.setTextIndexingSetting(
      this.indexAllTextFilesEnabled,
      this.textFileExtensions,
      this.projectPath
    ).subscribe({
      next: () => {
        this.saving = false;
      },
      error: (error) => {
        console.error('Error saving Text Indexing setting:', error);
        this.saving = false;
        this.indexAllTextFilesEnabled = !this.indexAllTextFilesEnabled;
      }
    });
  }

  onReindexTextFiles(): void {
    this.reindexingText = true;
    this.projectSettingsService.reindexTextFiles(this.serverMessages.connectionId ?? '').subscribe({
      next: () => {
        // Fire-and-forget on the backend; the index rebuilds in the background.
        setTimeout(() => { this.reindexingText = false; }, 1500);
      },
      error: (error) => {
        console.error('Error triggering text reindex:', error);
        this.reindexingText = false;
      }
    });
  }

  onGitHubModeChange(): void {
    console.log('onGitHubModeChange called, githubModeEnabled:', this.githubModeEnabled);
    this.saving = true;
    const mode = this.githubModeEnabled ? 'github' : 'mdexplorer';

    console.log('Setting compatibility mode to:', mode, 'for project:', this.projectPath);
    this.compatibilityService.setCompatibilityMode({
      mode,
      githubOptions: {
        embedImages: false,
        stripInteractive: true,
        preserveEmoji: true
      },
      projectPath: this.projectPath
    }).subscribe({
      next: (response) => {
        console.log('Compatibility mode saved successfully:', mode, response);
        this.saving = false;
      },
      error: (error) => {
        console.error('Error saving compatibility mode:', error);
        this.saving = false;
        // Revert the change on error
        this.githubModeEnabled = !this.githubModeEnabled;
      }
    });
  }

  onIdeChange(): void {
    console.log('onIdeChange called, selectedIde:', this.selectedIde);
    this.saving = true;

    this.ideConfigService.setIdeConfiguration({
      selectedIde: this.selectedIde,
      projectPath: this.projectPath
    }).subscribe({
      next: (response) => {
        console.log('IDE configuration saved successfully:', this.selectedIde, response);
        this.lastSavedIde = this.selectedIde;
        this.saving = false;
      },
      error: (error) => {
        console.error('Error saving IDE configuration:', error);
        this.saving = false;
        // Revert to the last successfully-saved value (works for any number of options)
        this.selectedIde = this.lastSavedIde;
      }
    });
  }

  onRagChange(): void {
    this.saving = true;
    const action$ = this.ragEnabled
      ? this.projectSettingsService.enableRag()
      : this.projectSettingsService.disableRag();

    action$.subscribe({
      next: () => {
        console.log('RAG setting saved:', this.ragEnabled);
        this.saving = false;
        this.projectsService.ragEnabled$.next(this.ragEnabled);
        this.refreshRagStatus();
      },
      error: (error) => {
        console.error('Error saving RAG setting:', error);
        this.saving = false;
        this.ragEnabled = !this.ragEnabled;
      }
    });
  }

  onRagReindex(): void {
    this.ragReindexing = true;
    this.ragProgress = 0;
    this.ragProcessed = 0;
    this.ragTotal = 0;
    this.ragMessage = this.translate.instant('PROJECT_SETTINGS.STARTING_INDEXING');
    this.projectSettingsService.reindexRag(this.projectPath).subscribe({
      next: (response: any) => {
        console.log('RAG reindex started:', response);
        // Progress updates will arrive via SignalR
      },
      error: (error) => {
        console.error('Error triggering RAG reindex:', error);
        this.ragReindexing = false;
        this.ragMessage = this.translate.instant('PROJECT_SETTINGS.ERROR_STARTING_INDEXING');
      }
    });
  }

  onRagClear(): void {
    if (!confirm(this.translate.instant('PROJECT_SETTINGS.DELETE_RAG_CONFIRM'))) {
      return;
    }
    this.saving = true;
    this.projectSettingsService.clearRagIndex(this.projectPath).subscribe({
      next: (response: any) => {
        console.log('RAG index cleared:', response);
        this.saving = false;
        this.ragMessage = this.translate.instant('PROJECT_SETTINGS.CLEARED_CHUNKS', { count: response.chunksDeleted });
        this.refreshRagStatus();
      },
      error: (error) => {
        console.error('Error clearing RAG index:', error);
        this.saving = false;
        this.ragMessage = this.translate.instant('PROJECT_SETTINGS.ERROR_CLEARING_INDEX');
      }
    });
  }

  private refreshRagStatus(): void {
    this.projectSettingsService.getRagStatus().subscribe({
      next: (response) => {
        this.ragEnabled = response.enabled;
        this.ragModelInstalled = response.modelInstalled;
        this.ragModelLoaded = response.modelLoaded;
        this.ragChunksCount = response.chunksCount || 0;
        this.ragEmbeddedCount = response.embeddedCount || 0;
      }
    });
  }

  // ── Apps tab ──────────────────────────────

  loadAppsConfig(): void {
    this.appsLoading = true;
    console.log('[ProjectSettings] loadAppsConfig — projectPath:', this.projectPath);
    this.externalAppsService.getConfig(this.projectPath).subscribe({
      next: (config) => {
        console.log('[ProjectSettings] apps config loaded:', config);
        this.appDefinitions = config.apps || [];
        this.appsTree = config.tree || [];
        this.appsLoading = false;
        console.log('[ProjectSettings] appsTree:', this.appsTree.length, 'appDefinitions:', this.appDefinitions.length);
      },
      error: (err) => {
        console.error('[ProjectSettings] Error loading apps config:', err);
        this.appDefinitions = [];
        this.appsTree = [];
        this.appsLoading = false;
      }
    });
  }

  getAppName(appId: string): string {
    const app = this.appDefinitions.find(a => a.id === appId);
    return app?.name || appId;
  }

  getAppIcon(appId: string): string {
    const app = this.appDefinitions.find(a => a.id === appId);
    return app?.icon || 'apps';
  }

  getCategories(): MdeTreeNode[] {
    return this.appsTree.filter(n => n.type === 'category');
  }

  openCatalogPicker(): void {
    const existingIds = this.appDefinitions.map(a => a.id);
    const ref = this.dialog.open(CatalogPickerDialogComponent, {
      width: '560px',
      data: { existingAppIds: existingIds }
    });

    ref.afterClosed().subscribe((selected: StoreCatalogApp | null) => {
      if (!selected) return;

      // Convert StoreCatalogApp to MdeAppDefinition
      const newApp: MdeAppDefinition = {
        id: selected.id,
        name: selected.name,
        description: selected.description || '',
        icon: selected.icon || 'apps',
        executable: '',
        args: selected.defaultArgs || [],
        singleton: true
      };

      this.appDefinitions.push(newApp);
      this.appsTree.push({ type: 'app', appId: selected.id });

      // Save full config (apps + tree) in one call
      this.saveAppsTree();
    });
  }

  addCategory(): void {
    const name = prompt(this.translate.instant('PROJECT_SETTINGS.CATEGORY_NAME_PROMPT'));
    if (!name || !name.trim()) return;

    const cat: MdeTreeNode = {
      type: 'category',
      id: 'cat-' + Date.now(),
      name: name.trim(),
      icon: 'folder',
      children: []
    };
    this.appsTree.push(cat);
    this.saveAppsTree();
  }

  deleteCategory(cat: MdeTreeNode): void {
    // Move children to root level
    if (cat.children && cat.children.length > 0) {
      for (const child of cat.children) {
        this.appsTree.push(child);
      }
    }
    this.appsTree = this.appsTree.filter(n => n !== cat);
    this.saveAppsTree();
  }

  startRenameCategory(cat: MdeTreeNode): void {
    this.editingCategoryId = cat.id!;
    this.editingCategoryName = cat.name || '';
  }

  confirmRenameCategory(cat: MdeTreeNode): void {
    if (this.editingCategoryName.trim()) {
      cat.name = this.editingCategoryName.trim();
      this.saveAppsTree();
    }
    this.editingCategoryId = null;
  }

  cancelRenameCategory(): void {
    this.editingCategoryId = null;
  }

  moveAppToCategory(appNode: MdeTreeNode, targetCat: MdeTreeNode): void {
    // Remove from root
    this.appsTree = this.appsTree.filter(n => n !== appNode);
    // Remove from any other category
    for (const cat of this.getCategories()) {
      if (cat.children) {
        cat.children = cat.children.filter(c => c.appId !== appNode.appId);
      }
    }
    // Add to target
    if (!targetCat.children) targetCat.children = [];
    targetCat.children.push({ type: 'app', appId: appNode.appId });
    this.saveAppsTree();
  }

  moveAppToRoot(cat: MdeTreeNode, childNode: MdeTreeNode): void {
    if (cat.children) {
      cat.children = cat.children.filter(c => c !== childNode);
    }
    this.appsTree.push({ type: 'app', appId: childNode.appId });
    this.saveAppsTree();
  }

  removeApp(appId: string): void {
    const appName = this.getAppName(appId);
    if (!confirm(this.translate.instant('PROJECT_SETTINGS.REMOVE_APP_CONFIRM', { name: appName }))) return;

    // Remove from tree (root + categories)
    this.appsTree = this.appsTree.filter(n => !(n.type === 'app' && n.appId === appId));
    for (const cat of this.getCategories()) {
      if (cat.children) {
        cat.children = cat.children.filter(c => c.appId !== appId);
      }
    }
    // Remove from definitions
    this.appDefinitions = this.appDefinitions.filter(a => a.id !== appId);

    // Save full config (apps + tree) in one call
    this.saveAppsTree();
  }

  saveAppsTree(): void {
    this.appsSaving = true;
    this.appsSavedMessage = '';
    const config: MdeAppsConfig = {
      version: '2',
      apps: this.appDefinitions,
      tree: this.appsTree
    };
    this.externalAppsService.saveConfig(config, this.projectPath).subscribe({
      next: () => {
        this.appsSaving = false;
        this.appsSavedMessage = this.translate.instant('PROJECT_SETTINGS.SAVED');
        this.refreshTree();
        setTimeout(() => this.appsSavedMessage = '', 3000);
      },
      error: (err) => {
        console.error('Error saving config:', err);
        this.appsSaving = false;
        this.appsSavedMessage = this.translate.instant('PROJECT_SETTINGS.ERROR_SAVING');
      }
    });
  }

  private refreshTree(): void {
    // Refresh md-tree solo se siamo dentro un progetto aperto (connectionId registrato)
    if (this.serverMessages.connectionId) {
      this.mdFileService.loadAll(null, null);
    }
  }

  close(): void {
    this.dialogRef.close();
  }

  // ============================================================
  //   Knowledge Graph (Neo4j) — tab logic
  // ============================================================
  loadKgSettings(): void {
    if (!this.projectId) return;
    this.kgLoading = true;
    this.projectSettingsService.getKgSettings(this.projectId).subscribe({
      next: (r) => {
        this.kgEnabled = !!r.enabled;
        this.kgUri = r.uri || 'bolt://localhost:7687';
        this.kgDatabase = r.database || 'neo4j';
        this.kgUsername = r.username || 'neo4j';
        this.kgHasPassword = !!r.hasPassword;
        this.kgPassword = '';
        this.kgSyncOnTocGeneration = r.syncOnTocGeneration !== false;
        this.kgSyncOnKgFileSave = r.syncOnKgFileSave !== false;
        if (r.lastTestSuccess === true) {
          this.kgTestMessage = this.translate.instant('PROJECT_SETTINGS.KG_TEST_OK_PREVIOUS');
          this.kgTestSuccess = true;
        } else if (r.lastTestSuccess === false) {
          this.kgTestMessage = this.translate.instant('PROJECT_SETTINGS.KG_TEST_FAIL_PREVIOUS');
          this.kgTestSuccess = false;
        }
        this.kgLoading = false;
        if (this.kgEnabled && this.kgHasPassword) {
          this.loadKgState();
        }
      },
      error: (err) => {
        console.error('Error loading KG settings:', err);
        this.kgLoading = false;
      }
    });
  }

  onKgEnabledChange(): void {
    // No immediate save — user must click Save to apply (so password and other fields go together).
    // If user just turned it off, persist immediately (no other state to gather).
    if (!this.kgEnabled) {
      this.onSaveKgSettings();
    }
  }

  onTestKgConnection(): void {
    this.kgTesting = true;
    this.kgTestMessage = '';
    this.projectSettingsService.testKgConnection({
      projectId: this.projectId,
      uri: this.kgUri,
      database: this.kgDatabase,
      username: this.kgUsername,
      password: this.kgPassword || (this.kgHasPassword ? '********' : '')
    }).subscribe({
      next: (r) => {
        this.kgTesting = false;
        this.kgTestSuccess = !!r.success;
        if (r.success) {
          this.kgTestMessage = this.translate.instant('PROJECT_SETTINGS.KG_TEST_OK', { ms: r.latencyMs });
        } else {
          this.kgTestMessage = this.translate.instant('PROJECT_SETTINGS.KG_TEST_FAIL', { error: r.error || 'unknown' });
        }
      },
      error: (err) => {
        this.kgTesting = false;
        this.kgTestSuccess = false;
        this.kgTestMessage = this.translate.instant('PROJECT_SETTINGS.KG_TEST_FAIL', { error: err?.message || 'http error' });
      }
    });
  }

  onSaveKgSettings(): void {
    this.kgSaving = true;
    this.projectSettingsService.saveKgSettings(this.projectId, {
      enabled: this.kgEnabled,
      uri: this.kgUri,
      database: this.kgDatabase,
      username: this.kgUsername,
      password: this.kgPassword || '',
      syncOnTocGeneration: this.kgSyncOnTocGeneration,
      syncOnKgFileSave: this.kgSyncOnKgFileSave
    }).subscribe({
      next: () => {
        this.kgSaving = false;
        if (this.kgPassword) {
          this.kgHasPassword = true;
          this.kgPassword = '';
        }
        if (this.kgEnabled && this.kgHasPassword) {
          this.loadKgState();
        }
      },
      error: (err) => {
        this.kgSaving = false;
        console.error('Error saving KG settings:', err);
      }
    });
  }

  loadKgState(): void {
    this.projectSettingsService.getKgState(this.projectId).subscribe({
      next: (r) => {
        this.kgStateTotals = r.totals;
        this.kgPerNamespace = r.perNamespace || [];
        this.kgStateLoaded = true;
      },
      error: (err) => {
        console.error('Error loading KG state:', err);
        this.kgStateLoaded = false;
      }
    });
  }

  onSyncKgProject(): void {
    this.kgSyncing = true;
    this.kgSyncMessage = '';
    this.kgSyncErrors = [];
    this.projectSettingsService.syncKgProject(this.projectId).subscribe({
      next: (r) => {
        this.kgSyncing = false;
        const results = r.results || [];
        const ok = results.filter((x: any) => !x.error && !x.skipped).length;
        const skipped = results.filter((x: any) => x.skipped).length;
        const failed = results.filter((x: any) => x.error).length;
        this.kgSyncMessage = this.translate.instant('PROJECT_SETTINGS.KG_SYNC_DONE', {
          ok, skipped, failed, total: results.length
        });
        this.kgSyncErrors = results.filter((x: any) => x.error);
        this.loadKgState();
      },
      error: (err) => {
        this.kgSyncing = false;
        this.kgSyncMessage = this.translate.instant('PROJECT_SETTINGS.KG_SYNC_FAILED', {
          error: err?.error?.error || err?.message || 'http error'
        });
      }
    });
  }

  onResetKg(): void {
    if (!confirm(this.translate.instant('PROJECT_SETTINGS.KG_RESET_CONFIRM'))) return;
    this.kgResetting = true;
    this.kgSyncMessage = '';
    this.projectSettingsService.resetKg(this.projectId).subscribe({
      next: () => {
        this.kgResetting = false;
        this.kgSyncMessage = this.translate.instant('PROJECT_SETTINGS.KG_RESET_DONE');
        this.kgSyncErrors = [];
        this.loadKgState();
      },
      error: (err) => {
        this.kgResetting = false;
        this.kgSyncMessage = this.translate.instant('PROJECT_SETTINGS.KG_RESET_FAILED', {
          error: err?.error?.error || err?.message || 'http error'
        });
      }
    });
  }

  // ============================================================
  //   Apache Jena Fuseki — tab logic (specchio del blocco KG/Neo4j)
  // ============================================================
  loadFusekiSettings(): void {
    if (!this.projectId) return;
    this.fsLoading = true;
    this.projectSettingsService.getFusekiSettings(this.projectId).subscribe({
      next: (r) => {
        this.fsEnabled = !!r.enabled;
        this.fsUri = r.uri || 'http://localhost:3030';
        this.fsDataset = r.dataset || '';
        this.fsDefaultDataset = r.defaultDataset || '';
        this.fsUsername = r.username || '';
        this.fsHasPassword = !!r.hasPassword;
        this.fsPassword = '';
        this.fsSyncOnTocGeneration = r.syncOnTocGeneration !== false;
        this.fsSyncOnKgFileSave = r.syncOnKgFileSave !== false;
        if (r.lastTestSuccess === true) {
          this.fsTestMessage = this.translate.instant('PROJECT_SETTINGS.FUSEKI_TEST_OK_PREVIOUS');
          this.fsTestSuccess = true;
        } else if (r.lastTestSuccess === false) {
          this.fsTestMessage = this.translate.instant('PROJECT_SETTINGS.FUSEKI_TEST_FAIL_PREVIOUS');
          this.fsTestSuccess = false;
        }
        this.fsLoading = false;
      },
      error: (err) => {
        console.error('Error loading Fuseki settings:', err);
        this.fsLoading = false;
      }
    });
  }

  onFusekiEnabledChange(): void {
    if (!this.fsEnabled) {
      this.onSaveFusekiSettings();
    }
  }

  onTestFusekiConnection(autoCreate: boolean = false): void {
    this.fsTesting = true;
    this.fsTestMessage = '';
    this.projectSettingsService.testFusekiConnection({
      projectId: this.projectId,
      uri: this.fsUri,
      dataset: this.fsDataset,
      username: this.fsUsername,
      password: this.fsPassword || (this.fsHasPassword ? '********' : ''),
      autoCreateDataset: autoCreate
    }).subscribe({
      next: (r) => {
        this.fsTesting = false;
        this.fsTestSuccess = !!r.success;
        if (r.success) {
          if (r.datasetCreated) {
            this.fsTestMessage = this.translate.instant('PROJECT_SETTINGS.FUSEKI_TEST_OK_DATASET_CREATED', {
              dataset: r.dataset, ms: r.latencyMs
            });
          } else {
            this.fsTestMessage = this.translate.instant('PROJECT_SETTINGS.FUSEKI_TEST_OK', {
              dataset: r.dataset, ms: r.latencyMs
            });
          }
        } else if (r.serverReachable && !r.datasetExists) {
          this.fsTestMessage = this.translate.instant('PROJECT_SETTINGS.FUSEKI_TEST_DATASET_MISSING', {
            dataset: r.dataset
          });
        } else {
          this.fsTestMessage = this.translate.instant('PROJECT_SETTINGS.FUSEKI_TEST_FAIL', {
            error: r.error || 'unknown'
          });
        }
      },
      error: (err) => {
        this.fsTesting = false;
        this.fsTestSuccess = false;
        this.fsTestMessage = this.translate.instant('PROJECT_SETTINGS.FUSEKI_TEST_FAIL', {
          error: err?.message || 'http error'
        });
      }
    });
  }

  onCreateFusekiDataset(): void {
    // Test + auto-create in un colpo solo
    this.onTestFusekiConnection(true);
  }

  onSaveFusekiSettings(): void {
    this.fsSaving = true;
    this.projectSettingsService.saveFusekiSettings(this.projectId, {
      enabled: this.fsEnabled,
      uri: this.fsUri,
      dataset: this.fsDataset,
      username: this.fsUsername,
      password: this.fsPassword || '',
      syncOnTocGeneration: this.fsSyncOnTocGeneration,
      syncOnKgFileSave: this.fsSyncOnKgFileSave
    }).subscribe({
      next: (r) => {
        this.fsSaving = false;
        if (r.dataset) {
          this.fsDataset = r.dataset; // server può aver sanitizzato
        }
        if (this.fsPassword) {
          this.fsHasPassword = true;
          this.fsPassword = '';
        }
      },
      error: (err) => {
        this.fsSaving = false;
        console.error('Error saving Fuseki settings:', err);
      }
    });
  }

  // ============================================================
  //   Atlassian (Jira/Confluence) — tab logic
  // ============================================================
  loadAtlassianSettings(): void {
    if (!this.projectId) return;
    this.atlLoading = true;
    this.projectSettingsService.getAtlassianSettings(this.projectId).subscribe({
      next: (r) => {
        this.atlEnabled = !!r.enabled;
        this.atlBaseUrl = r.jiraBaseUrl || '';
        this.atlProjectKeys = (r.jiraProjectKeys || []).join(', ');
        this.atlConfluenceBaseUrl = r.confluenceBaseUrl || '';
        this.atlConfluenceBaseUrlEffective = r.confluenceBaseUrlEffective || '';
        this.atlConfluenceSpaceKeys = (r.confluenceSpaceKeys || []).join(', ');
        this.atlEmail = r.email || '';
        this.atlHasToken = !!r.hasToken;
        this.atlToken = '';
        if (r.lastTestSuccess === true) {
          this.atlTestMessage = this.translate.instant('PROJECT_SETTINGS.ATLASSIAN_TEST_OK_PREVIOUS');
          this.atlTestSuccess = true;
        } else if (r.lastTestSuccess === false) {
          this.atlTestMessage = this.translate.instant('PROJECT_SETTINGS.ATLASSIAN_TEST_FAIL_PREVIOUS');
          this.atlTestSuccess = false;
        }
        this.atlLoading = false;
      },
      error: (err) => {
        console.error('Error loading Atlassian settings:', err);
        this.atlLoading = false;
      }
    });
  }

  onAtlassianEnabledChange(): void {
    // Persist immediately when turning off; otherwise wait for explicit Save
    // so the token and shared config travel together.
    if (!this.atlEnabled) {
      this.onSaveAtlassianSettings();
    }
  }

  private parseProjectKeys(): string[] {
    return (this.atlProjectKeys || '')
      .split(',')
      .map(k => k.trim())
      .filter(k => k.length > 0);
  }

  private parseConfluenceSpaceKeys(): string[] {
    return (this.atlConfluenceSpaceKeys || '')
      .split(',')
      .map(k => k.trim())
      .filter(k => k.length > 0);
  }

  onTestAtlassianConnection(): void {
    this.atlTesting = true;
    this.atlTestMessage = '';
    this.projectSettingsService.testAtlassianConnection({
      projectId: this.projectId,
      jiraBaseUrl: this.atlBaseUrl,
      email: this.atlEmail,
      apiToken: this.atlToken || (this.atlHasToken ? '********' : '')
    }).subscribe({
      next: (r) => {
        this.atlTesting = false;
        this.atlTestSuccess = !!r.success;
        if (r.success) {
          this.atlTestMessage = this.translate.instant('PROJECT_SETTINGS.ATLASSIAN_TEST_OK', {
            name: r.displayName || '', ms: r.latencyMs
          });
        } else {
          this.atlTestMessage = this.translate.instant('PROJECT_SETTINGS.ATLASSIAN_TEST_FAIL', {
            error: r.error || 'unknown'
          });
        }
      },
      error: (err) => {
        this.atlTesting = false;
        this.atlTestSuccess = false;
        this.atlTestMessage = this.translate.instant('PROJECT_SETTINGS.ATLASSIAN_TEST_FAIL', {
          error: err?.error?.error || err?.message || 'http error'
        });
      }
    });
  }

  onSaveAtlassianSettings(): void {
    this.atlSaving = true;
    this.projectSettingsService.saveAtlassianSettings(this.projectId, {
      enabled: this.atlEnabled,
      jiraBaseUrl: this.atlBaseUrl,
      jiraProjectKeys: this.parseProjectKeys(),
      confluenceBaseUrl: this.atlConfluenceBaseUrl,
      confluenceSpaceKeys: this.parseConfluenceSpaceKeys(),
      email: this.atlEmail,
      apiToken: this.atlToken || ''
    }).subscribe({
      next: () => {
        this.atlSaving = false;
        if (this.atlToken) {
          this.atlHasToken = true;
          this.atlToken = '';
        }
        // Refresh the derived/effective Confluence base URL after save.
        this.loadAtlassianSettings();
      },
      error: (err) => {
        this.atlSaving = false;
        console.error('Error saving Atlassian settings:', err);
      }
    });
  }
}