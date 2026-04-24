export interface Participant {
  gitEmail: string;
  gitName?: string;
  displayName?: string;
  chatEmail: string;
  manual: boolean;
}

export interface GitAuthor {
  name: string;
  email: string;
  commitCount: number;
}

export interface CurrentGitUser {
  email: string | null;
  name: string | null;
}
