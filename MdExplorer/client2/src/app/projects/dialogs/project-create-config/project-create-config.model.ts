/**
 * Harness agentico del progetto: dove MdExplorer installa le proprie skill, agent e prompt.
 * La scelta è ESCLUSIVA — un progetto opencode non si ritrova anche un .github — e viene
 * scritta in .development.yml, che è committato: viaggia col repository invece di essere
 * ridomandata a ogni macchina.
 */
export type HarnessTarget = 'copilot' | 'opencode' | 'none';

export interface ProjectCreateConfigOptions {
    projectPath: string;
    initializeGit: boolean;
    harness: HarnessTarget;
}
