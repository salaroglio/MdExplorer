import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private readonly STORAGE_KEY = 'mdexplorer_language';
  private readonly SUPPORTED_LANGS = ['en', 'it'];

  constructor(private translate: TranslateService) {
    this.translate.addLangs(this.SUPPORTED_LANGS);
    this.translate.setDefaultLang('en');

    const saved = localStorage.getItem(this.STORAGE_KEY);
    const lang = saved && this.SUPPORTED_LANGS.includes(saved)
      ? saved
      : (navigator.language?.startsWith('it') ? 'it' : 'en');

    this.translate.use(lang);
    localStorage.setItem(this.STORAGE_KEY, lang);

    // Always sync language to Electron main process at startup
    // (fixes tray menu staying in English when frontend auto-detected Italian)
    if ((window as any).electronAPI?.setLanguage) {
      (window as any).electronAPI.setLanguage(lang);
    }
  }

  setLanguage(lang: string): void {
    if (!this.SUPPORTED_LANGS.includes(lang)) return;
    this.translate.use(lang);
    localStorage.setItem(this.STORAGE_KEY, lang);

    // Notify Electron of language change (for tray menu, dialogs, etc.)
    if ((window as any).electronAPI?.setLanguage) {
      (window as any).electronAPI.setLanguage(lang);
    }
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
}
