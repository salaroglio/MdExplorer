import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { TranslateService } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';

/**
 * The UI language. The source of truth is the backend (user settings DB, `GET/POST api/AppSettings/*Language`):
 * localStorage belongs to one origin, and the desktop app gets a new port at every start, so a language kept
 * only there was lost at every restart (12/10/2026). The backend makes it work for the web edition too; Electron
 * is a shell and is only told (tray menu).
 *
 * localStorage `mdexplorer_language` is still written, as a copy for the pages inside the document iframe
 * (same origin, e.g. the PlantUML legend), which read it from there.
 */
@Injectable({ providedIn: 'root' })
export class LanguageService {
  private readonly STORAGE_KEY = 'mdexplorer_language';
  private readonly SUPPORTED_LANGS = ['en', 'it'];
  private readonly api = '../api/AppSettings';

  constructor(private translate: TranslateService, private http: HttpClient) {
    this.translate.addLangs(this.SUPPORTED_LANGS);
    this.translate.setDefaultLang('en');
  }

  /**
   * Called by APP_INITIALIZER, before the first render. The stored language wins; on the first start (nothing
   * stored) the one this browser remembered from before the move to the backend, else the OS one — and it is
   * saved, so from then on it is the backend's.
   */
  async init(): Promise<void> {
    let stored: string | null = null;
    try {
      stored = (await firstValueFrom(this.http.get<{ language: string | null }>(`${this.api}/GetLanguage`))).language;
    } catch (err) {
      console.error('[LanguageService] could not read the language from the backend', err);
    }
    if (stored && this.SUPPORTED_LANGS.includes(stored)) {
      this.apply(stored);
      return;
    }
    const remembered = this.readLocal();
    const lang = remembered ?? (navigator.language?.startsWith('it') ? 'it' : 'en');
    this.apply(lang);
    this.save(lang);
  }

  setLanguage(lang: string): void {
    if (!this.SUPPORTED_LANGS.includes(lang)) return;
    this.apply(lang);
    this.save(lang);
  }

  getCurrentLanguage(): string {
    return this.translate.currentLang || 'en';
  }

  getSupportedLanguages(): { code: string; label: string }[] {
    return [
      { code: 'en', label: 'English' },
      { code: 'it', label: 'Italiano' }
    ];
  }

  private apply(lang: string): void {
    this.translate.use(lang);
    try { localStorage.setItem(this.STORAGE_KEY, lang); } catch { /* storage unavailable: only the iframe copy is lost */ }
    // Electron (tray menu, dialogs) is told, it does not decide
    if ((window as any).electronAPI?.setLanguage) {
      (window as any).electronAPI.setLanguage(lang);
    }
  }

  private save(lang: string): void {
    this.http.post(`${this.api}/SetLanguage`, { language: lang }).subscribe({
      error: err => console.error('[LanguageService] could not save the language in the backend', err),
    });
  }

  private readLocal(): string | null {
    try {
      const v = localStorage.getItem(this.STORAGE_KEY);
      return v && this.SUPPORTED_LANGS.includes(v) ? v : null;
    } catch { return null; }
  }
}
