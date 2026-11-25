import { Component, OnInit, Inject } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA, MatLegacyDialog as MatDialog } from '@angular/material/legacy-dialog';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { GITService } from '../../services/gitservice.service';
import { IBranch, BranchInfo, CheckoutResult } from '../../models/branch';
import { MdServerMessagesService } from '../../../signalR/services/server-messages.service';
import { TranslateService } from '@ngx-translate/core';
import { Observable } from 'rxjs';
import { GitDialogRepo } from '../../components/git-repo-picker/git-repo-picker.component';

export interface GitBranchDialogData {
  projectPath: string;
  projectName?: string;
  /** Il progetto e i suoi submodule: ognuno ha i suoi rami. Assente = solo il progetto. */
  repos?: GitDialogRepo[];
  /** Rilegge i repository: dopo un cambio di ramo del progetto anche i submodule si sono spostati. */
  reloadRepos?: () => Observable<GitDialogRepo[]>;
}

export interface BranchGroup {
  name: string;
  icon: string;
  branches: BranchInfo[];
}

import { NewBranchDialogComponent } from '../new-branch-dialog/new-branch-dialog.component';
@Component({
  selector: 'app-git-branch-dialog',
  templateUrl: './git-branch-dialog.component.html',
  styleUrls: ['./git-branch-dialog.component.scss']
})
export class GitBranchDialogComponent implements OnInit {
  currentBranch: IBranch | null = null;
  branches: BranchInfo[] = [];
  filteredBranches: BranchInfo[] = [];
  branchGroups: BranchGroup[] = [];
  selectedTabIndex: number = 0;
  searchTerm: string = '';
  isLoading = true;
  isSwitching = false;
  error: string | null = null;

  /** I repository fra cui scegliere, e quello di cui si stanno guardando i rami. */
  repos: GitDialogRepo[] = [];
  activeRepo: GitDialogRepo | null = null;
  /** Vero se un ramo è stato cambiato: chi ha aperto il dialogo deve rileggere lo stato. */
  private switched = false;
  /** Perché l'ultimo cambio di ramo è stato rifiutato: dice cosa fare prima. */
  switchRefusal: string | null = null;

  constructor(
    public dialogRef: MatDialogRef<GitBranchDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: GitBranchDialogData,
    private gitService: GITService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
    private serverMessages: MdServerMessagesService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.repos = this.data.repos || [];
    this.activeRepo = this.repos.find(r => r.isRoot) || this.repos[0] || null;
    this.loadBranchInfo();
    this.loadBranches();
  }

  /** Il percorso su cui si agisce: il repository scelto, o il progetto se non c'è scelta. */
  get activePath(): string {
    return this.activeRepo?.path || this.data.projectPath;
  }

  /** Si stanno guardando i rami del progetto, non di un submodule. */
  get onRoot(): boolean {
    return !this.activeRepo || this.activeRepo.isRoot;
  }

  selectRepo(repo: GitDialogRepo): void {
    this.activeRepo = repo;
    this.searchTerm = '';
    this.currentBranch = null;
    this.loadBranchInfo();
    this.loadBranches();
  }

  loadBranchInfo(): void {
    const path = this.activePath;
    this.gitService.modernGetBranchStatus(path).subscribe({
      next: (branch) => {
        if (path !== this.activePath) return;   // nel frattempo si è scelto un altro repository
        this.currentBranch = branch;
        // Il ramo mostrato nella toolbar è quello del PROGETTO: lo stato di un submodule non
        // deve finirci dentro, altrimenti la toolbar mostrerebbe il ramo del submodule.
        if (this.onRoot) this.gitService.currentBranch$.next(branch);
      },
      error: (err) => {
        this.error = this.translate.instant('GIT_BRANCH.LOAD_INFO_ERROR');
        console.error('Error loading branch info:', err);
      }
    });
  }

  loadBranches(): void {
    this.isLoading = true;
    this.error = null;

    this.gitService.getBranches(this.activePath, true).subscribe({
      next: (branches) => {
        this.branches = branches;
        this.filteredBranches = this.sortBranches(branches);
        this.groupBranchesBySource(branches);
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.error = this.translate.instant('GIT_BRANCH.LOAD_LIST_ERROR');
        console.error('Error loading branches:', err);
      }
    });
  }

  groupBranchesBySource(branches: BranchInfo[]): void {
    const groups: Map<string, BranchInfo[]> = new Map();

    // Group local branches
    const localBranches = branches.filter(b => !b.isRemote);
    if (localBranches.length > 0) {
      groups.set('local', this.sortBranches(localBranches));
    }

    // Group remote branches by remoteName
    const remoteBranches = branches.filter(b => b.isRemote);
    remoteBranches.forEach(branch => {
      const remoteName = branch.remoteName || 'origin';
      if (!groups.has(remoteName)) {
        groups.set(remoteName, []);
      }
      groups.get(remoteName)!.push(branch);
    });

    // Sort remote branches within each group
    groups.forEach((branchList, key) => {
      if (key !== 'local') {
        groups.set(key, this.sortBranches(branchList));
      }
    });

    // Build branchGroups array with local first, then remotes alphabetically
    this.branchGroups = [];

    if (groups.has('local')) {
      this.branchGroups.push({
        name: this.translate.instant('GIT_BRANCH.LOCAL'),
        icon: 'computer',
        branches: groups.get('local')!
      });
    }

    // Add remote groups sorted alphabetically
    const remoteKeys = Array.from(groups.keys())
      .filter(k => k !== 'local')
      .sort();

    remoteKeys.forEach(remoteName => {
      this.branchGroups.push({
        name: remoteName,
        icon: 'cloud',
        branches: groups.get(remoteName)!
      });
    });
  }

  sortBranches(branches: BranchInfo[]): BranchInfo[] {
    return branches.sort((a, b) => {
      // Current branch first
      if (a.isCurrentBranch) return -1;
      if (b.isCurrentBranch) return 1;

      // Then local branches
      if (!a.isRemote && b.isRemote) return -1;
      if (a.isRemote && !b.isRemote) return 1;

      // Alphabetically
      return a.name.localeCompare(b.name);
    });
  }

  filterBranches(): void {
    const term = this.searchTerm.toLowerCase();
    if (!term) {
      this.filteredBranches = this.sortBranches(this.branches);
      this.groupBranchesBySource(this.branches);
    } else {
      const filtered = this.branches.filter(b =>
        b.name.toLowerCase().includes(term)
      );
      this.filteredBranches = this.sortBranches(filtered);
      this.groupBranchesBySource(filtered);
    }
  }

  async switchToBranch(branch: BranchInfo): Promise<void> {
    if (branch.isCurrentBranch) {
      this.snackBar.open(this.translate.instant('GIT_BRANCH.ALREADY_ON_BRANCH'), 'OK', { duration: 2000 });
      return;
    }

    if (this.isSwitching) {
      return; // Prevent multiple simultaneous switches
    }

    // Il lavoro aperto: lo si elenca e si chiede conferma qui dentro (prima era un confirm() del browser con un testo
    // che parlava di «perdere le modifiche», e un force che non arrivava mai al servizio). Se non si sa com'è la cartella,
    // non si cambia ramo alla cieca: lo si dice.
    this.gitService.newBranchPreview(this.activePath).subscribe({
      next: (preview) => {
        if (preview.carriedFiles.length > 0) {
          this.pendingSwitch = { branch, files: preview.carriedFiles };
        } else {
          this.performCheckout(branch);
        }
      },
      error: (err) => {
        this.switchRefusal = this.translate.instant('GIT_BRANCH.STATUS_UNKNOWN') + ' ' + (err?.error?.error || err?.message || err);
      }
    });
  }

  /** Il cambio di ramo in attesa di conferma, con i file non registrati che vengono con te. */
  pendingSwitch: { branch: BranchInfo; files: string[] } | null = null;

  confirmSwitch(): void {
    const pending = this.pendingSwitch;
    this.pendingSwitch = null;
    if (pending) this.performCheckout(pending.branch);
  }

  cancelSwitch(): void {
    this.pendingSwitch = null;
  }

  /** «Nuovo ramo» anche da qui: nasce nel repository che si sta guardando. */
  openNewBranch(): void {
    this.dialog.open(NewBranchDialogComponent, {
      width: '620px',
      data: { projectName: this.data.projectName, repos: this.activeRepo ? [this.activeRepo] : this.repos },
    }).afterClosed().subscribe(created => {
      if (!created) return;
      this.switched = true;
      this.loadBranchInfo();
      this.loadBranches();
    });
  }

  private performCheckout(branch: BranchInfo): void {
    this.isSwitching = true;
    this.error = null;
    this.switchRefusal = null;

    // Extract branch name (remove remote prefix if present)
    let branchName = branch.name;
    if (branch.isRemote) {
      // For remote branches, remove the remote prefix (e.g., "origin/test" -> "test")
      // Handle both cases: when remoteName is set or when we need to detect it
      if (branch.remoteName) {
        branchName = branch.name.replace(`${branch.remoteName}/`, '');
      } else {
        // Fallback: remove everything before the last slash
        const slashIndex = branch.name.indexOf('/');
        if (slashIndex !== -1) {
          branchName = branch.name.substring(slashIndex + 1);
        }
      }
    }

    const path = this.activePath;
    const onRoot = this.onRoot;
    this.gitService.checkoutBranch(path, branchName, this.serverMessages.connectionId).subscribe({
      next: (result: CheckoutResult) => {
        this.isSwitching = false;

        if (result.success) {
          const actualBranchName = result.branchName || branchName;
          this.switched = true;
          if (this.activeRepo) { this.activeRepo.branch = actualBranchName; this.activeRepo.detached = false; }

          // Con un avviso da leggere il messaggio resta finché non lo si chiude: dice quale
          // submodule NON è stato portato alla versione del ramo nuovo, e perché.
          const warnings = result.warnings || [];
          this.snackBar.open(
            [this.translate.instant('GIT_BRANCH.SWITCHED_TO', { branch: actualBranchName }), ...warnings].join(' '),
            'OK',
            { duration: warnings.length ? undefined : 3000, panelClass: ['success-snackbar'] }
          );

          // Il ramo della toolbar è quello del progetto: si aggiorna solo se è lui ad aver cambiato ramo.
          if (result.branchName && onRoot) {
            this.gitService.currentBranch$.next({
              id: '',
              name: result.branchName,
              somethingIsChangedInTheBranch: false,
              howManyFilesAreChanged: 0,
              howManyCommitAreToPush: 0,
              fullPath: this.data.projectPath
            });
          }

          // Refresh branch list
          this.loadBranchInfo();
          this.loadBranches();
          this.reloadRepos();
        } else {
          // Un rifiuto dice cosa fare prima (per esempio: un submodule ha lavoro non salvato):
          // resta scritto nel dialogo, senza sparire dopo qualche secondo. Non è un errore di
          // caricamento, quindi l'elenco dei rami resta dov'è.
          this.switchRefusal = result.error || this.translate.instant('GIT_BRANCH.SWITCH_ERROR');
        }
      },
      error: (err) => {
        this.isSwitching = false;
        this.error = this.translate.instant('GIT_BRANCH.NETWORK_ERROR');
        console.error('Checkout error:', err);
        this.snackBar.open(
          this.error,
          'OK',
          { duration: 5000, panelClass: ['error-snackbar'] }
        );
      }
    });
  }

  /**
   * Dopo un cambio di ramo il selettore diceva ancora il ramo di prima — e, cambiando ramo al
   * progetto, anche quello dei submodule, che nel frattempo si erano spostati.
   */
  private reloadRepos(): void {
    if (!this.data.reloadRepos) return;
    const activePath = this.activePath;
    this.data.reloadRepos().subscribe({
      next: repos => {
        if (!repos.length) return;
        this.repos = repos;
        this.activeRepo = repos.find(r => r.path === activePath) || repos.find(r => r.isRoot) || repos[0];
      },
      error: err => console.error('Error reloading repositories:', err),
    });
  }

  onClose(): void {
    this.dialogRef.close(this.switched);
  }
}
