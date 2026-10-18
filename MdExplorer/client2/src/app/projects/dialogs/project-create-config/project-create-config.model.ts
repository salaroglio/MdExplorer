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
    kind: 'project' | 'method';
    url: string;
    languages: string[];
    title: { [language: string]: string };
    description: { [language: string]: string };
}

/**
 * Il progetto nasce da template (P4): una base facoltativa e dei metodi, nella cartella nuova `folderName` dentro quella
 * scelta. `originUrl` vuoto = un origin locale accanto alla cartella.
 */
export interface TemplateChoice {
    baseUrl: string | null;
    methodUrls: string[];
    language: string;
    example: boolean;
    folderName: string;
    originUrl: string;
}

export interface ProjectCreateConfigOptions {
    projectPath: string;
    initializeGit: boolean;
    harness: HarnessTarget;
    /** Presente = il progetto nasce da template; harness e git li decide il template. */
    template?: TemplateChoice;
}
