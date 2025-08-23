import { Component, OnInit, OnDestroy } from '@angular/core';
import { AiChatService, ModelInfo, DownloadProgress } from '../services/ai-chat.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-model-manager',
  templateUrl: './model-manager.component.html',
  styleUrls: ['./model-manager.component.scss']
})
export class ModelManagerComponent implements OnInit, OnDestroy {
  availableModels: ModelInfo[] = [];
  downloadProgress: { [modelId: string]: DownloadProgress } = {};
  currentModel: string | null = null;
  currentModelId: string | null = null;
  systemPrompt: string = '';
  editingSystemPrompt = false;
  isModelLoaded = false;
  loading = false;
  
  private destroy$ = new Subject<void>();

  constructor(private aiService: AiChatService) {}

  ngOnInit(): void {
    this.loadModels();
    this.loadSystemPrompt();
    
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
        // When a model is loaded, reload the system prompt
        if (loaded) {
          this.loadSystemPrompt();
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
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
        // Load system prompt if provided in response
        if (response && response.systemPrompt) {
          this.systemPrompt = response.systemPrompt;
        }
        // Refresh models list to update status
        this.loadModels();
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
}