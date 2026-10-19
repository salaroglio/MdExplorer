import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { TranslateService } from '@ngx-translate/core';
import { MatLegacyDialogRef as MatDialogRef } from '@angular/material/legacy-dialog';
import { Subscription, firstValueFrom } from 'rxjs';
import { ThemeService } from '../../../services/theme.service';
import { TemplateCatalogEntry, TemplateShowcaseChoice, TemplateShowcaseEntry } from '../project-create-config/project-create-config.model';

/** Una riga della colonna di sinistra: il progetto vuoto, una base o un metodo (dall'elenco o aggiunto per indirizzo). */
interface ShowcaseItem {
  /** null = il progetto vuoto. */
  url: string | null;
  kind: 'empty' | 'project' | 'method';
  title: string;
  description: string;
  languages: string[];
}

/**
 * La vetrina di «Crea nuovo progetto» (sprint 2026-10-19-Vetrina-Dei-Template.md, VD1): a sinistra il progetto vuoto, le
 * basi e i metodi, la lingua in fondo; a destra le slide del template che si guarda. Guardare (clic sulla riga) non è
 * scegliere (pallino o casella). La cartella e la configurazione vengono dopo, nella finestra che apre chi la chiama.
 */
@Component({
  selector: 'app-template-showcase-dialog',
  templateUrl: './template-showcase-dialog.component.html',
  styleUrls: ['./template-showcase-dialog.component.scss']
})
export class TemplateShowcaseDialogComponent implements OnInit, OnDestroy {

  readonly languages = [{ code: 'it', name: 'italiano' }, { code: 'en', name: 'English' }];
  language = 'it';

  empty: ShowcaseItem;
  bases: ShowcaseItem[] = [];
  methods: ShowcaseItem[] = [];
  catalogLoaded = false;
  catalogError = '';

  /** La base scelta: '' = il progetto vuoto. */
  baseUrl = '';
  chosen: { [url: string]: boolean } = {};

  /** La riga che si sta guardando, e la sua pagina (un campo: l'iframe non si ricarica a ogni ciclo). */
  viewed: ShowcaseItem;
  deckUrl: SafeResourceUrl | null = null;
  deckLoading = false;
  deckError = '';
  /** «Solo in: italiano» sopra il deck, quando le slide sono in un'altra lingua. */
  deckOnlyIn = '';

  otherUrl = '';
  otherError = '';
  /** Il messaggio sta in fondo alla colonna che scorre: quando compare, va portato in vista. */
  @ViewChild('otherErrorText') otherErrorText?: ElementRef<HTMLElement>;
  otherLoading = false;

  private theme = 'light';
  /** Quello che il servizio ha detto di un template in una lingua: non si richiede a ogni clic. */
  private described = new Map<string, TemplateShowcaseEntry>();
  private subscriptions = new Subscription();

  constructor(
    public dialogRef: MatDialogRef<TemplateShowcaseDialogComponent, TemplateShowcaseChoice | null>,
    private http: HttpClient,
    private sanitizer: DomSanitizer,
    private translate: TranslateService,
    themeService: ThemeService,
  ) {
    this.subscriptions.add(themeService.currentTheme$.subscribe(t => this.theme = t));
    this.empty = {
      url: null, kind: 'empty', languages: ['it', 'en'],
      title: this.translate.instant('TEMPLATE_SHOWCASE.EMPTY'),
      description: this.translate.instant('TEMPLATE_SHOWCASE.EMPTY_DESC'),
    };
    this.viewed = this.empty;
  }

  ngOnInit(): void {
    this.language = this.translate.currentLang === 'en' ? 'en' : 'it';
    this.http.get<{ templates: TemplateCatalogEntry[] }>('../api/MdProjects/TemplateCatalog').subscribe({
      next: res => {
        const items = (res?.templates || []).map(t => ({
          url: t.url, kind: t.kind, languages: t.languages || [], title: this.text(t.title), description: this.text(t.description),
        } as ShowcaseItem));
        this.bases = items.filter(i => i.kind === 'project');
        this.methods = items.filter(i => i.kind === 'method');
        this.catalogLoaded = true;
      },
      error: err => { this.catalogLoaded = true; this.catalogError = err?.error?.error || err?.message || ''; },
    });
    this.show(this.empty);
  }

  ngOnDestroy(): void { this.subscriptions.unsubscribe(); }

  /** Il template c'è nella lingua scelta: un metodo che non la parla non si spunta (VD5). */
  speaks(item: ShowcaseItem): boolean { return item.languages.includes(this.language); }

  /** I nomi delle lingue del template nella lingua dell'interfaccia, come li scrive la slide di circostanza. */
  languageNames(item: ShowcaseItem): string {
    const it = this.translate.currentLang === 'it';
    const names: { [code: string]: string } = it ? { it: 'italiano', en: 'inglese' } : { it: 'Italian', en: 'English' };
    return item.languages.map(l => names[l] || l).join(', ');
  }

  /** Guardare: le slide della riga a destra. */
  async show(item: ShowcaseItem): Promise<void> {
    this.viewed = item;
    this.deckError = '';
    this.deckOnlyIn = '';
    if (item.kind === 'empty') {
      this.deckUrl = this.trusted(`/api/templateshowcase/empty?language=${this.language}`);
      return;
    }
    this.deckUrl = null;
    this.deckLoading = true;
    try {
      const entry = await this.describe(item.url!);
      if (this.viewed !== item) return; // nel frattempo si guarda un'altra riga
      if (!entry.speaks)
        this.deckOnlyIn = this.translate.instant('PROJECT_CONFIG.TEMPLATE_ONLY_IN', { languages: this.languageNames(item) });
      this.deckUrl = this.trusted(entry.page);
    } catch (err: any) {
      if (this.viewed === item) this.deckError = err?.error?.error || err?.message || String(err);
    } finally {
      if (this.viewed === item) this.deckLoading = false;
    }
  }

  /** La lingua cambia: le slide si rifanno e un metodo che non la parla perde la spunta. */
  onLanguageChange(): void {
    for (const m of this.methods) if (this.chosen[m.url!] && !this.speaks(m)) this.chosen[m.url!] = false;
    const base = this.bases.find(b => b.url === this.baseUrl);
    if (base && !this.speaks(base)) this.baseUrl = '';
    this.show(this.viewed);
  }

  /** Un template fuori elenco, per indirizzo: entra nella colonna come gli altri, già scelto. */
  async addOther(): Promise<void> {
    const url = this.otherUrl.trim();
    this.otherError = '';
    if (!url) return;
    if ([...this.bases, ...this.methods].some(i => i.url === url)) { this.otherError = this.translate.instant('TEMPLATE_SHOWCASE.ALREADY_LISTED'); return; }
    this.otherLoading = true;
    try {
      const entry = await this.describe(url);
      const item: ShowcaseItem = { url, kind: entry.kind, title: entry.title, description: entry.description, languages: entry.languages };
      if (item.kind === 'project') {
        this.bases = [...this.bases, item];
        if (this.speaks(item)) this.baseUrl = url;
      } else {
        this.methods = [...this.methods, item];
        this.chosen[url] = this.speaks(item);
      }
      this.otherUrl = '';
      this.show(item);
    } catch (err: any) {
      this.otherError = err?.error?.error || err?.message || String(err);
      setTimeout(() => this.otherErrorText?.nativeElement.scrollIntoView({ block: 'nearest' }));
    } finally {
      this.otherLoading = false;
    }
  }

  onNext(): void {
    const methods = this.methods.filter(m => this.chosen[m.url!] && this.speaks(m));
    const base = this.bases.find(b => b.url === this.baseUrl) || null;
    this.dialogRef.close({
      baseUrl: base?.url || null,
      methodUrls: methods.map(m => m.url!),
      language: this.language,
      titles: [...(base ? [base.title] : []), ...methods.map(m => m.title)],
    });
  }

  onCancel(): void { this.dialogRef.close(null); }

  private async describe(url: string): Promise<TemplateShowcaseEntry> {
    const key = `${this.language}|${url}`;
    const known = this.described.get(key);
    if (known) return known;
    const entry = await firstValueFrom(this.http.get<TemplateShowcaseEntry>('../api/templateshowcase/describe', { params: { url, language: this.language } }));
    this.described.set(key, entry);
    return entry;
  }

  private trusted(page: string): SafeResourceUrl {
    return this.sanitizer.bypassSecurityTrustResourceUrl(`${page}${page.includes('?') ? '&' : '?'}theme=${this.theme}`);
  }

  /** Il testo di una voce dell'elenco nella lingua dell'interfaccia, o nella prima che ha. */
  private text(texts: { [language: string]: string }): string {
    return texts?.[this.translate.currentLang] || texts?.[Object.keys(texts || {})[0]] || '';
  }
}
