/** Le risposte di api/SetupGuide/Status (SetupGuideController). */
export interface HarnessEnvironment {
  id: string;
  installed: boolean;
  usable: boolean;
  reason?: string;
  detail?: string;
}

export interface SetupGuideStatus {
  os: 'windows' | 'linux' | 'macos';
  copilotCliVersion?: string;
  harness: { environments?: HarnessEnvironment[]; usable?: string[]; error?: string };
  java: {
    ok?: boolean; outcome?: string; path?: string; major?: number; bestPath?: string; bestMajor?: number;
    configuredPath?: string; minimum: number; error?: string;
  };
  pandoc: { usable: boolean; needsRestart: boolean; path?: string; version?: string; error?: string };
}
