import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';

export type ThemeMode = 'light' | 'dark' | 'system' | 'milan';
export type ResolvedTheme = 'light' | 'dark' | 'milan';

interface SettingsResponse {
  settings: { name: string; valueString?: string }[];
}

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly DARK_CLASS = 'dark-theme';
  private readonly MILAN_CLASS = 'milan-theme';

  private readonly resolvedThemeSubject = new BehaviorSubject<ResolvedTheme>('light');
  public currentTheme$: Observable<ResolvedTheme> = this.resolvedThemeSubject.asObservable();

  private currentMode: ThemeMode = 'light';
  private mediaQuery: MediaQueryList;
  private mediaQueryHandler: ((e: MediaQueryListEvent) => void) | null = null;
  private electronCleanup: (() => void) | null = null;

  constructor(private http: HttpClient) {
    this.mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');

    // Load theme from backend DB
    this.loadFromBackend();

    // Listen for Electron nativeTheme changes (forwarded from main process)
    if ((window as any).electronAPI?.onThemeChanged) {
      this.electronCleanup = (window as any).electronAPI.onThemeChanged((resolved: string) => {
        if (this.currentMode === 'system') {
          this.applyResolved(resolved === 'dark' ? 'dark' : 'light');
        }
      });
    }
  }

  private loadFromBackend(): void {
    this.http.get<SettingsResponse>('../api/AppSettings/GetSettings').subscribe({
      next: (data) => {
        const themeSetting = data?.settings?.find(s => s.name === 'ThemeMode');
        if (themeSetting?.valueString && ['light', 'dark', 'system', 'milan'].includes(themeSetting.valueString)) {
          this.applyMode(themeSetting.valueString as ThemeMode);
        }
      },
      error: () => {}
    });
  }

  setTheme(mode: ThemeMode): void {
    this.applyMode(mode);

    if ((window as any).electronAPI?.setTheme) {
      (window as any).electronAPI.setTheme(mode);
    }
  }

  getCurrentMode(): ThemeMode {
    return this.currentMode;
  }

  getResolvedTheme(): ResolvedTheme {
    return this.resolvedThemeSubject.value;
  }

  getSupportedThemes(): { code: ThemeMode; label: string; icon: string }[] {
    return [
      { code: 'light', label: 'Light', icon: 'light_mode' },
      { code: 'dark', label: 'Dark', icon: 'dark_mode' },
      { code: 'system', label: 'System', icon: 'settings_brightness' },
      { code: 'milan', label: 'Red', icon: 'favorite' }
    ];
  }

  private applyMode(mode: ThemeMode): void {
    this.currentMode = mode;

    if (this.mediaQueryHandler) {
      this.mediaQuery.removeEventListener('change', this.mediaQueryHandler);
      this.mediaQueryHandler = null;
    }

    if (mode === 'system') {
      this.mediaQueryHandler = (e: MediaQueryListEvent) => {
        this.applyResolved(e.matches ? 'dark' : 'light');
      };
      this.mediaQuery.addEventListener('change', this.mediaQueryHandler);
      this.applyResolved(this.mediaQuery.matches ? 'dark' : 'light');
    } else {
      this.applyResolved(mode);
    }
  }

  private applyResolved(theme: ResolvedTheme): void {
    document.body.classList.remove(this.DARK_CLASS, this.MILAN_CLASS);
    if (theme === 'dark') {
      document.body.classList.add(this.DARK_CLASS);
    } else if (theme === 'milan') {
      document.body.classList.add(this.MILAN_CLASS);
    }
    this.resolvedThemeSubject.next(theme);
  }
}
