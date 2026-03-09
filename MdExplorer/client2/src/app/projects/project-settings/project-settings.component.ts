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

@Component({
  selector: 'app-project-settings',
  templateUrl: './project-settings.component.html',
  styleUrls: ['./project-settings.component.scss']
})
export class ProjectSettingsComponent implements OnInit, OnDestroy {
  rule1Enabled: boolean = false;
  linkIndexingEnabled: boolean = true;
  githubModeEnabled: boolean = false;
  stickyScrollEnabled: boolean = true;
  selectedIde: string = 'vscode';
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
    private dialog: MatDialog
  ) {
    this.projectId = data.projectId;
    this.projectName = data.projectName;
    this.projectPath = data.projectPath;
  }

  ngOnInit(): void {
    this.loadSettings();
    this.loadAppsConfig();

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
  }

  ngOnDestroy(): void {
    this.ragProgressSub?.unsubscribe();
  }

  loadSettings(): void {
    this.loading = true;
    let rule1Loaded = false;
    let linkIndexingLoaded = false;
    let compatibilityLoaded = false;
    let ideConfigLoaded = false;
    let ragLoaded = false;
    let stickyScrollLoaded = false;

    const checkIfDone = () => {
      if (rule1Loaded && linkIndexingLoaded && compatibilityLoaded && ideConfigLoaded && ragLoaded && stickyScrollLoaded) {
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
          this.ragMessage = response.indexingProgress.message || 'Indexing in progress...';
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
        this.saving = false;
      },
      error: (error) => {
        console.error('Error saving IDE configuration:', error);
        this.saving = false;
        // Revert the change on error
        this.selectedIde = this.selectedIde === 'vscode' ? 'intellij' : 'vscode';
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
    this.ragMessage = 'Starting indexing...';
    this.projectSettingsService.reindexRag(this.projectPath).subscribe({
      next: (response: any) => {
        console.log('RAG reindex started:', response);
        // Progress updates will arrive via SignalR
      },
      error: (error) => {
        console.error('Error triggering RAG reindex:', error);
        this.ragReindexing = false;
        this.ragMessage = 'Error starting indexing';
      }
    });
  }

  onRagClear(): void {
    if (!confirm('This will delete all indexed RAG chunks. Continue?')) {
      return;
    }
    this.saving = true;
    this.projectSettingsService.clearRagIndex(this.projectPath).subscribe({
      next: (response: any) => {
        console.log('RAG index cleared:', response);
        this.saving = false;
        this.ragMessage = `Cleared ${response.chunksDeleted} chunks`;
        this.refreshRagStatus();
      },
      error: (error) => {
        console.error('Error clearing RAG index:', error);
        this.saving = false;
        this.ragMessage = 'Error clearing index';
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
    const name = prompt('Category name:');
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
    if (!confirm(`Remove "${appName}" from this project?`)) return;

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
        this.appsSavedMessage = 'Saved';
        this.refreshTree();
        setTimeout(() => this.appsSavedMessage = '', 3000);
      },
      error: (err) => {
        console.error('Error saving config:', err);
        this.appsSaving = false;
        this.appsSavedMessage = 'Error saving';
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
}