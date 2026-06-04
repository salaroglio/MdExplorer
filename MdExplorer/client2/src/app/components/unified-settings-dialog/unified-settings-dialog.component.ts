import { Component, Inject, OnInit, OnDestroy } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { AppCurrentMetadataService } from '../../services/app-current-metadata.service';
import { FileChangeNotificationService } from '../../services/file-change-notification.service';
import { AiChatService, ModelInfo, DownloadProgress, GpuInfo } from '../../services/ai-chat.service';
import { EmbeddingConfigService, EmbeddingConfig, EmbeddingConfigResponse, EmbeddingModelInfo } from '../../services/embedding-config.service';
import { IMdSetting } from '../../models/IMdSetting';
import { TocGenerationService } from '../../md-explorer/services/toc-generation.service';
import { LanguageService } from '../../services/language.service';
import { ThemeService, ThemeMode } from '../../services/theme.service';
import { TranslateService } from '@ngx-translate/core';
import { ServicesMonitorService, ServiceDto } from '../../services/services-monitor.service';
import { MdServerMessagesService } from '../../signalR/services/server-messages.service';

@Component({
  selector: 'app-unified-settings-dialog',
  templateUrl: './unified-settings-dialog.component.html',
  styleUrls: ['./unified-settings-dialog.component.scss']
})
export class UnifiedSettingsDialogComponent implements OnInit, OnDestroy {
  selectedCategory: string = 'application';
  private destroy$ = new Subject<void>();

  // === Application tab ===
  _settings: IMdSetting[] = [];
  vscodePath: string = '';
  intellijPath: string = '';
  jiraServer: string = '';
  jiraEnabled: boolean = false;
  plantumlLocalPath: string = '';

  javaPath: string = '';
  localGraphvizDotPath: string = '';
  fileChangeNotificationEnabled: boolean = true;
  teamsChatEnabled: boolean = true;
  isElectronEnvironment: boolean = false;
  appSaving: boolean = false;

  // === Language ===
  currentLanguage: string = 'en';
  supportedLanguages: { code: string; label: string }[] = [];

  // === Theme ===
  currentThemeMode: ThemeMode = 'light';
  supportedThemes: { code: ThemeMode; label: string; icon: string }[] = [];

  // === AI Models tab ===
  availableModels: ModelInfo[] = [];
  downloadProgress: { [modelId: string]: DownloadProgress } = {};
  currentModel: string | null = null;
  currentModelId: string | null = null;
  isModelLoaded: boolean = false;
  aiLoading: boolean = false;
  gpuInfo: GpuInfo | null = null;
  gpuEnabled: boolean = false;
  gpuLayerCount: number = 0;
  systemPrompt: string = '';
  editingSystemPrompt: boolean = false;

  // Gemini
  useGemini: boolean = false;
  geminiApiKey: string = '';
  geminiModels: any[] = [];
  selectedGeminiModel: string = 'gemini-1.5-flash';
  geminiConfigured: boolean = false;
  showGeminiConfig: boolean = false;
  testingApiKey: boolean = false;
  geminiSystemPrompt: string = '';
  editingGeminiSystemPrompt: boolean = false;

  // OpenAI
  useOpenAi: boolean = false;
  openAiApiKey: string = '';
  openAiModels: any[] = [];
  selectedOpenAiModel: string = 'gpt-4o';
  openAiConfigured: boolean = false;
  showOpenAiConfig: boolean = false;
  testingOpenAiApiKey: boolean = false;
  openAiSystemPrompt: string = '';
  editingOpenAiSystemPrompt: boolean = false;

  selectedProvider: string = 'local';

  // === Embedding tab ===
  embeddingConfig: EmbeddingConfig | null = null;
  embeddingPresets: { [modelId: string]: EmbeddingConfig } = {};
  embeddingModels: EmbeddingModelInfo[] = [];
  embeddingModelLoaded: boolean = false;
  embeddingDimension: number = 0;
  embeddingCurrentModelPath: string = '';
  embeddingLoading: boolean = false;
  embeddingSaving: boolean = false;

  // Editable fields
  editContextSize: number = 4096;
  editBatchSize: number = 4096;
  editMaxChunkChars: number = 2000;
  editMaxEmbeddingChars: number = 12000;
  editSelectedModel: string = '';

  // === Services tab ===
  services: ServiceDto[] = [];
  servicesLoading: boolean = false;
  private servicesPollTimer: any = null;

  constructor(
    private dialogRef: MatDialogRef<UnifiedSettingsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private appMetadataService: AppCurrentMetadataService,
    private fileChangeNotificationService: FileChangeNotificationService,
    private aiService: AiChatService,
    private embeddingConfigService: EmbeddingConfigService,
    private tocService: TocGenerationService,
    private snackBar: MatSnackBar,
    private languageService: LanguageService,
    private themeService: ThemeService,
    private translate: TranslateService,
    private servicesMonitor: ServicesMonitorService,
    private serverMessages: MdServerMessagesService
  ) {
    this.isElectronEnvironment = !!(window as any).electronAPI?.flashTaskbarIcon;
    if (data?.initialTab) {
      this.selectedCategory = data.initialTab;
    }
  }

  ngOnInit(): void {
    this.currentLanguage = this.languageService.getCurrentLanguage();
    this.supportedLanguages = this.languageService.getSupportedLanguages();
    this.currentThemeMode = this.themeService.getCurrentMode();
    this.supportedThemes = this.themeService.getSupportedThemes();
    this.loadApplicationSettings();
    this.loadAiModels();
    this.loadEmbeddingConfig();

    // Subscribe to download progress
    this.aiService.downloadProgress$
      .pipe(takeUntil(this.destroy$))
      .subscribe(progress => {
        this.downloadProgress[progress.modelId] = progress;
      });

    this.aiService.currentModel$
      .pipe(takeUntil(this.destroy$))
      .subscribe(model => {
        this.currentModel = model;
      });

    this.aiService.isModelLoaded$
      .pipe(takeUntil(this.destroy$))
      .subscribe(loaded => {
        this.isModelLoaded = loaded;
        if (loaded) {
          this.loadGpuInfo();
        }
      });

    // Keep the Services list fresh whenever a service starts/stops anywhere.
    this.serverMessages.serviceStarted$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => { if (this.selectedCategory === 'services') this.loadServices(); });
    this.serverMessages.serviceStopped$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => { if (this.selectedCategory === 'services') this.loadServices(); });

    if (this.selectedCategory === 'services') {
      this.startServicesPolling();
    }
  }

  selectCategory(category: string): void {
    this.selectedCategory = category;
    if (category === 'services') {
      this.startServicesPolling();
    } else {
      this.stopServicesPolling();
    }
  }

  // ============================
  //  APPLICATION TAB
  // ============================

  loadApplicationSettings(): void {
    this.appMetadataService.loadSettings();
    this.appMetadataService.settings.subscribe((settings: IMdSetting[]) => {
      if (settings && settings.length > 0) {
        this._settings = settings;
        this.vscodePath = settings.find(_ => _.name === 'EditorPath')?.valueString || '';
        this.intellijPath = settings.find(_ => _.name === 'IntelliJPath')?.valueString || '';
        this.jiraServer = settings.find(_ => _.name === 'JiraServer')?.valueString || '';
        this.jiraEnabled = (settings.find(_ => _.name === 'JiraEnabled')?.valueInt ?? 0) === 1;
        this.plantumlLocalPath = settings.find(_ => _.name === 'PlantumlLocalPath')?.valueString || '';
        this.javaPath = settings.find(_ => _.name === 'JavaPath')?.valueString || '';
        this.localGraphvizDotPath = settings.find(_ => _.name === 'LocalGraphvizDotPath')?.valueString || '';
        // Teams chat defaults to enabled when the setting is missing.
        this.teamsChatEnabled = (settings.find(_ => _.name === 'TeamsChatEnabled')?.valueInt ?? 1) === 1;

        const savedTheme = settings.find(_ => _.name === 'ThemeMode')?.valueString;
        if (savedTheme && ['light', 'dark', 'system'].includes(savedTheme)) {
          this.currentThemeMode = savedTheme as ThemeMode;
          this.themeService.setTheme(this.currentThemeMode);
        }
      }
    });
    this.fileChangeNotificationEnabled = this.fileChangeNotificationService.isEnabled();
  }

  onFileChangeNotificationToggle(): void {
    this.fileChangeNotificationService.setEnabled(this.fileChangeNotificationEnabled);
  }

  saveApplicationSettings(): void {
    this.updateSetting('EditorPath', this.vscodePath);
    this.updateSetting('IntelliJPath', this.intellijPath);
    this.updateSetting('JiraServer', this.jiraServer);
    this.updateSettingInt('JiraEnabled', this.jiraEnabled ? 1 : 0);
    this.updateSetting('PlantumlLocalPath', this.plantumlLocalPath);
    this.updateSetting('JavaPath', this.javaPath);
    this.updateSetting('LocalGraphvizDotPath', this.localGraphvizDotPath);
    this.updateSettingInt('TeamsChatEnabled', this.teamsChatEnabled ? 1 : 0);
    this.updateSetting('ThemeMode', this.currentThemeMode);

    this.appMetadataService.saveSettings(this._settings).subscribe(() => {
      this.snackBar.open(this.translate.instant('UNIFIED_SETTINGS.APP_SAVED'), '', { duration: 2000 });
      this.dialogRef.close();
    });
  }

  private updateSetting(name: string, value: string): void {
    const setting = this._settings.find(_ => _.name === name);
    if (setting) {
      setting.valueString = value;
    } else {
      this._settings.push({ name: name, valueString: value } as IMdSetting);
    }
  }

  private updateSettingInt(name: string, value: number): void {
    const setting = this._settings.find(_ => _.name === name);
    if (setting) {
      setting.valueInt = value;
    } else {
      this._settings.push({ name: name, valueInt: value } as IMdSetting);
    }
  }

  onLanguageChange(): void {
    this.languageService.setLanguage(this.currentLanguage);
  }

  onThemeChange(): void {
    this.themeService.setTheme(this.currentThemeMode);
    this.updateSetting('ThemeMode', this.currentThemeMode);
    this.appMetadataService.saveSettings(this._settings).subscribe();
  }

  // ============================
  //  AI MODELS TAB
  // ============================

  loadAiModels(): void {
    this.aiLoading = true;
    this.aiService.getAvailableModels().subscribe({
      next: (models) => {
        this.availableModels = models;
        this.aiLoading = false;
      },
      error: () => { this.aiLoading = false; }
    });
    this.loadSystemPrompt();
    this.loadGpuInfo();
    this.checkGeminiConfiguration();
    this.checkOpenAiConfiguration();
  }

  downloadModel(model: ModelInfo): void {
    if (this.isDownloading(model.id)) return;
    this.downloadProgress[model.id] = {
      modelId: model.id, bytesDownloaded: 0,
      totalBytes: model.fileSize, percentComplete: 0, status: 'Starting'
    };
    this.aiService.downloadModel(model.id).subscribe({
      next: () => console.log(`Download started for ${model.name}`),
      error: () => { delete this.downloadProgress[model.id]; }
    });
  }

  deleteModel(model: ModelInfo): void {
    if (!confirm(this.translate.instant('UNIFIED_SETTINGS.DELETE_MODEL_CONFIRM', { name: model.name }))) return;
    this.aiService.deleteModel(model.id).subscribe({
      next: () => this.loadAiModels(),
      error: (err) => console.error(`Error deleting ${model.name}:`, err)
    });
  }

  loadModel(model: ModelInfo): void {
    if (!model.isInstalled) return;
    this.aiLoading = true;
    this.selectedProvider = 'local';
    this.aiService.loadModel(model.id).subscribe({
      next: (response: any) => {
        this.aiLoading = false;
        this.currentModelId = model.id;
        if (response?.systemPrompt) this.systemPrompt = response.systemPrompt;
        if (response?.gpuEnabled !== undefined) {
          this.gpuEnabled = response.gpuEnabled;
          this.gpuLayerCount = response.gpuLayerCount || 0;
        }
        this.aiService.saveDefaultAiPreferences('local', model.id).subscribe();
        this.loadAiModels();
      },
      error: () => { this.aiLoading = false; }
    });
  }

  isDownloading(modelId: string): boolean {
    const progress = this.downloadProgress[modelId];
    return progress && progress.status !== 'Error' && progress.status !== 'Cancelled' && progress.percentComplete < 100;
  }

  isCurrentModel(model: ModelInfo): boolean {
    return this.currentModel === model.fileName?.replace('.gguf', '');
  }

  loadSystemPrompt(): void {
    this.aiService.getSystemPrompt().subscribe({
      next: (response: any) => {
        if (response?.systemPrompt) {
          this.systemPrompt = response.systemPrompt;
          this.currentModelId = response.modelId;
        }
      },
      error: () => {}
    });
  }

  editSystemPromptToggle(): void {
    this.editingSystemPrompt = !this.editingSystemPrompt;
  }

  saveSystemPrompt(): void {
    this.aiService.setSystemPrompt(this.systemPrompt).subscribe({
      next: () => {
        this.editingSystemPrompt = false;
        this.snackBar.open(this.translate.instant('UNIFIED_SETTINGS.SYSTEM_PROMPT_SAVED'), '', { duration: 2000 });
      },
      error: () => this.snackBar.open(this.translate.instant('UNIFIED_SETTINGS.SYSTEM_PROMPT_ERROR'), '', { duration: 3000 })
    });
  }

  loadGpuInfo(): void {
    this.aiService.getGpuInfo().subscribe({
      next: (response: any) => {
        if (response?.gpu) {
          this.gpuInfo = response.gpu;
          this.gpuEnabled = response.modelGpuEnabled || false;
          this.gpuLayerCount = response.modelGpuLayerCount || 0;
        }
      },
      error: () => {}
    });
  }

  // Gemini
  checkGeminiConfiguration(): void {
    this.aiService.checkGeminiConfiguration().subscribe({
      next: (response: any) => {
        this.geminiConfigured = response.configured;
        if (this.geminiConfigured) {
          this.aiService.getGeminiModels().subscribe({ next: (m) => this.geminiModels = m });
        }
      },
      error: () => {}
    });
  }

  toggleGeminiConfig(): void { this.showGeminiConfig = !this.showGeminiConfig; }

  saveGeminiApiKey(): void {
    this.aiService.saveGeminiApiKey(this.geminiApiKey).subscribe({
      next: () => {
        this.geminiConfigured = true;
        this.geminiApiKey = '';
        this.aiService.getGeminiModels().subscribe({ next: (m) => this.geminiModels = m });
        this.snackBar.open(this.translate.instant('UNIFIED_SETTINGS.GEMINI_KEY_SAVED'), '', { duration: 2000 });
      },
      error: () => this.snackBar.open(this.translate.instant('UNIFIED_SETTINGS.GEMINI_KEY_ERROR'), '', { duration: 3000 })
    });
  }

  connectGeminiModel(modelId: string): void {
    this.selectedGeminiModel = modelId;
    this.useGemini = true;
    this.selectedProvider = 'gemini';
    this.aiService.setProvider('gemini', modelId);
    this.aiService.notifyGeminiConnected(modelId);
    this.aiService.saveDefaultAiPreferences('gemini', modelId).subscribe();
    this.snackBar.open(this.translate.instant('UNIFIED_SETTINGS.CONNECTED_GEMINI', { model: modelId }), '', { duration: 2000 });
  }

  disconnectGemini(): void {
    this.useGemini = false;
    this.aiService.setUseGemini(false, null);
    this.aiService.notifyGeminiDisconnected();
  }

  // OpenAI
  checkOpenAiConfiguration(): void {
    this.aiService.checkOpenAiConfiguration().subscribe({
      next: (response: any) => {
        this.openAiConfigured = response.configured;
        if (this.openAiConfigured) {
          this.aiService.getOpenAiModels().subscribe({ next: (m) => this.openAiModels = m });
        }
      },
      error: () => {}
    });
  }

  toggleOpenAiConfig(): void { this.showOpenAiConfig = !this.showOpenAiConfig; }

  saveOpenAiApiKey(): void {
    this.aiService.saveOpenAiApiKey(this.openAiApiKey).subscribe({
      next: () => {
        this.openAiConfigured = true;
        this.openAiApiKey = '';
        this.aiService.getOpenAiModels().subscribe({ next: (m) => this.openAiModels = m });
        this.snackBar.open(this.translate.instant('UNIFIED_SETTINGS.OPENAI_KEY_SAVED'), '', { duration: 2000 });
      },
      error: () => this.snackBar.open(this.translate.instant('UNIFIED_SETTINGS.OPENAI_KEY_ERROR'), '', { duration: 3000 })
    });
  }

  connectOpenAiModel(modelId: string): void {
    this.selectedOpenAiModel = modelId;
    this.useOpenAi = true;
    this.selectedProvider = 'openai';
    if (this.useGemini) this.disconnectGemini();
    this.aiService.setProvider('openai', modelId);
    this.aiService.notifyOpenAiConnected(modelId);
    this.aiService.saveDefaultAiPreferences('openai', modelId).subscribe();
    this.snackBar.open(this.translate.instant('UNIFIED_SETTINGS.CONNECTED_OPENAI', { model: modelId }), '', { duration: 2000 });
  }

  disconnectOpenAi(): void {
    this.useOpenAi = false;
    this.selectedProvider = 'local';
    this.aiService.notifyGeminiDisconnected();
  }

  // ============================
  //  EMBEDDING TAB
  // ============================

  loadEmbeddingConfig(): void {
    this.embeddingLoading = true;
    this.embeddingConfigService.getConfig().subscribe({
      next: (response: EmbeddingConfigResponse) => {
        this.embeddingConfig = response.config;
        this.embeddingPresets = response.presets;
        this.embeddingModels = response.embeddingModels;
        this.embeddingModelLoaded = response.modelLoaded;
        this.embeddingDimension = response.embeddingDimension;
        this.embeddingCurrentModelPath = response.currentModelPath;

        // Populate edit fields
        this.editSelectedModel = response.config.selectedModel || '';
        this.editContextSize = response.config.contextSize;
        this.editBatchSize = response.config.batchSize;
        this.editMaxChunkChars = response.config.maxChunkChars;
        this.editMaxEmbeddingChars = response.config.maxEmbeddingChars;

        this.embeddingLoading = false;
      },
      error: () => { this.embeddingLoading = false; }
    });
  }

  applyPreset(modelId: string): void {
    const preset = this.embeddingPresets[modelId];
    if (preset) {
      this.editSelectedModel = preset.selectedModel;
      this.editContextSize = preset.contextSize;
      this.editBatchSize = preset.batchSize;
      this.editMaxChunkChars = preset.maxChunkChars;
      this.editMaxEmbeddingChars = preset.maxEmbeddingChars;
    }
  }

  onContextSizeChange(): void {
    // Auto-sync batch size for embedding models
    this.editBatchSize = this.editContextSize;
    // Auto-calculate max embedding chars (~3 chars/token)
    this.editMaxEmbeddingChars = this.editContextSize * 3;
  }

  saveEmbeddingConfig(): void {
    this.embeddingSaving = true;
    const config: EmbeddingConfig = {
      selectedModel: this.editSelectedModel,
      contextSize: this.editContextSize,
      batchSize: this.editBatchSize,
      maxChunkChars: this.editMaxChunkChars,
      maxEmbeddingChars: this.editMaxEmbeddingChars
    };

    this.embeddingConfigService.saveConfig(config).subscribe({
      next: (response) => {
        this.embeddingSaving = false;
        if (response.reindexRequired) {
          this.snackBar.open(response.message, 'OK', { duration: 5000 });
        } else {
          this.snackBar.open(this.translate.instant('UNIFIED_SETTINGS.EMBEDDING_SAVED'), '', { duration: 2000 });
        }
        // Reload to reflect new state
        this.loadEmbeddingConfig();
      },
      error: () => {
        this.embeddingSaving = false;
        this.snackBar.open(this.translate.instant('UNIFIED_SETTINGS.EMBEDDING_ERROR'), '', { duration: 3000 });
      }
    });
  }

  resetEmbeddingDefaults(): void {
    if (this.editSelectedModel && this.embeddingPresets[this.editSelectedModel]) {
      this.applyPreset(this.editSelectedModel);
    }
  }

  isEmbeddingModelActive(model: EmbeddingModelInfo): boolean {
    return this.editSelectedModel === model.id;
  }

  selectEmbeddingModel(model: EmbeddingModelInfo): void {
    if (model.isInstalled) {
      this.applyPreset(model.id);
    }
  }

  // ============================
  //  UTILITIES
  // ============================

  formatBytes(bytes: number): string {
    if (!bytes || bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }

  close(): void {
    this.dialogRef.close();
  }

  // ============================
  //  SERVICES TAB
  // ============================

  private startServicesPolling(): void {
    this.loadServices();
    if (this.servicesPollTimer) return;
    // Refresh periodically so the uptime column stays current while the panel is open.
    this.servicesPollTimer = setInterval(() => this.loadServices(), 3000);
  }

  private stopServicesPolling(): void {
    if (this.servicesPollTimer) {
      clearInterval(this.servicesPollTimer);
      this.servicesPollTimer = null;
    }
  }

  loadServices(): void {
    this.servicesLoading = true;
    this.servicesMonitor.list().subscribe({
      next: (list) => {
        this.services = list || [];
        this.servicesLoading = false;
      },
      error: () => { this.servicesLoading = false; }
    });
  }

  stopService(svc: ServiceDto): void {
    const msg = this.translate.instant('UNIFIED_SETTINGS.SERVICES_STOP_CONFIRM');
    if (!window.confirm(msg)) return;
    this.servicesMonitor.stop(svc.id).subscribe({
      next: () => this.loadServices(),
      error: () => this.loadServices(),
    });
  }

  openInBrowser(svc: ServiceDto): void {
    if (!svc.detectedPort) return;
    const url = `http://localhost:${svc.detectedPort}`;
    const api = (window as any).electronAPI;
    if (api?.openExternal) {
      api.openExternal(url);
    } else {
      window.open(url, '_blank');
    }
  }

  formatUptime(ms: number): string {
    if (!ms || ms < 0) return '0s';
    const totalSec = Math.floor(ms / 1000);
    const h = Math.floor(totalSec / 3600);
    const m = Math.floor((totalSec % 3600) / 60);
    const s = totalSec % 60;
    if (h > 0) return `${h}h ${m}m`;
    if (m > 0) return `${m}m ${s}s`;
    return `${s}s`;
  }

  ngOnDestroy(): void {
    this.stopServicesPolling();
    this.destroy$.next();
    this.destroy$.complete();
  }
}
