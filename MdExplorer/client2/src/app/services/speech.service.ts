import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

/** Cosa può fare la dettatura adesso, come lo racconta il Service. */
export interface SpeechStatus {
  available: boolean;
  model: string | null;
  expects: { format: string; sampleRate: number; channels: number };
}

export interface Transcription {
  text: string;
  milliseconds: number;
  model: string;
}

/**
 * La dettatura: qui si registra, il riconoscimento lo fa il Service .NET.
 *
 * Il confine è voluto — la pagina fa la sola cosa che solo lei può fare, aprire il microfono;
 * il modello sta nel Service, dove funziona uguale su Windows e su Linux e da dove l'audio non
 * esce. Vedi docs-internal/Sprints/2026-09-22-Dettatura-Vocale-MarkAgent.md.
 */
@Injectable({ providedIn: 'root' })
export class SpeechService {
  /**
   * ⚠️ Non è una preferenza: Whisper accetta **solo** WAV a 16 kHz e rifiuta tutto il resto
   * (`Only 16KHz sample rate is supported`). Chiedendo un `AudioContext` a questa frequenza è
   * Chromium a ricampionare il microfono, e il Service non ha bisogno né di ffmpeg né di un
   * decoder audio.
   */
  static readonly SAMPLE_RATE = 16000;

  constructor(private http: HttpClient) {}

  status(): Observable<SpeechStatus> {
    return this.http.get<SpeechStatus>('../api/speech/status');
  }

  transcribe(wav: Blob, language = 'it'): Observable<Transcription> {
    const form = new FormData();
    form.append('audio', wav, 'dettatura.wav');
    return this.http.post<Transcription>(`../api/speech/transcribe?language=${language}`, form);
  }

  /** Apre il microfono e comincia a registrare. Il risultato si raccoglie con `stop()`. */
  async record(): Promise<VoiceRecording> {
    const stream = await navigator.mediaDevices.getUserMedia({
      audio: { channelCount: 1, echoCancellation: true, noiseSuppression: true }
    });
    return new VoiceRecording(stream);
  }
}

/**
 * Una registrazione in corso. Raccoglie i campioni a 16 kHz mono e alla fine ne fa un WAV.
 *
 * Usa `createScriptProcessor`, deprecato ma presente ovunque, invece di un `AudioWorklet`: il
 * worklet vuole un modulo caricato a parte (o un blob URL, che la CSP dell'app può rifiutare) per
 * fare esattamente la stessa cosa — copiare campioni in un array. Se un domani sparisse davvero,
 * il pezzo da riscrivere è solo questa classe.
 */
export class VoiceRecording {
  private readonly context: AudioContext;
  private readonly source: MediaStreamAudioSourceNode;
  private readonly processor: ScriptProcessorNode;
  private readonly chunks: Float32Array[] = [];
  private stopped = false;

  constructor(private readonly stream: MediaStream) {
    this.context = new AudioContext({ sampleRate: SpeechService.SAMPLE_RATE });
    this.source = this.context.createMediaStreamSource(stream);
    this.processor = this.context.createScriptProcessor(4096, 1, 1);

    this.processor.onaudioprocess = (e) => {
      if (this.stopped) { return; }
      // Il buffer viene riusato dal browser al giro dopo: si copia, non si tiene il riferimento.
      this.chunks.push(new Float32Array(e.inputBuffer.getChannelData(0)));
    };

    this.source.connect(this.processor);
    // Il processor deve arrivare alla destinazione o non viene mai chiamato. Nessun suono esce:
    // il nodo non scrive niente nel proprio buffer di uscita.
    this.processor.connect(this.context.destination);
  }

  /** Quanti secondi di parlato sono stati raccolti finora. */
  get seconds(): number {
    return this.chunks.reduce((n, c) => n + c.length, 0) / SpeechService.SAMPLE_RATE;
  }

  /** Chiude microfono e contesto audio, e restituisce il WAV da mandare al Service. */
  async stop(): Promise<Blob> {
    this.stopped = true;
    this.processor.disconnect();
    this.source.disconnect();
    this.stream.getTracks().forEach(t => t.stop());
    try { await this.context.close(); } catch { /* già chiuso: non è un problema */ }
    return this.toWav();
  }

  /** Butta via tutto senza produrre niente (l'utente ha annullato). */
  async discard(): Promise<void> {
    this.stopped = true;
    this.processor.disconnect();
    this.source.disconnect();
    this.stream.getTracks().forEach(t => t.stop());
    try { await this.context.close(); } catch { /* idem */ }
    this.chunks.length = 0;
  }

  /** I campioni raccolti come WAV PCM 16 bit mono: l'unico formato che il riconoscimento accetta. */
  private toWav(): Blob {
    const campioni = this.chunks.reduce((n, c) => n + c.length, 0);
    const buffer = new ArrayBuffer(44 + campioni * 2);
    const view = new DataView(buffer);

    const testo = (offset: number, s: string) => {
      for (let i = 0; i < s.length; i++) { view.setUint8(offset + i, s.charCodeAt(i)); }
    };

    testo(0, 'RIFF');
    view.setUint32(4, 36 + campioni * 2, true);
    testo(8, 'WAVE');
    testo(12, 'fmt ');
    view.setUint32(16, 16, true);            // lunghezza del blocco fmt
    view.setUint16(20, 1, true);             // PCM non compresso
    view.setUint16(22, 1, true);             // mono
    view.setUint32(24, SpeechService.SAMPLE_RATE, true);
    view.setUint32(28, SpeechService.SAMPLE_RATE * 2, true);  // byte al secondo
    view.setUint16(32, 2, true);             // byte per campione
    view.setUint16(34, 16, true);            // bit per campione
    testo(36, 'data');
    view.setUint32(40, campioni * 2, true);

    let offset = 44;
    for (const chunk of this.chunks) {
      for (let i = 0; i < chunk.length; i++) {
        // Da float [-1, 1] a intero con segno a 16 bit, tagliando quello che esce dai bordi.
        const v = Math.max(-1, Math.min(1, chunk[i]));
        view.setInt16(offset, v < 0 ? v * 0x8000 : v * 0x7fff, true);
        offset += 2;
      }
    }

    return new Blob([buffer], { type: 'audio/wav' });
  }
}
