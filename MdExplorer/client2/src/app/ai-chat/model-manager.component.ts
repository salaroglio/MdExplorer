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
  
  private destroy$ = new Subject<void>();

  constructor(
    private aiService: AiChatService,
    private tocService: TocGenerationService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadModels();
    this.loadSystemPrompt();
    this.loadGpuInfo();
    this.checkGeminiConfiguration();
    
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
    this.aiService.setUseGemini(true, modelId);
    
    // Sync TOC generation service to use Gemini
    this.tocService.setAiMode(true, modelId).subscribe({
      next: () => console.log('[ModelManager] TOC service synced to use Gemini'),
      error: (err) => console.error('[ModelManager] Error syncing TOC service:', err)
    });
    
    // Notify that a model is now "loaded" (connected)
    this.aiService.notifyGeminiConnected(modelId);
    
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
  
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}