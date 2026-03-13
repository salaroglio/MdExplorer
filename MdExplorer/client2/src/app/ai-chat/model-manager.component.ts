import { Component, OnInit, OnDestroy, ChangeDetectorRef, Output, EventEmitter } from '@angular/core';
import { AiChatService, ModelInfo, DownloadProgress, GpuInfo } from '../services/ai-chat.service';
import { TocGenerationService } from '../md-explorer/services/toc-generation.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-model-manager',
  templateUrl: './model-manager.component.html',
  styleUrls: ['./model-manager.component.scss']
})
export class ModelManagerComponent implements OnInit, OnDestroy {
  @Output() contentChanged = new EventEmitter<void>();
  
  availableModels: ModelInfo[] = [];
  downloadProgress: { [modelId: string]: DownloadProgress } = {};
  currentModel: string | null = null;
  currentModelId: string | null = null;
  systemPrompt: string = '';
  editingSystemPrompt = false;
  applicationPrompt: string = '';
  defaultApplicationPrompt: string = '';
  editingApplicationPrompt = false;
  isModelLoaded = false;
  loading = false;
  gpuInfo: GpuInfo | null = null;
  gpuEnabled = false;
  gpuLayerCount = 0;
  
  // Gemini API properties
  useGemini = false;
  geminiApiKey = '';
  geminiModels: any[] = [];
  selectedGeminiModel = 'gemini-1.5-flash';
  geminiConfigured = false;
  showGeminiConfig = false;
  testingApiKey = false;
  geminiSystemPrompt = '';
  editingGeminiSystemPrompt = false;

  // OpenAI API properties
  useOpenAi = false;
  openAiApiKey = '';
  openAiModels: any[] = [];
  selectedOpenAiModel = 'gpt-4o';
  openAiConfigured = false;
  showOpenAiConfig = false;
  testingOpenAiApiKey = false;
  openAiSystemPrompt = '';
  editingOpenAiSystemPrompt = false;

  // Copilot CLI properties
  useCopilotCli = false;
  copilotCliModels: any[] = [];
  selectedCopilotCliModel = 'claude-sonnet-4';
  copilotCliAvailable = false;
  showCopilotCliConfig = false;
  copilotCliSystemPrompt = '';
  editingCopilotCliSystemPrompt = false;
  refreshingCopilotModels = false;

  // Multi-provider properties
  availableProviders: any[] = [];
  selectedProvider: string = 'local'; // 'local', 'openai', 'gemini', 'copilotcli'
  showProviderSelector = false;

  // Backend management properties
  backendStatus: any = null;
  availableBackends: any[] = [];
  recommendedBackend: string = '';
  backendDownloading = false;
  backendDownloadVariant: string = '';

  private destroy$ = new Subject<void>();

  constructor(
    private aiService: AiChatService,
    private tocService: TocGenerationService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadBackendStatus();
    this.loadModels();
    this.loadSystemPrompt();
    this.loadApplicationPrompt();
    this.loadGpuInfo();
    this.checkGeminiConfiguration();
    this.checkOpenAiConfiguration();
    this.checkCopilotCliConfiguration();
    this.loadAvailableProviders();
    this.loadDefaultPreferences();

    // Subscribe to download progress
    this.aiService.downloadProgress$
      .pipe(takeUntil(this.destroy$))
      .subscribe(progress => {
        this.downloadProgress[progress.modelId] = progress;
      });
    
    // Subscribe to current model
    this.aiService.currentModel$
      .pipe(takeUntil(this.destroy$))
      .subscribe(model => {
        this.currentModel = model;
      });
    
    // Subscribe to model loaded status
    this.aiService.isModelLoaded$
      .pipe(takeUntil(this.destroy$))
      .subscribe(loaded => {
        this.isModelLoaded = loaded;
        // When a model is loaded, reload the system prompt and GPU info
        if (loaded) {
          this.loadSystemPrompt();
          this.loadGpuInfo();
        }
      });
  }

  loadModels(): void {
    this.loading = true;
    this.aiService.getAvailableModels().subscribe({
      next: (models) => {
        this.availableModels = models;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading models:', err);
        this.loading = false;
      }
    });
  }

  downloadModel(model: ModelInfo): void {
    if (this.isDownloading(model.id)) return;
    
    this.downloadProgress[model.id] = {
      modelId: model.id,
      bytesDownloaded: 0,
      totalBytes: model.fileSize,
      percentComplete: 0,
      status: 'Starting'
    };
    
    this.aiService.downloadModel(model.id).subscribe({
      next: () => {
        console.log(`Download started for ${model.name}`);
      },
      error: (err) => {
        console.error(`Error downloading ${model.name}:`, err);
        delete this.downloadProgress[model.id];
      }
    });
  }

  deleteModel(model: ModelInfo): void {
    if (!confirm(`Delete model ${model.name}?`)) return;
    
    this.aiService.deleteModel(model.id).subscribe({
      next: () => {
        this.loadModels();
      },
      error: (err) => {
        console.error(`Error deleting ${model.name}:`, err);
      }
    });
  }

  loadModel(model: ModelInfo): void {
    console.log('[ModelManager] loadModel() called with:', model);
    if (!model.isInstalled) {
      console.log('[ModelManager] Model not installed, returning');
      return;
    }

    console.log('[ModelManager] Starting to load model:', model.id);
    this.loading = true;
    this.selectedProvider = 'local';

    this.aiService.loadModel(model.id).subscribe({
      next: (response: any) => {
        console.log(`[ModelManager] Model ${model.name} loaded successfully, response:`, response);
        this.loading = false;
        this.currentModelId = model.id;

        // Sync TOC generation service to use local model
        this.tocService.setAiMode(false).subscribe({
          next: () => console.log('[ModelManager] TOC service synced to use local model'),
          error: (err) => console.error('[ModelManager] Error syncing TOC service:', err)
        });

        // Load system prompt if provided in response
        if (response && response.systemPrompt) {
          this.systemPrompt = response.systemPrompt;
        }
        // Update GPU status
        if (response && response.gpuEnabled !== undefined) {
          this.gpuEnabled = response.gpuEnabled;
          this.gpuLayerCount = response.gpuLayerCount || 0;
        }
        // Refresh models list to update status
        this.loadModels();
        this.loadGpuInfo();

        // Save as default preference
        this.saveCurrentPreference('local', model.id);
      },
      error: (err) => {
        console.error(`[ModelManager] Error loading ${model.name}:`, err);
        console.error('[ModelManager] Error details:', err.error || err);
        this.loading = false;
      }
    });
  }
  
  editSystemPrompt(): void {
    this.editingSystemPrompt = true;
    this.contentChanged.emit();
  }
  
  
  saveSystemPrompt(): void {
    if (!this.systemPrompt.trim()) {
      alert('System prompt cannot be empty');
      return;
    }
    
    this.aiService.setSystemPrompt(this.systemPrompt).subscribe({
      next: () => {
        console.log('System prompt saved successfully');
        this.editingSystemPrompt = false;
      },
      error: (err) => {
        console.error('Error saving system prompt:', err);
        alert('Failed to save system prompt');
      }
    });
  }
  
  cancelEditSystemPrompt(): void {
    this.editingSystemPrompt = false;
    // Reload current system prompt
    this.loadSystemPrompt();
  }

  loadSystemPrompt(): void {
    this.aiService.getSystemPrompt().subscribe({
      next: (response: any) => {
        if (response && response.systemPrompt) {
          this.systemPrompt = response.systemPrompt;
          this.currentModelId = response.modelId;
        }
      },
      error: (err) => {
        console.error('Error loading system prompt:', err);
      }
    });
  }

  // Application System Prompt (tool guidance, thinking instructions)
  editApplicationPrompt(): void {
    this.editingApplicationPrompt = true;
    this.contentChanged.emit();
  }

  saveApplicationPrompt(): void {
    this.aiService.setApplicationPrompt(this.applicationPrompt).subscribe({
      next: (response: any) => {
        if (response?.applicationPrompt) {
          this.applicationPrompt = response.applicationPrompt;
        }
        this.editingApplicationPrompt = false;
      },
      error: (err) => {
        console.error('Error saving application prompt:', err);
        alert('Failed to save application prompt');
      }
    });
  }

  resetApplicationPrompt(): void {
    this.applicationPrompt = this.defaultApplicationPrompt;
  }

  cancelEditApplicationPrompt(): void {
    this.editingApplicationPrompt = false;
    this.loadApplicationPrompt();
  }

  loadApplicationPrompt(): void {
    this.aiService.getApplicationPrompt().subscribe({
      next: (response: any) => {
        if (response) {
          this.applicationPrompt = response.applicationPrompt || '';
          this.defaultApplicationPrompt = response.defaultPrompt || '';
        }
      },
      error: (err) => {
        console.error('Error loading application prompt:', err);
      }
    });
  }

  isDownloading(modelId: string): boolean {
    const progress = this.downloadProgress[modelId];
    return progress && progress.status !== 'Error' && progress.status !== 'Cancelled' && progress.percentComplete < 100;
  }

  formatBytes(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }

  isCurrentModel(model: ModelInfo): boolean {
    return this.currentModel === model.fileName?.replace('.gguf', '');
  }
  
  // Gemini API methods
  checkGeminiConfiguration(): void {
    this.aiService.checkGeminiConfiguration().subscribe({
      next: (response: any) => {
        this.geminiConfigured = response.configured;
        if (this.geminiConfigured) {
          this.loadGeminiModels();
          this.loadGeminiSystemPrompt();
        }
      },
      error: (err) => {
        console.error('Error checking Gemini configuration:', err);
      }
    });
  }
  
  loadGeminiModels(): void {
    this.aiService.getGeminiModels().subscribe({
      next: (models) => {
        this.geminiModels = models;
      },
      error: (err) => {
        console.error('Error loading Gemini models:', err);
      }
    });
  }
  
  loadGeminiSystemPrompt(): void {
    this.aiService.getGeminiSystemPrompt().subscribe({
      next: (response: any) => {
        this.geminiSystemPrompt = response.systemPrompt;
      },
      error: (err) => {
        console.error('Error loading Gemini system prompt:', err);
      }
    });
  }
  
  toggleGeminiConfig(): void {
    this.showGeminiConfig = !this.showGeminiConfig;
    if (this.showGeminiConfig && this.geminiConfigured) {
      this.loadGeminiModels();
    }
    this.contentChanged.emit();
  }
  
  
  testGeminiApiKey(): void {
    if (!this.geminiApiKey.trim()) {
      alert('Please enter an API key');
      return;
    }
    
    this.testingApiKey = true;
    this.aiService.testGeminiApiKey(this.geminiApiKey).subscribe({
      next: (response: any) => {
        if (response.valid) {
          alert('API key is valid!');
        } else {
          alert('Invalid API key');
        }
        this.testingApiKey = false;
      },
      error: (err) => {
        console.error('Error testing API key:', err);
        alert('Error testing API key');
        this.testingApiKey = false;
      }
    });
  }
  
  saveGeminiApiKey(): void {
    if (!this.geminiApiKey.trim()) {
      alert('Please enter an API key');
      return;
    }
    
    this.loading = true;
    this.aiService.saveGeminiApiKey(this.geminiApiKey).subscribe({
      next: () => {
        this.geminiConfigured = true;
        this.showGeminiConfig = false;
        this.geminiApiKey = '';
        this.loadGeminiModels();
        this.loading = false;
        alert('Gemini API key saved successfully');
      },
      error: (err) => {
        console.error('Error saving API key:', err);
        alert('Error saving API key. Please check if it is valid.');
        this.loading = false;
      }
    });
  }
  
  connectGeminiModel(modelId: string): void {
    console.log('[ModelManager] Connecting to Gemini model:', modelId);
    this.selectedGeminiModel = modelId;
    this.useGemini = true;
    this.selectedProvider = 'gemini';
    this.aiService.setProvider('gemini', modelId);

    // Sync TOC generation service to use Gemini
    this.tocService.setAiMode(true, modelId).subscribe({
      next: () => console.log('[ModelManager] TOC service synced to use Gemini'),
      error: (err) => console.error('[ModelManager] Error syncing TOC service:', err)
    });

    // Notify that a model is now "loaded" (connected)
    console.log('[ModelManager] Calling notifyGeminiConnected for model:', modelId);
    this.aiService.notifyGeminiConnected(modelId);
    console.log('[ModelManager] notifyGeminiConnected called successfully');

    // Save as default preference
    this.saveCurrentPreference('gemini', modelId);

    alert(`Connected to Gemini model: ${this.geminiModels.find(m => m.id === modelId)?.name}`);
  }
  
  disconnectGemini(): void {
    console.log('[ModelManager] Disconnecting from Gemini');
    this.useGemini = false;
    this.selectedGeminiModel = null;
    this.aiService.setUseGemini(false, null);
    
    // Sync TOC generation service to use local model
    this.tocService.setAiMode(false).subscribe({
      next: () => console.log('[ModelManager] TOC service synced to use local model'),
      error: (err) => console.error('[ModelManager] Error syncing TOC service:', err)
    });
    
    // Notify that Gemini is disconnected
    this.aiService.notifyGeminiDisconnected();
  }
  
  selectGeminiModel(modelId: string): void {
    // This method is now replaced by connectGeminiModel
    this.connectGeminiModel(modelId);
  }
  
  disableGemini(): void {
    // This method is now replaced by disconnectGemini
    this.disconnectGemini();
  }
  
  editGeminiSystemPrompt(): void {
    this.editingGeminiSystemPrompt = true;
    this.contentChanged.emit();
  }
  
  saveGeminiSystemPrompt(): void {
    if (!this.geminiSystemPrompt.trim()) {
      alert('System prompt cannot be empty');
      return;
    }
    
    this.aiService.setGeminiSystemPrompt(this.geminiSystemPrompt).subscribe({
      next: () => {
        console.log('Gemini system prompt saved successfully');
        this.editingGeminiSystemPrompt = false;
      },
      error: (err) => {
        console.error('Error saving Gemini system prompt:', err);
        alert('Failed to save system prompt');
      }
    });
  }
  
  cancelEditGeminiSystemPrompt(): void {
    this.editingGeminiSystemPrompt = false;
    this.loadGeminiSystemPrompt();
  }
  
  loadGpuInfo(): void {
    this.aiService.getGpuInfo().subscribe({
      next: (response: any) => {
        if (response && response.gpu) {
          this.gpuInfo = response.gpu;
          this.gpuEnabled = response.modelGpuEnabled || false;
          this.gpuLayerCount = response.modelGpuLayerCount || 0;
        }
      },
      error: (err) => {
        console.error('Error loading GPU info:', err);
      }
    });
  }

  // OpenAI methods
  checkOpenAiConfiguration(): void {
    this.aiService.checkOpenAiConfiguration().subscribe({
      next: (response: any) => {
        this.openAiConfigured = response.configured;
        if (this.openAiConfigured) {
          this.loadOpenAiModels();
          this.loadOpenAiSystemPrompt();
        }
      },
      error: (err) => {
        console.error('Error checking OpenAI configuration:', err);
      }
    });
  }

  loadOpenAiModels(): void {
    this.aiService.getOpenAiModels().subscribe({
      next: (models) => {
        this.openAiModels = models;
      },
      error: (err) => {
        console.error('Error loading OpenAI models:', err);
      }
    });
  }

  loadOpenAiSystemPrompt(): void {
    this.aiService.getOpenAiSystemPrompt().subscribe({
      next: (response: any) => {
        this.openAiSystemPrompt = response.systemPrompt;
      },
      error: (err) => {
        console.error('Error loading OpenAI system prompt:', err);
      }
    });
  }

  toggleOpenAiConfig(): void {
    this.showOpenAiConfig = !this.showOpenAiConfig;
    if (this.showOpenAiConfig && this.openAiConfigured) {
      this.loadOpenAiModels();
    }
    this.contentChanged.emit();
  }

  testOpenAiApiKey(): void {
    if (!this.openAiApiKey.trim()) {
      alert('Please enter an API key');
      return;
    }

    this.testingOpenAiApiKey = true;
    this.aiService.testOpenAiApiKey(this.openAiApiKey).subscribe({
      next: (response: any) => {
        if (response.valid) {
          alert('API key is valid!');
        } else {
          alert('Invalid API key');
        }
        this.testingOpenAiApiKey = false;
      },
      error: (err) => {
        console.error('Error testing API key:', err);
        alert('Error testing API key');
        this.testingOpenAiApiKey = false;
      }
    });
  }

  saveOpenAiApiKey(): void {
    if (!this.openAiApiKey.trim()) {
      alert('Please enter an API key');
      return;
    }

    this.loading = true;
    this.aiService.saveOpenAiApiKey(this.openAiApiKey).subscribe({
      next: () => {
        this.openAiConfigured = true;
        this.showOpenAiConfig = false;
        this.openAiApiKey = '';
        this.loadOpenAiModels();
        this.loading = false;
        alert('OpenAI API key saved successfully');
      },
      error: (err) => {
        console.error('Error saving API key:', err);
        alert('Error saving API key. Please check if it is valid.');
        this.loading = false;
      }
    });
  }

  connectOpenAiModel(modelId: string): void {
    console.log('[ModelManager] Connecting to OpenAI model:', modelId);
    this.selectedOpenAiModel = modelId;
    this.useOpenAi = true;
    this.selectedProvider = 'openai';

    // Disconnect Gemini if active
    if (this.useGemini) {
      this.disconnectGemini();
    }

    // Set provider to OpenAI
    this.aiService.setProvider('openai', modelId);

    // Notify that a model is now "loaded" (connected)
    console.log('[ModelManager] Calling notifyOpenAiConnected for OpenAI model:', modelId);
    this.aiService.notifyOpenAiConnected(modelId);
    console.log('[ModelManager] OpenAI model connected successfully');

    // Save as default preference
    this.saveCurrentPreference('openai', modelId);

    alert(`Connected to OpenAI model: ${this.openAiModels.find(m => m.id === modelId)?.name || modelId}`);
  }

  disconnectOpenAi(): void {
    console.log('[ModelManager] Disconnecting from OpenAI');
    this.useOpenAi = false;
    this.selectedOpenAiModel = null;
    this.selectedProvider = 'local';

    // Notify that OpenAI is disconnected
    this.aiService.notifyGeminiDisconnected();
  }

  editOpenAiSystemPrompt(): void {
    this.editingOpenAiSystemPrompt = true;
    this.contentChanged.emit();
  }

  saveOpenAiSystemPrompt(): void {
    if (!this.openAiSystemPrompt.trim()) {
      alert('System prompt cannot be empty');
      return;
    }

    this.aiService.setOpenAiSystemPrompt(this.openAiSystemPrompt).subscribe({
      next: () => {
        console.log('OpenAI system prompt saved successfully');
        this.editingOpenAiSystemPrompt = false;
      },
      error: (err) => {
        console.error('Error saving OpenAI system prompt:', err);
        alert('Failed to save system prompt');
      }
    });
  }

  cancelEditOpenAiSystemPrompt(): void {
    this.editingOpenAiSystemPrompt = false;
    this.loadOpenAiSystemPrompt();
  }

  // Multi-provider methods
  loadAvailableProviders(): void {
    this.aiService.getAvailableProviders().subscribe({
      next: (response: any) => {
        this.availableProviders = response.providers || [];
        console.log('[ModelManager] Loaded providers:', this.availableProviders);
      },
      error: (err) => {
        console.error('Error loading providers:', err);
      }
    });
  }

  selectProvider(providerType: string): void {
    console.log('[ModelManager] Selecting provider:', providerType);
    this.selectedProvider = providerType.toLowerCase();

    // Disconnect all providers first
    if (this.useGemini) {
      this.disconnectGemini();
    }
    if (this.useOpenAi) {
      this.disconnectOpenAi();
    }
    if (this.useCopilotCli) {
      this.disconnectCopilotCli();
    }

    this.showProviderSelector = false;
  }

  toggleProviderSelector(): void {
    this.showProviderSelector = !this.showProviderSelector;
    this.contentChanged.emit();
  }

  // Preference management methods
  loadDefaultPreferences(): void {
    this.aiService.getDefaultAiPreferences().subscribe({
      next: (response: any) => {
        console.log('[ModelManager] Loaded preferences:', response);
        if (response.hasDefault && response.provider && response.model) {
          this.autoConnectToDefaultProvider(response.provider, response.model);
        }
      },
      error: (err) => {
        console.error('Error loading AI preferences:', err);
      }
    });
  }

  autoConnectToDefaultProvider(provider: string, model: string): void {
    console.log('[ModelManager] Auto-connecting to provider:', provider, 'model:', model);

    // Wait for configurations to load
    setTimeout(() => {
      switch (provider.toLowerCase()) {
        case 'gemini':
          if (this.geminiConfigured && this.geminiModels.length > 0) {
            this.connectGeminiModel(model);
          }
          break;
        case 'openai':
          if (this.openAiConfigured && this.openAiModels.length > 0) {
            this.connectOpenAiModel(model);
          }
          break;
        case 'copilotcli':
          if (this.copilotCliAvailable && this.copilotCliModels.length > 0) {
            this.connectCopilotCliModel(model);
          }
          break;
        case 'local':
          // For local models, find the model and load it
          const localModel = this.availableModels.find(m => m.id === model);
          if (localModel && localModel.isInstalled) {
            this.loadModel(localModel);
          }
          break;
      }
    }, 1000); // Give time for configurations and models to load
  }

  saveCurrentPreference(provider: string, model: string): void {
    console.log('[ModelManager] Saving preference:', provider, model);
    this.aiService.saveDefaultAiPreferences(provider, model).subscribe({
      next: (response: any) => {
        console.log('[ModelManager] Preference saved successfully:', response);
      },
      error: (err) => {
        console.error('Error saving AI preference:', err);
      }
    });
  }

  // Copilot CLI methods
  checkCopilotCliConfiguration(): void {
    this.aiService.checkCopilotCliConfiguration().subscribe({
      next: (response: any) => {
        this.copilotCliAvailable = response.configured;
        if (this.copilotCliAvailable) {
          this.loadCopilotCliModels();
          this.loadCopilotCliSystemPrompt();
        }
      },
      error: (err) => {
        console.error('Error checking Copilot CLI configuration:', err);
      }
    });
  }

  loadCopilotCliModels(): void {
    this.aiService.getCopilotCliModels().subscribe({
      next: (models) => {
        this.copilotCliModels = models;
      },
      error: (err) => {
        console.error('Error loading Copilot CLI models:', err);
      }
    });
  }

  refreshCopilotCliModels(): void {
    this.refreshingCopilotModels = true;
    this.aiService.refreshCopilotCliModels().subscribe({
      next: (response) => {
        this.copilotCliModels = response.models || [];
        this.refreshingCopilotModels = false;
      },
      error: (err) => {
        console.error('Error refreshing Copilot CLI models:', err);
        this.refreshingCopilotModels = false;
      }
    });
  }

  loadCopilotCliSystemPrompt(): void {
    this.aiService.getCopilotCliSystemPrompt().subscribe({
      next: (response: any) => {
        this.copilotCliSystemPrompt = response.systemPrompt;
      },
      error: (err) => {
        console.error('Error loading Copilot CLI system prompt:', err);
      }
    });
  }

  toggleCopilotCliConfig(): void {
    this.showCopilotCliConfig = !this.showCopilotCliConfig;
    if (this.showCopilotCliConfig && this.copilotCliAvailable) {
      this.loadCopilotCliModels();
    }
    this.contentChanged.emit();
  }

  connectCopilotCliModel(modelId: string): void {
    console.log('[ModelManager] Connecting to Copilot CLI model:', modelId);
    this.selectedCopilotCliModel = modelId;
    this.useCopilotCli = true;
    this.selectedProvider = 'copilotcli';

    // Disconnect other providers
    if (this.useGemini) {
      this.disconnectGemini();
    }
    if (this.useOpenAi) {
      this.disconnectOpenAi();
    }

    // Set provider
    this.aiService.setProvider('copilotcli', modelId);

    // Notify that a model is now connected
    this.aiService.notifyCopilotCliConnected(modelId);

    // Save as default preference
    this.saveCurrentPreference('copilotcli', modelId);

    alert(`Connected to Copilot CLI model: ${this.copilotCliModels.find(m => m.id === modelId)?.name || modelId}`);
  }

  disconnectCopilotCli(): void {
    console.log('[ModelManager] Disconnecting from Copilot CLI');
    this.useCopilotCli = false;
    this.selectedCopilotCliModel = null;
    this.selectedProvider = 'local';
    this.aiService.notifyCopilotCliDisconnected();
  }

  editCopilotCliSystemPrompt(): void {
    this.editingCopilotCliSystemPrompt = true;
    this.contentChanged.emit();
  }

  saveCopilotCliSystemPrompt(): void {
    if (!this.copilotCliSystemPrompt.trim()) {
      alert('System prompt cannot be empty');
      return;
    }

    this.aiService.setCopilotCliSystemPrompt(this.copilotCliSystemPrompt).subscribe({
      next: () => {
        console.log('Copilot CLI system prompt saved successfully');
        this.editingCopilotCliSystemPrompt = false;
      },
      error: (err) => {
        console.error('Error saving Copilot CLI system prompt:', err);
        alert('Failed to save system prompt');
      }
    });
  }

  cancelEditCopilotCliSystemPrompt(): void {
    this.editingCopilotCliSystemPrompt = false;
    this.loadCopilotCliSystemPrompt();
  }

  // Backend management methods
  loadBackendStatus(): void {
    this.aiService.getBackendStatus().subscribe({
      next: (status) => {
        this.backendStatus = status;
        if (!status.isInstalled) {
          this.loadAvailableBackendsForDownload();
        }
      },
      error: (err) => {
        console.error('Error loading backend status:', err);
      }
    });
  }

  loadAvailableBackendsForDownload(): void {
    this.aiService.getAvailableBackends().subscribe({
      next: (response: any) => {
        this.availableBackends = response.backends || [];
        this.recommendedBackend = response.recommended || 'cpu';
      },
      error: (err) => {
        console.error('Error loading available backends:', err);
      }
    });
  }

  downloadBackend(variant: string): void {
    if (this.backendDownloading) return;

    this.backendDownloading = true;
    this.backendDownloadVariant = variant;

    this.downloadProgress[`backend-${variant}`] = {
      modelId: `backend-${variant}`,
      bytesDownloaded: 0,
      totalBytes: 0,
      percentComplete: 0,
      status: 'Starting'
    };

    this.aiService.downloadBackend(variant).subscribe({
      next: () => {
        console.log(`Backend ${variant} download started`);
      },
      error: (err) => {
        console.error(`Error downloading backend ${variant}:`, err);
        this.backendDownloading = false;
        this.backendDownloadVariant = '';
        delete this.downloadProgress[`backend-${variant}`];
        alert(`Error downloading backend: ${err.error?.error || err.message}`);
      },
      complete: () => {
        this.backendDownloading = false;
        this.backendDownloadVariant = '';
        delete this.downloadProgress[`backend-${variant}`];
        this.loadBackendStatus();
      }
    });
  }

  deleteBackend(): void {
    if (!confirm('Delete the installed AI engine? You will need to download it again to use local AI models.')) return;

    this.aiService.deleteBackend().subscribe({
      next: () => {
        this.loadBackendStatus();
        this.loadAvailableBackendsForDownload();
      },
      error: (err) => {
        console.error('Error deleting backend:', err);
      }
    });
  }

  isBackendDownloading(variant: string): boolean {
    const progress = this.downloadProgress[`backend-${variant}`];
    return progress && progress.status !== 'Error' && progress.status !== 'Cancelled' && progress.status !== 'Complete';
  }

  getBackendVariantName(variant: string): string {
    const names: {[key: string]: string} = {
      'cpu': 'CPU Only',
      'cuda-12.4': 'NVIDIA CUDA',
      'vulkan': 'Vulkan (AMD/Intel/NVIDIA)'
    };
    return names[variant] || variant;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}