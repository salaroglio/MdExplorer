/**
 * Harness agentico del progetto: dove MdExplorer installa le proprie skill, agent e prompt.
 * La scelta è ESCLUSIVA — un progetto opencode non si ritrova anche un .github — e viene
 * scritta in .development.yml, che è committato: viaggia col repository invece di essere
 * ridomandata a ogni macchina.
 */
export type HarnessTarget = 'copilot' | 'opencode' | 'claude' | 'none';

/** Una voce dell'elenco dei template che viaggia con MdExplorer (project-templates.json, P4). */
export interface TemplateCatalogEntry {
    id: string;
    kind: 'foundation' | 'project' | 'method';
    url: string;
    languages: string[];
    title: { [language: string]: string };
    description: { [language: string]: string };
}

/**
 * Il progetto nasce da template (P4): il primo template (lo strato comune o la demo) e i metodi, nella cartella nuova
 * `folderName` dentro quella scelta. `originUrl` vuoto = un origin locale accanto alla cartella.
 */
export interface TemplateChoice {
    baseUrl: string | null;
    methodUrls: string[];
    /** Gli indirizzi di cui si vuole anche la teoria (MC5). */
    theoryUrls: string[];
    /** Il motore lo sceglie chi crea: solo la demo porta il suo. */
    asksEngine: boolean;
    /** La città degli agenti di MDE, e la sua documentazione (C5). */
    agentCity: boolean;
    agentCityGuide: boolean;
    language: string;
    example: boolean;
    folderName: string;
    originUrl: string;
}

export interface ProjectCreateConfigOptions {
    projectPath: string;
    initializeGit: boolean;
    harness: HarnessTarget;
    /**
     * Presente = il progetto nasce da template: git lo fa la creazione; il motore lo porta la base, e senza una base vale
     * `harness` (P5).
     */
    template?: TemplateChoice;
}

/** Che cosa la vetrina dice di un template (api/templateshowcase/describe, sprint 2026-10-19-Vetrina-Dei-Template.md). */
export interface TemplateShowcaseEntry {
    url: string;
    id: string;
    kind: 'foundation' | 'project' | 'method';
    title: string;
    description: string;
    version: string;
    languages: string[];
    /** Il template c'è nella lingua scelta: si può spuntare. */
    speaks: boolean;
    /** La lingua delle slide mostrate; null = slide di circostanza. */
    slidesLanguage: string | null;
    /** Il template ha la teoria nella lingua scelta (MC5). */
    theory: boolean;
    /** La pagina per l'iframe. */
    page: string;
}

/**
 * La scelta fatta nella vetrina (VD1, M4): il primo template (lo strato comune, la demo, o il primo metodo se l'elenco non
 * ha lo strato comune), gli altri metodi, la teoria chiesta, la lingua. Nessun template = il progetto di sempre, la
 * cartella aperta così com'è.
 */
export interface TemplateShowcaseChoice {
    baseUrl: string | null;
    methodUrls: string[];
    theoryUrls: string[];
    /** Il motore lo sceglie chi crea: falso solo per la demo, che porta il suo. */
    asksEngine: boolean;
    /** La città degli agenti scelta fra le funzioni, e la sua documentazione (C5). */
    agentCity: boolean;
    agentCityGuide: boolean;
    language: string;
    /** I titoli di ciò che si è scelto, per ricordarlo nella configurazione. */
    titles: string[];
}
