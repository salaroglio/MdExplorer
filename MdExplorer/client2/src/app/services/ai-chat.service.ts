import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Observable, Subject, BehaviorSubject } from 'rxjs';

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
  
  // Gemini API state
  private _useGemini$ = new BehaviorSubject<boolean>(false);
  public useGemini$ = this._useGemini$.asObservable();
  
  private _geminiModel$ = new BehaviorSubject<string>('gemini-1.5-flash');
  public geminiModel$ = this._geminiModel$.asObservable();

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

    this.hubConnection.on('ReceiveStreamChunk', (chunk: string) => {
      this._streamingMessage$.next(chunk);
      this.appendToStreamingMessage(chunk);
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

  private finalizeStreamingMessage(): void {
    if (!this.currentStreamingMessageId) return;
    
    const messages = this._messages$.value;
    const messageIndex = messages.findIndex(m => m.id === this.currentStreamingMessageId);
    
    if (messageIndex !== -1) {
      messages[messageIndex].isStreaming = false;
      this._messages$.next([...messages]);
    }
    
    this.currentStreamingMessageId = null;
  }

  clearMessages(): void {
    this._messages$.next([]);
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
  
  notifyGeminiConnected(modelId: string): void {
    // When Gemini is connected, we treat it as a "loaded" model
    this._isModelLoaded$.next(true);
    this._currentModel$.next(`Gemini: ${modelId}`);
    console.log('[AiChatService] Gemini connected:', modelId);
  }
  
  notifyGeminiDisconnected(): void {
    // When Gemini is disconnected, check if we have a local model loaded
    // For now, we'll set to false assuming no local model
    this._isModelLoaded$.next(false);
    this._currentModel$.next(null);
    console.log('[AiChatService] Gemini disconnected');
  }

  ngOnDestroy(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }
}