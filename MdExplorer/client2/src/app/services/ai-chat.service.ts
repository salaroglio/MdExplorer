import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Observable, Subject, BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';

export interface ModelInfo {
  id: string;
  name: string;
  description: string;
  url: string;
  fileName: string;
  fileSize: number;
  isInstalled: boolean;
  localPath?: string;
  contextLength: number;
  parameters: string;
}

export interface GpuInfo {
  isNvidiaGpu: boolean;
  isRtxCard: boolean;
  name: string;
  deviceId: string;
  memoryBytes: number;
  driverVersion: string;
  cudaVersion: number;
  isCudaAvailable: boolean;
  status: string;
  formattedMemory?: string;
}

export interface DownloadProgress {
  modelId: string;
  bytesDownloaded: number;
  totalBytes: number;
  percentComplete: number;
  status: string;
}

export interface ChatMessage {
  id: string;
  role: 'user' | 'assistant' | 'system';
  content: string;
  timestamp: Date;
  isStreaming?: boolean;
  thinkingContent?: string;
  providerType?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AiChatService {
  private hubConnection: HubConnection;
  private baseUrl = '/api/AiModels';
  
  private _messages$ = new BehaviorSubject<ChatMessage[]>([]);
  public messages$ = this._messages$.asObservable();
  
  private _downloadProgress$ = new Subject<DownloadProgress>();
  public downloadProgress$ = this._downloadProgress$.asObservable();
  
  private _currentModel$ = new BehaviorSubject<string | null>(null);
  public currentModel$ = this._currentModel$.asObservable();
  
  private _isModelLoaded$ = new BehaviorSubject<boolean>(false);
  public isModelLoaded$ = this._isModelLoaded$.asObservable();
  
  private _streamingMessage$ = new Subject<string>();
  public streamingMessage$ = this._streamingMessage$.asObservable();
  
  private _gpuInfo$ = new BehaviorSubject<GpuInfo | null>(null);
  public gpuInfo$ = this._gpuInfo$.asObservable();
  
  private _gpuEnabled$ = new BehaviorSubject<boolean>(false);
  public gpuEnabled$ = this._gpuEnabled$.asObservable();
  
  private currentStreamingMessageId: string | null = null;
  private currentStreamingProviderType: string | null = null;

  // Gemini API state
  private _useGemini$ = new BehaviorSubject<boolean>(false);
  public useGemini$ = this._useGemini$.asObservable();

  private _geminiModel$ = new BehaviorSubject<string>('gemini-1.5-flash');
  public geminiModel$ = this._geminiModel$.asObservable();

  // Current document context for AI
  private _currentDocument$ = new BehaviorSubject<string | null>(null);
  public currentDocument$ = this._currentDocument$.asObservable();

  constructor(private http: HttpClient) {
    this.initializeSignalR();
  }

  private initializeSignalR(): void {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl('/signalr/aichat')
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect()
      .build();

    // Setup event handlers
    this.hubConnection.on('DownloadProgress', (progress: DownloadProgress) => {
      this._downloadProgress$.next(progress);
    });

    this.hubConnection.on('DownloadComplete', (modelId: string) => {
      console.log(`Model ${modelId} download complete`);
      this.getAvailableModels().subscribe(); // Refresh model list
    });

    this.hubConnection.on('DownloadError', (modelId: string, error: string) => {
      console.error(`Model ${modelId} download error:`, error);
    });

    this.hubConnection.on('ModelLoaded', (modelName: string) => {
      console.log('[AiChatService] Received ModelLoaded event:', modelName);
      this._currentModel$.next(modelName);
      this._isModelLoaded$.next(true);
    });
    
    this.hubConnection.on('ModelLoading', (modelName: string) => {
      console.log('[AiChatService] Received ModelLoading event:', modelName);
    });

    this.hubConnection.on('ModelLoadError', (error: string) => {
      console.error('[AiChatService] Received ModelLoadError event:', error);
      this._isModelLoaded$.next(false);
    });

    this.hubConnection.on('ReceiveMessage', (role: string, content: string) => {
      this.addMessage(role as 'system' | 'assistant', content);
    });

    this.hubConnection.on('ReceiveStreamMeta', (meta: { providerType: string }) => {
      this.currentStreamingProviderType = meta.providerType;
    });

    this.hubConnection.on('ReceiveStreamChunk', (chunk: string) => {
      this._streamingMessage$.next(chunk);
      this.appendToStreamingMessage(chunk);
    });

    this.hubConnection.on('ReceiveThinking', (chunk: string) => {
      this.appendToThinkingContent(chunk);
    });

    this.hubConnection.on('StreamComplete', () => {
      this.finalizeStreamingMessage();
    });

    this.hubConnection.on('ReceiveError', (error: string) => {
      console.error('Chat error:', error);
      this.addMessage('system', `Error: ${error}`);
    });

    // Start connection
    this.startConnection();
  }

  private async startConnection(): Promise<void> {
    try {
      await this.hubConnection.start();
      console.log('SignalR connection established');
      await this.getModelStatus();
    } catch (err) {
      console.error('Error establishing SignalR connection:', err);
      setTimeout(() => this.startConnection(), 5000);
    }
  }

  // Model Management
  getAvailableModels(): Observable<ModelInfo[]> {
    return this.http.get<ModelInfo[]>(`${this.baseUrl}/available`);
  }

  getInstalledModels(): Observable<ModelInfo[]> {
    return this.http.get<ModelInfo[]>(`${this.baseUrl}/installed`);
  }

  downloadModel(modelId: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/download/${modelId}`, {});
  }

  deleteModel(modelId: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${modelId}`);
  }

  loadModel(modelId: string): Observable<any> {
    console.log('[AiChatService] loadModel() called with modelId:', modelId);
    console.log('[AiChatService] HubConnection state:', this.hubConnection.state);
    
    if (this.hubConnection.state === 'Connected') {
      console.log('[AiChatService] Using SignalR to load model');
      return new Observable(observer => {
        this.hubConnection.invoke('LoadModel', modelId)
          .then((response) => {
            console.log('[AiChatService] SignalR LoadModel success, response:', response);
            observer.next(response);
            observer.complete();
          })
          .catch(err => {
            console.error('[AiChatService] SignalR LoadModel error:', err);
            observer.error(err);
          });
      });
    }
    
    console.log('[AiChatService] Using HTTP POST to load model');
    return this.http.post(`${this.baseUrl}/load/${modelId}`, {});
  }

  async getModelStatus(): Promise<void> {
    if (this.hubConnection.state === 'Connected') {
      try {
        const status = await this.hubConnection.invoke('GetModelStatus');
        this._isModelLoaded$.next(status.isModelLoaded);
        this._currentModel$.next(status.currentModel);
      } catch (err) {
        console.error('Error getting model status:', err);
      }
    }
  }

  // Chat functionality
  sendMessage(message: string): void {
    if (!message.trim()) return;

    // Add user message
    this.addMessage('user', message);

    // Create placeholder for assistant response
    const assistantMessageId = this.generateMessageId();
    this.currentStreamingMessageId = assistantMessageId;
    this.addMessage('assistant', '', assistantMessageId, true);

    // Set providerType on the placeholder if known
    if (this.currentStreamingProviderType) {
      const messages = this._messages$.value;
      const lastMsg = messages[messages.length - 1];
      lastMsg.providerType = this.currentStreamingProviderType;
    }

    // Send to server
    if (this.hubConnection.state === 'Connected') {
      this.hubConnection.invoke('SendMessage', message)
        .catch(err => {
          console.error('Error sending message:', err);
          this.addMessage('system', `Failed to send message: ${err}`);
        });
    }
  }

  private addMessage(role: 'user' | 'assistant' | 'system', content: string, id?: string, isStreaming?: boolean): void {
    const messages = this._messages$.value;
    const newMessage: ChatMessage = {
      id: id || this.generateMessageId(),
      role,
      content,
      timestamp: new Date(),
      isStreaming
    };
    this._messages$.next([...messages, newMessage]);
  }

  private appendToStreamingMessage(chunk: string): void {
    if (!this.currentStreamingMessageId) return;
    
    const messages = this._messages$.value;
    const messageIndex = messages.findIndex(m => m.id === this.currentStreamingMessageId);
    
    if (messageIndex !== -1) {
      messages[messageIndex].content += chunk;
      this._messages$.next([...messages]);
    }
  }

  private appendToThinkingContent(chunk: string): void {
    if (!this.currentStreamingMessageId) return;

    const messages = this._messages$.value;
    const idx = messages.findIndex(m => m.id === this.currentStreamingMessageId);

    if (idx !== -1) {
      messages[idx].thinkingContent = (messages[idx].thinkingContent || '') + chunk;
      messages[idx].providerType = this.currentStreamingProviderType;
      this._messages$.next([...messages]);
    }
  }

  private finalizeStreamingMessage(): void {
    if (!this.currentStreamingMessageId) return;

    const messages = this._messages$.value;
    const messageIndex = messages.findIndex(m => m.id === this.currentStreamingMessageId);

    if (messageIndex !== -1) {
      messages[messageIndex].isStreaming = false;
      this._messages$.next([...messages]);
    }

    this.currentStreamingMessageId = null;
    this.currentStreamingProviderType = null;
  }

  clearMessages(): void {
    this._messages$.next([]);
    // Clear backend conversation history too
    if (this.hubConnection.state === 'Connected') {
      this.hubConnection.invoke('ClearHistory').catch(err => {
        console.error('[AiChatService] Error clearing history:', err);
      });
    }
  }
  
  getSystemPrompt(): Observable<any> {
    return this.http.get(`${this.baseUrl}/system-prompt`);
  }
  
  setSystemPrompt(systemPrompt: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/system-prompt`, { systemPrompt });
  }
  
  getGpuInfo(): Observable<any> {
    return this.http.get(`${this.baseUrl}/gpu-info`);
  }

  private generateMessageId(): string {
    return `msg-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }
  
  // Gemini API methods
  checkGeminiConfiguration(): Observable<any> {
    return this.http.get('/api/gemini/configured');
  }

  getGeminiModels(): Observable<any[]> {
    return this.http.get<any[]>('/api/gemini/models');
  }

  testGeminiApiKey(apiKey: string): Observable<any> {
    return this.http.post('/api/gemini/test-api-key', { apiKey });
  }

  saveGeminiApiKey(apiKey: string): Observable<any> {
    return this.http.post('/api/gemini/api-key', { apiKey });
  }

  getGeminiSystemPrompt(): Observable<any> {
    return this.http.get('/api/gemini/system-prompt');
  }

  setGeminiSystemPrompt(systemPrompt: string): Observable<any> {
    return this.http.post('/api/gemini/system-prompt', { systemPrompt });
  }

  // Multi-Provider AI methods
  getAvailableProviders(): Observable<any> {
    return this.http.get('/api/aiproviders/list');
  }

  getAllModelsFromAllProviders(): Observable<any> {
    return this.http.get('/api/aiproviders/models');
  }

  getModelsByProvider(providerType: string): Observable<any> {
    return this.http.get(`/api/aiproviders/models/${providerType}`);
  }

  testChatWithProvider(providerType: string, message: string, modelId?: string): Observable<any> {
    return this.http.post('/api/aiproviders/test-chat', {
      providerType,
      message,
      modelId
    });
  }

  // OpenAI API methods
  checkOpenAiConfiguration(): Observable<any> {
    return this.http.get('/api/openai/configured');
  }

  getOpenAiModels(): Observable<any[]> {
    // Uses the multi-provider endpoint
    return this.getModelsByProvider('OpenAI').pipe(
      map((response: any) => response.models || [])
    );
  }

  testOpenAiApiKey(apiKey: string): Observable<any> {
    return this.http.post('/api/openai/test-api-key', { apiKey });
  }

  saveOpenAiApiKey(apiKey: string): Observable<any> {
    return this.http.post('/api/openai/api-key', { apiKey });
  }

  getOpenAiSystemPrompt(): Observable<any> {
    return this.http.get('/api/openai/system-prompt');
  }

  setOpenAiSystemPrompt(systemPrompt: string): Observable<any> {
    return this.http.post('/api/openai/system-prompt', { systemPrompt });
  }
  
  setUseGemini(useGemini: boolean, modelId: string | null): void {
    this._useGemini$.next(useGemini);
    if (modelId) {
      this._geminiModel$.next(modelId);
    }

    // Notify via SignalR
    if (this.hubConnection.state === 'Connected') {
      this.hubConnection.invoke('SetChatMode', useGemini ? 'gemini' : 'local', modelId);
    }
  }

  /**
   * Set the AI provider to use (generic method for all providers).
   * @param provider 'local', 'gemini', or 'openai'
   * @param modelId Model ID to use with the provider
   */
  setProvider(provider: string, modelId: string | null): void {
    console.log('[AiChatService] setProvider called with:', provider, modelId);

    // Update internal state based on provider
    if (provider === 'gemini') {
      this._useGemini$.next(true);
      if (modelId) {
        this._geminiModel$.next(modelId);
      }
    } else {
      this._useGemini$.next(false);
    }

    // Notify backend via SignalR
    if (this.hubConnection.state === 'Connected') {
      this.hubConnection.invoke('SetChatMode', provider, modelId)
        .then(() => {
          console.log('[AiChatService] SetChatMode sent to backend:', provider, modelId);
        })
        .catch(err => {
          console.error('[AiChatService] Error calling SetChatMode:', err);
        });
    }
  }
  
  notifyGeminiConnected(modelId: string): void {
    console.log('[AiChatService] notifyGeminiConnected called with modelId:', modelId);
    console.log('[AiChatService] Current isModelLoaded value before update:', this._isModelLoaded$.getValue());
    
    // When Gemini is connected, we treat it as a "loaded" model
    this._isModelLoaded$.next(true);
    this._currentModel$.next(`Gemini: ${modelId}`);
    
    console.log('[AiChatService] Gemini connected:', modelId);
    console.log('[AiChatService] isModelLoaded value after update:', this._isModelLoaded$.getValue());
    console.log('[AiChatService] currentModel value after update:', this._currentModel$.getValue());
  }
  
  notifyGeminiDisconnected(): void {
    // When Gemini is disconnected, check if we have a local model loaded
    // For now, we'll set to false assuming no local model
    this._isModelLoaded$.next(false);
    this._currentModel$.next(null);
    console.log('[AiChatService] Gemini disconnected');
  }

  /**
   * Notify that OpenAI has been connected.
   * Updates the model loaded status for the UI.
   */
  notifyOpenAiConnected(modelId: string): void {
    console.log('[AiChatService] notifyOpenAiConnected called with modelId:', modelId);
    console.log('[AiChatService] Current isModelLoaded value before update:', this._isModelLoaded$.getValue());

    // When OpenAI is connected, we treat it as a "loaded" model
    this._isModelLoaded$.next(true);
    this._currentModel$.next(`OpenAI: ${modelId}`);

    console.log('[AiChatService] OpenAI connected:', modelId);
    console.log('[AiChatService] isModelLoaded value after update:', this._isModelLoaded$.getValue());
    console.log('[AiChatService] currentModel value after update:', this._currentModel$.getValue());
  }

  /**
   * Notify that OpenAI has been disconnected.
   */
  notifyOpenAiDisconnected(): void {
    // When OpenAI is disconnected, check if we have a local model loaded
    // For now, we'll set to false assuming no local model
    this._isModelLoaded$.next(false);
    this._currentModel$.next(null);
    console.log('[AiChatService] OpenAI disconnected');
  }

  // Copilot CLI methods
  checkCopilotCliConfiguration(): Observable<any> {
    return this.http.get('/api/copilotcli/configured');
  }

  getCopilotCliModels(): Observable<any[]> {
    return this.getModelsByProvider('CopilotCli').pipe(
      map((response: any) => response.models || [])
    );
  }

  getCopilotCliSystemPrompt(): Observable<any> {
    return this.http.get('/api/copilotcli/system-prompt');
  }

  setCopilotCliSystemPrompt(systemPrompt: string): Observable<any> {
    return this.http.post('/api/copilotcli/system-prompt', { systemPrompt });
  }

  refreshCopilotCliModels(): Observable<any> {
    return this.http.post('/api/copilotcli/refresh-models', {});
  }

  notifyCopilotCliConnected(modelId: string): void {
    console.log('[AiChatService] notifyCopilotCliConnected called with modelId:', modelId);
    this._isModelLoaded$.next(true);
    this._currentModel$.next(`CopilotCli: ${modelId}`);
    console.log('[AiChatService] CopilotCli connected:', modelId);
  }

  notifyCopilotCliDisconnected(): void {
    this._isModelLoaded$.next(false);
    this._currentModel$.next(null);
    console.log('[AiChatService] CopilotCli disconnected');
  }

  generateCommitMessage(projectPath: string): Observable<any> {
    return this.http.post('/api/GitAi/generate-commit-message', { projectPath });
  }
  
  getGitAiStatus(): Observable<any> {
    return this.http.get('/api/GitAi/ai-status');
  }

  // AI Preferences methods
  getDefaultAiPreferences(): Observable<any> {
    return this.http.get('/api/AiPreferences/default');
  }

  saveDefaultAiPreferences(provider: string, model: string): Observable<any> {
    return this.http.post('/api/AiPreferences/default', { provider, model });
  }

  clearDefaultAiPreferences(): Observable<any> {
    return this.http.delete('/api/AiPreferences/default');
  }

  /**
   * Set the current document context for AI.
   * This allows context-aware operations like "add a section" without specifying the file.
   * @param filePath Relative path to the current document (from workspace root)
   */
  setCurrentDocument(filePath: string | null): void {
    console.log('[AiChatService] setCurrentDocument called with:', filePath);

    // Update local state
    this._currentDocument$.next(filePath);

    // Notify backend via SignalR
    if (this.hubConnection.state === 'Connected' && filePath) {
      this.hubConnection.invoke('SetCurrentDocument', filePath)
        .then(() => {
          console.log('[AiChatService] SetCurrentDocument sent to backend:', filePath);
        })
        .catch(err => {
          console.error('[AiChatService] Error calling SetCurrentDocument:', err);
        });
    }
  }

  /**
   * Edit a user message and regenerate the conversation from that point.
   * All messages after the edited message will be removed.
   * @param messageIndex Index of the message in the conversation history (0-based)
   * @param newContent New content for the message
   */
  editAndRegenerateMessage(messageIndex: number, newContent: string): void {
    console.log('[AiChatService] editAndRegenerateMessage called with index:', messageIndex);

    if (this.hubConnection.state === 'Connected') {
      this.hubConnection.invoke('EditAndRegenerateFromMessage', messageIndex, newContent)
        .then(() => {
          console.log('[AiChatService] EditAndRegenerateFromMessage sent to backend');
        })
        .catch(err => {
          console.error('[AiChatService] Error calling EditAndRegenerateFromMessage:', err);
        });
    }
  }

  ngOnDestroy(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }
}