import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { Observable } from 'rxjs';
import { IndexingProgressService, IndexingProgressState } from '../../services/indexing-progress.service';

/**
 * Snackbar custom mostrata in basso a destra durante l'indicizzazione
 * del progetto. UNA sola istanza per tutta la durata della pipeline:
 * apertura via MatSnackBar.openFromComponent in md-tree.component.ts,
 * stato pilotato da IndexingProgressService (alimentato dagli eventi
 * SignalR knowledgeProgress).
 *
 * Pattern intenzionalmente diverso dal vecchio MatSnackBar.open()
 * ripetuto per ogni cartella (che faceva flicker). Qui la snackbar
 * vive una sola volta e il contenuto si aggiorna via Observable.
 */
@Component({
  selector: 'app-indexing-progress-snack',
  templateUrl: './indexing-progress-snack.component.html',
  styleUrls: ['./indexing-progress-snack.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class IndexingProgressSnackComponent implements OnInit {

  public state$: Observable<IndexingProgressState>;

  constructor(private indexingProgress: IndexingProgressService) {}

  ngOnInit(): void {
    this.state$ = this.indexingProgress.state$;
  }
}
