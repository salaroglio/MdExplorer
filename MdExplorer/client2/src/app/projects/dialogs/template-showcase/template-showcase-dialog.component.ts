import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { TranslateService } from '@ngx-translate/core';
import { MatLegacyDialogRef as MatDialogRef } from '@angular/material/legacy-dialog';
import { Subscription, firstValueFrom } from 'rxjs';
import { ThemeService } from '../../../services/theme.service';
import { TemplateCatalogEntry, TemplateShowcaseChoice, TemplateShowcaseEntry } from '../project-create-config/project-create-config.model';

/** Una riga della colonna di sinistra: lo strato comune, un metodo, la demo o il progetto vuoto (dall'elenco o per indirizzo). */
interface ShowcaseItem {
  /** null = il progetto vuoto. */
  url: string | null;
  /** L'id del template (null per il progetto vuoto): due righe con lo stesso id sono lo stesso template. */
  id: string | null;
  /** Aggiunto per indirizzo: la riga mostra l'indirizzo, che è ciò che lo distingue. */
  byAddress?: boolean;
  kind: 'empty' | 'foundation' | 'project' | 'method' | 'city';
  title: string;
  description: string;
  languages: string[];
}

/**
 * La vetrina di «Crea nuovo progetto» (sprint 2026-10-19-Vetrina-Dei-Template.md, VD1; 2026-10-19-Metodi-Componibili.md,
 * M4): a sinistra tre modi di far nascere il progetto — comporlo (lo strato comune, sempre, più i metodi spuntati, ognuno
 * con la sua teoria se la si chiede), la demo che ha tutto incluso, o la cartella così com'è — e la lingua in fondo; a
 * destra le slide del template che si guarda. Guardare (clic sulla riga) non è scegliere (pallino o casella). La cartella
 * e la configurazione vengono dopo, nella finestra che apre chi la chiama.
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
  /** La città degli agenti (sprint 2026-10-20-Citta-Degli-Agenti-Come-Funzione.md, C5): una funzione, con il suo deck dentro MDE. */
  city: ShowcaseItem;
  /** La città scelta, e con lei la sua documentazione (CF1, CF3). */
  agentCity = false;
  agentCityGuide = false;
  /** Lo strato comune (MC2): null finché l'elenco non ne ha uno; allora si compone con i soli metodi. */
  foundation: ShowcaseItem | null = null;
  /** I template di progetto, cioè la demo: hanno tutto incluso e stanno da soli (MC6). */
  bases: ShowcaseItem[] = [];
  methods: ShowcaseItem[] = [];
  catalogLoaded = false;
  catalogError = '';

  /** Come nasce il progetto: {@link Compose}, l'indirizzo di una demo, o '' = la cartella così com'è. */
  mode = TemplateShowcaseDialogComponent.Compose;
  static readonly Compose = 'compose';
  chosen: { [url: string]: boolean } = {};
  /** La teoria chiesta, per indirizzo (MC5). */
  theory: { [url: string]: boolean } = {};

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
      url: null, id: null, kind: 'empty', languages: ['it', 'en'],
      title: this.translate.instant('TEMPLATE_SHOWCASE.EMPTY'),
      description: this.translate.instant('TEMPLATE_SHOWCASE.EMPTY_DESC'),
    };
    this.city = {
      url: null, id: null, kind: 'city', languages: ['it', 'en'],
      title: this.translate.instant('TEMPLATE_SHOWCASE.AGENT_CITY'),
      description: this.translate.instant('TEMPLATE_SHOWCASE.AGENT_CITY_DESC'),
    };
    this.viewed = this.empty;
  }

  ngOnInit(): void {
    this.language = this.translate.currentLang === 'en' ? 'en' : 'it';
    this.http.get<{ templates: TemplateCatalogEntry[] }>('../api/MdProjects/TemplateCatalog').subscribe({
      next: res => {
        const items = (res?.templates || []).map(t => ({
          url: t.url, id: t.id, kind: t.kind, languages: t.languages || [], title: this.text(t.title), description: this.text(t.description),
        } as ShowcaseItem));
        this.foundation = items.find(i => i.kind === 'foundation') || null;
        this.bases = items.filter(i => i.kind === 'project');
        this.methods = items.filter(i => i.kind === 'method');
        this.catalogLoaded = true;
        // Senza lo strato comune si parte dalla cartella così com'è, come prima dei metodi componibili.
        if (this.foundation) this.show(this.foundation);
        else this.mode = '';
        this.describeComposable();
      },
      error: err => { this.catalogLoaded = true; this.catalogError = err?.error?.error || err?.message || ''; },
    });
    this.show(this.empty);
  }

  ngOnDestroy(): void { this.subscriptions.unsubscribe(); }

  get composing(): boolean { return this.mode === TemplateShowcaseDialogComponent.Compose; }

  /** Le righe che si compongono: lo strato comune (se c'è) e i metodi. */
  private get composable(): ShowcaseItem[] { return [...(this.foundation ? [this.foundation] : []), ...this.methods]; }

  /** La riga entra nel progetto: lo strato comune sempre, un metodo se spuntato; tutto solo componendo. */
  isIn(item: ShowcaseItem): boolean {
    return this.composing && this.speaks(item) && (item === this.foundation || !!this.chosen[item.url!]);
  }

  /** Il template ha la teoria nella lingua scelta: lo si sa dopo averlo letto (le righe si leggono in sottofondo). */
  hasTheory(item: ShowcaseItem): boolean { return !!this.described.get(`${this.language}|${item.url}`)?.theory; }

  /** Si legge in sottofondo ogni riga componibile, per sapere quali hanno la teoria; un errore lo mostra la riga guardata. */
  private describeComposable(): void {
    for (const item of this.composable)
      this.describe(item.url!).catch(() => { /* la casella resta spenta; il perché si vede guardandolo */ });
  }

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
    if (item.kind === 'empty' || item.kind === 'city') {
      this.deckUrl = this.trusted(`/api/templateshowcase/${item.kind === 'city' ? 'agent-city' : 'empty'}?language=${this.language}`);
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

  /** La lingua cambia: le slide si rifanno, un metodo che non la parla perde la spunta, la teoria si rilegge. */
  onLanguageChange(): void {
    for (const m of this.methods) if (this.chosen[m.url!] && !this.speaks(m)) this.chosen[m.url!] = false;
    const base = this.bases.find(b => b.url === this.mode);
    if (base && !this.speaks(base)) this.mode = TemplateShowcaseDialogComponent.Compose;
    this.theory = {};
    this.describeComposable();
    this.show(this.viewed);
  }

  /** Un template fuori elenco, per indirizzo: entra nella colonna come gli altri, già scelto. */
  async addOther(): Promise<void> {
    const url = this.otherUrl.trim();
    this.otherError = '';
    if (!url) return;
    if ([...this.composable, ...this.bases].some(i => i.url === url)) { this.otherError = this.translate.instant('TEMPLATE_SHOWCASE.ALREADY_LISTED'); return; }
    this.otherLoading = true;
    try {
      const entry = await this.describe(url);
      // Lo stesso template da un altro indirizzo: due righe uguali, e la creazione lo riceverebbe due volte.
      const twin = [...this.composable, ...this.bases].find(i => i.id === entry.id);
      if (twin) {
        this.otherError = this.translate.instant('TEMPLATE_SHOWCASE.SAME_TEMPLATE', { title: twin.title, url: twin.url });
        setTimeout(() => this.otherErrorText?.nativeElement.scrollIntoView({ block: 'nearest' }));
        return;
      }
      const item: ShowcaseItem = { url, id: entry.id, kind: entry.kind, title: entry.title, description: entry.description, languages: entry.languages, byAddress: true };
      if (item.kind === 'foundation') {
        // Lo strato comune è uno (MC2): per indirizzo entra solo se l'elenco non ne ha.
        if (this.foundation) {
          this.otherError = this.translate.instant('TEMPLATE_SHOWCASE.ONE_COMMON_LAYER', { title: this.foundation.title });
          setTimeout(() => this.otherErrorText?.nativeElement.scrollIntoView({ block: 'nearest' }));
          return;
        }
        this.foundation = item;
        this.mode = TemplateShowcaseDialogComponent.Compose;
      } else if (item.kind === 'project') {
        this.bases = [...this.bases, item];
        if (this.speaks(item)) this.mode = url;
      } else {
        this.methods = [...this.methods, item];
        this.chosen[url] = this.speaks(item);
        if (this.speaks(item)) this.mode = TemplateShowcaseDialogComponent.Compose;
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

  /** Componendo senza strato comune (l'elenco non ne ha) serve almeno un metodo. */
  get canGoOn(): boolean { return !this.composing || this.composable.some(i => this.isIn(i)); }

  onNext(): void {
    if (!this.canGoOn) return;
    if (!this.composing) {
      const demo = this.bases.find(b => b.url === this.mode) || null;
      this.dialogRef.close({
        baseUrl: demo?.url || null, methodUrls: [], theoryUrls: [], language: this.language,
        titles: demo ? [demo.title] : [], asksEngine: !demo, agentCity: false, agentCityGuide: false,
      });
      return;
    }
    const included = this.composable.filter(i => this.isIn(i));
    const first = included[0];
    this.dialogRef.close({
      // Il primo template è lo strato comune; senza, il primo metodo (finché l'elenco non ha lo strato comune).
      baseUrl: first.url,
      methodUrls: included.slice(1).map(i => i.url!),
      theoryUrls: included.filter(i => this.theory[i.url!] && this.hasTheory(i)).map(i => i.url!),
      language: this.language,
      titles: [...included.map(i => i.title), ...(this.agentCity ? [this.city.title] : [])],
      // Né lo strato comune né un metodo portano il motore: lo sceglie chi crea.
      asksEngine: true,
      agentCity: this.agentCity,
      agentCityGuide: this.agentCity && this.agentCityGuide,
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
