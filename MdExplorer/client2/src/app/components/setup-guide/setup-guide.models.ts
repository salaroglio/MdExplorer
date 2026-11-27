/** Le risposte di api/SetupGuide/Status (SetupGuideController). */
export interface HarnessEnvironment {
  id: string;
  installed: boolean;
  usable: boolean;
  reason?: string;
  detail?: string;
}

/** Un programma che MDE lancia per nome dal PATH (Pandoc, git). */
export interface ExecutableStatus { usable: boolean; needsRestart: boolean; path?: string; version?: string; error?: string; }

/** Un'installazione con winget lanciata dalla pagina (api/SetupGuide/Install). */
export interface InstallJob {
  tool: string; packageId: string; state: 'running' | 'succeeded' | 'failed';
  lastLine?: string; exitCode?: number; alreadyInstalled?: boolean; error?: string; tail?: string[];
}

export interface SetupGuideStatus {
  os: 'windows' | 'linux' | 'macos';
  copilotCliVersion?: string;
  harness: { environments?: HarnessEnvironment[]; usable?: string[]; error?: string };
  java: {
    ok?: boolean; outcome?: string; path?: string; major?: number; bestPath?: string; bestMajor?: number;
    configuredPath?: string; minimum: number; error?: string;
  };
  pandoc: ExecutableStatus;
  git: ExecutableStatus;
  /** Solo su Windows: se c'è winget, «Installa adesso» si può fare. */
  winget?: { available: boolean } | null;
}
