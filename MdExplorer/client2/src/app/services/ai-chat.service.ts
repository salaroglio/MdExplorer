import { Injectable, Inject, forwardRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Observable, Subject, BehaviorSubject, of } from 'rxjs';
import { filter, map, tap, shareReplay, finalize } from 'rxjs/operators';
import { MdServerMessagesService } from '../signalR/services/server-messages.service';

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

  // Last scroll position of the chat message list. Persisted here (in the
  // singleton service) rather than in AiChatComponent because mat-tab-group
  // uses preserveContent=false: switching away from the Mark Agent tab detaches
  // the tab body portal and destroys the component, so any component-local
  // scroll state would be lost. savedAtBottom lets the freshly recreated
  // component decide between "restore the exact position" and "snap to bottom".
  public savedScrollTop = 0;
  public savedAtBottom = true;
  
  private _downloadProgress$ = new Subject<DownloadProgress>();
  public downloadProgress$ = this._downloadProgress$.asObservable();
  
  private _currentModel$ = new BehaviorSubject<string | null>(null);
  public currentModel$ = this._currentModel$.asObservable();
  
  private _isModelLoaded$ = new BehaviorSubject<boolean>(false);
  public isModelLoaded$ = this._isModelLoaded$.asObservable();

  private _isConfiguringProvider$ = new BehaviorSubject<boolean>(false);
  public isConfiguringProvider$ = this._isConfiguringProvider$.asObservable();
  
  private _streamingMessage$ = new Subject<string>();
  public streamingMessage$ = this._streamingMessage$.asObservable();

  // True while a prompt is streaming a response on the default channel — drives the Stop button.
  private _isStreaming$ = new BehaviorSubject<boolean>(false);
  public isStreaming$ = this._isStreaming$.asObservable();

  private _gpuInfo$ = new BehaviorSubject<GpuInfo | null>(null);
  public gpuInfo$ = this._gpuInfo$.asObservable();
  
  private _gpuEnabled$ = new BehaviorSubject<boolean>(false);
  public gpuEnabled$ = this._gpuEnabled$.asObservable();
  
  private currentStreamingMessageId: string | null = null;
  private currentStreamingProviderType: string | null = null;

  // Channel-aware event stream for PromptLab and multi-channel consumers
  private _channelEvent$ = new Subject<{ type: string; data: any; channelId: string }>();

  // Gemini API state
  private _useGemini$ = new BehaviorSubject<boolean>(false);
  public useGemini$ = this._useGemini$.asObservable();

  private _geminiModel$ = new BehaviorSubject<string>('gemini-1.5-flash');
  public geminiModel$ = this._geminiModel$.asObservable();

  // Current document context for AI
  private _currentDocument$ = new BehaviorSubject<string | null>(null);
  public currentDocument$ = this._currentDocument$.asObservable();

  // Captures a SetChatMode attempted before the SignalR hub was connected.
  // Replayed in startConnection() BEFORE getModelStatus() so the backend
  // answers with the correct provider instead of "None".
  private _pendingChatMode: { provider: string; modelId: string | null } | null = null;

  // Last chat mode successfully applied. Unlike _pendingChatMode (cleared on flush),
  // this persists so onreconnected() can re-register the provider on the NEW connectionId
  // that automatic reconnect assigns — otherwise the reconnected hub has no provider and
  // every prompt fails with "Copilot not available".
  private _lastChatMode: { provider: string; modelId: string | null } | null = null;

  constructor(
    private http: HttpClient,
    @Inject(forwardRef(() => MdServerMessagesService)) private serverMessages: MdServerMessagesService
  ) {
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

    this.hubConnection.on('ReceiveStreamChunk', (chunk: string, channelId?: string) => {
      const ch = channelId || 'default';
      this._channelEvent$.next({ type: 'chunk', data: chunk, channelId: ch });
      if (ch === 'default') {
        this._streamingMessage$.next(chunk);
        this.appendToStreamingMessage(chunk);
      }
    });

    this.hubConnection.on('ReceiveThinking', (chunk: string, channelId?: string) => {
      const ch = channelId || 'default';
      this._channelEvent$.next({ type: 'thinking', data: chunk, channelId: ch });
      if (ch === 'default') {
        this.appendToThinkingContent(chunk);
      }
    });

    this.hubConnection.on('StreamComplete', (channelId?: string) => {
      const ch = channelId || 'default';
      this._channelEvent$.next({ type: 'complete', data: null, channelId: ch });
      if (ch === 'default') {
        this.finalizeStreamingMessage();
        this._isStreaming$.next(false);
      }
    });

    this.hubConnection.on('ReceiveError', (error: string, channelId?: string) => {
      const ch = channelId || 'default';
      this._channelEvent$.next({ type: 'error', data: error, channelId: ch });
      if (ch === 'default') {
        console.error('Chat error:', error);
        this.addMessage('system', `Error: ${error}`);
        this._isStreaming$.next(false);
      }
    });

    // Automatic reconnect assigns a NEW connectionId, and the backend wiped all state
    // (chat mode, project mapping, ACP session) for the old one in OnDisconnectedAsync.
    // Re-register everything the hub needs, or every subsequent prompt would fail with
    // "provider not available".
    this.hubConnection.onreconnected(() => {
      console.log('[AiChatService] Reconnected — replaying project connection and chat mode');
      this.sendProjectConnectionId();
      if (this._lastChatMode) {
        this.hubConnection.invoke('SetChatMode', this._lastChatMode.provider, this._lastChatMode.modelId)
          .then(() => console.log('[AiChatService] Chat mode replayed after reconnect:', this._lastChatMode))
          .catch(err => console.error('[AiChatService] Error replaying chat mode after reconnect:', err));
      }
    });

    // Start connection
    this.startConnection();
  }

  private async startConnection(): Promise<void> {
    try {
      await this.hubConnection.start();
      console.log('SignalR connection established');

      // Send the MonitorMDHub connectionId so AiChatHub can resolve the project path
      this.sendProjectConnectionId();

      // Flush any SetChatMode queued while the hub was still connecting.
      // Must run BEFORE getModelStatus() — otherwise the backend answers with
      // ProviderType=null and the client overrides the locally-set state back to
      // isModelLoaded=false, currentModel="None".
      if (this._pendingChatMode) {
        const pending = this._pendingChatMode;
        this._pendingChatMode = null;
        try {
          await this.hubConnection.invoke('SetChatMode', pending.provider, pending.modelId);
          console.log('[AiChatService] Flushed pending SetChatMode on connect:', pending.provider, pending.modelId);
        } catch (err) {
          console.error('[AiChatService] Error flushing pending SetChatMode:', err);
        }
      }

      await this.getModelStatus();
    } catch (err) {
      console.error('Error establishing SignalR connection:', err);
      setTimeout(() => this.startConnection(), 5000);
    }
  }

  /**
   * Sends the MonitorMDHub connectionId to AiChatHub so it can
   * look up the project path via WatcherManager.
   */
  private sendProjectConnectionId(): void {
    const projectConnId = this.serverMessages?.connectionId;
    if (projectConnId && this.hubConnection.state === 'Connected') {
      this.hubConnection.invoke('SetProjectConnectionId', projectConnId)
        .then(() => console.log('[AiChatService] Sent project connectionId:', projectConnId))
        .catch(err => console.error('[AiChatService] Error sending project connectionId:', err));
    } else {
      // MonitorMDHub might not be connected yet — retry after a short delay
      setTimeout(() => {
        const connId = this.serverMessages?.connectionId;
        if (connId && this.hubConnection.state === 'Connected') {
          this.hubConnection.invoke('SetProjectConnectionId', connId)
            .then(() => console.log('[AiChatService] Sent project connectionId (retry):', connId))
            .catch(err => console.error('[AiChatService] Error sending project connectionId:', err));
        }
      }, 2000);
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
    // Defense in depth: never start a second default-channel turn while one is streaming.
    // The component already guards on isStreaming, but a programmatic caller must not be
    // able to reassign currentStreamingMessageId and race a live prompt (would kill any
    // running sub-agent). Interrupt via cancelPrompt() (Stop) instead.
    if (this._isStreaming$.value) {
      console.warn('[AiChatService] sendMessage ignored: a prompt is already streaming. Use Stop to interrupt.');
      return;
    }

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
      this._isStreaming$.next(true);
      this.hubConnection.invoke('SendMessage', message, 'default')
        .catch(err => {
          console.error('Error sending message:', err);
          this.addMessage('system', `Failed to send message: ${err}`);
          this._isStreaming$.next(false);
        });
    }
  }

  /**
   * Ask the backend to abort the prompt currently streaming on the default channel
   * (user pressed Stop). The backend ends the turn cleanly and sends StreamComplete,
   * which clears the streaming state; we optimistically clear it here too.
   */
  cancelPrompt(): void {
    if (this.hubConnection.state !== 'Connected') {
      this._isStreaming$.next(false);   // niente connessione: non c'è nulla che stia generando
      return;
    }

    // NIENTE OTTIMISMO. Prima si spegneva subito l'indicatore: il pulsante SEMBRAVA aver
    // fermato la generazione mentre il backend continuava — ed era il motivo per cui lo Stop
    // "a volte non funzionava". Lo stato si spegne quando arriva StreamComplete dal server,
    // che è l'unico a sapere davvero quando il turno è finito.
    this.hubConnection.invoke<boolean>('CancelPrompt')
      .then(cancelled => {
        if (!cancelled) {
          // Il server non aveva nulla in volo: allora l'indicatore era già disallineato, e
          // spegnerlo qui è corretto (non stiamo nascondendo un turno vivo).
          console.warn('CancelPrompt: nessun turno in volo lato server');
          this._isStreaming$.next(false);
        }
      })
      .catch(err => {
        console.error('Error cancelling prompt:', err);
        this._isStreaming$.next(false);
      });
  }

  /**
   * Send a message on a specific channel (used by PromptLab cards).
   * Each channel maintains independent conversation history on the backend.
   */
  sendMessageToChannel(message: string, channelId: string): void {
    if (!message.trim()) return;

    console.log(`[AiChatService] sendMessageToChannel hub state: ${this.hubConnection.state}, channelId: ${channelId}`);

    if (this.hubConnection.state === 'Connected') {
      this.hubConnection.invoke('SendMessage', message, channelId)
        .then(() => console.log(`[AiChatService] SendMessage invoked successfully for channel: ${channelId}`))
        .catch(err => {
          console.error(`[AiChatService] Error sending message to channel ${channelId}:`, err);
          this._channelEvent$.next({ type: 'error', data: `Failed to send message: ${err}`, channelId });
        });
    } else {
      console.error(`[AiChatService] Hub NOT connected! State: ${this.hubConnection.state}. Message dropped.`);
    }
  }

  /**
   * Like sendMessageToChannel but the server reads the given project files fresh from
   * disk and injects their content as context (SendMessageWithContext). The client only
   * ships the paths — no large content round-trip over SignalR. Empty paths → plain send.
   */
  sendMessageWithContextToChannel(message: string, channelId: string, contextFiles: string[]): void {
    if (!message.trim()) return;

    if (!contextFiles || contextFiles.length === 0) {
      this.sendMessageToChannel(message, channelId);
      return;
    }

    if (this.hubConnection.state === 'Connected') {
      this.hubConnection.invoke('SendMessageWithContext', message, channelId, contextFiles)
        .catch(err => {
          console.error(`[AiChatService] Error sending message+context to channel ${channelId}:`, err);
          this._channelEvent$.next({ type: 'error', data: `Failed to send message: ${err}`, channelId });
        });
    } else {
      console.error(`[AiChatService] Hub NOT connected! State: ${this.hubConnection.state}. Message dropped.`);
    }
  }

  /**
   * Clear conversation history for a specific channel on the backend.
   */
  clearChannelHistory(channelId: string): void {
    if (this.hubConnection.state === 'Connected') {
      this.hubConnection.invoke('ClearHistory', channelId).catch(err => {
        console.error(`[AiChatService] Error clearing history for channel ${channelId}:`, err);
      });
    }
  }

  /**
   * Get an Observable stream of events filtered for a specific channelId.
   * Each event has { type: 'chunk' | 'thinking' | 'complete' | 'error', data: any }.
   * Used by PromptLab cards to subscribe to their own channel.
   */
  getChannelStream$(channelId: string): Observable<{ type: string; data: any }> {
    return this._channelEvent$.asObservable().pipe(
      filter(event => event.channelId === channelId),
      map(({ type, data }) => ({ type, data }))
    );
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
      this.hubConnection.invoke('ClearHistory', null).catch(err => {
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

  getApplicationPrompt(): Observable<any> {
    return this.http.get(`${this.baseUrl}/application-prompt`);
  }

  setApplicationPrompt(applicationPrompt: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/application-prompt`, { applicationPrompt });
  }
  
  getGpuInfo(): Observable<any> {
    return this.http.get(`${this.baseUrl}/gpu-info`);
  }

  // llama.cpp Backend Management
  getBackendStatus(): Observable<any> {
    return this.http.get(`${this.baseUrl}/backend-status`);
  }

  getAvailableBackends(): Observable<any> {
    return this.http.get(`${this.baseUrl}/backends`);
  }

  downloadBackend(variant: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/backends/download/${variant}`, {});
  }

  deleteBackend(): Observable<any> {
    return this.http.delete(`${this.baseUrl}/backends`);
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
    this._lastChatMode = { provider, modelId };

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
    } else {
      // Hub not connected yet (boot-time race): queue so startConnection() replays it
      // before getModelStatus() — otherwise the backend reports no provider and wipes
      // the locally-updated state back to not-loaded.
      this._pendingChatMode = { provider, modelId };
      console.log('[AiChatService] Hub not connected — queued SetChatMode:', provider, modelId);
    }
  }

  /**
   * Async version of setProvider — awaits the hub invoke before returning.
   * Sets `isConfiguringProvider$` to true while the hub invoke is in flight
   * so the UI can show a spinner until the backend has registered the provider.
   */
  async setProviderAsync(provider: string, modelId: string | null): Promise<void> {
    console.log('[AiChatService] setProviderAsync called with:', provider, modelId);
    this._lastChatMode = { provider, modelId };

    if (provider === 'gemini') {
      this._useGemini$.next(true);
      if (modelId) {
        this._geminiModel$.next(modelId);
      }
    } else {
      this._useGemini$.next(false);
    }

    if (this.hubConnection.state !== 'Connected') {
      return;
    }

    this._isConfiguringProvider$.next(true);
    try {
      await this.hubConnection.invoke('SetChatMode', provider, modelId);
      console.log('[AiChatService] SetChatMode completed for:', provider, modelId);
    } finally {
      this._isConfiguringProvider$.next(false);
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

  /** Cached models from DB — populated by getCachedModels() */
  private _cachedModels: { value: string; label: string }[] | null = null;
  private _refreshInFlight: Observable<any> | null = null;

  get cachedModels() { return this._cachedModels; }

  /**
   * Load models from the DB cache (instant, no Copilot CLI call).
   */
  getCachedModels(): Observable<{ value: string; label: string }[]> {
    if (this._cachedModels?.length) {
      return of(this._cachedModels);
    }
    return this.http.get<any>('/api/aimodels/cached').pipe(
      map(response => {
        const models = (response?.models || []).map((m: any) => ({
          value: m.id || m.modelId,
          label: m.name || m.id || m.modelId
        }));
        this._cachedModels = models;
        return models;
      })
    );
  }

  /**
   * Refresh models via Copilot CLI discovery (slow, ~5s).
   * Deduplicates concurrent calls. Updates DB cache on the backend.
   */
  refreshCopilotCliModels(): Observable<any> {
    if (this._refreshInFlight) {
      return this._refreshInFlight;
    }
    this._refreshInFlight = this.http.post('/api/copilotcli/refresh-models', {}).pipe(
      tap((response: any) => {
        const modelList = response?.models || [];
        if (modelList.length) {
          this._cachedModels = modelList.map((m: any) => ({
            value: m.id || m.Id || m,
            label: m.name || m.Name || m.id || m.Id || m
          }));
        }
      }),
      finalize(() => { this._refreshInFlight = null; }),
      shareReplay(1)
    );
    return this._refreshInFlight;
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