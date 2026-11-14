import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { ProjectSettingsService } from '../../projects/services/project-settings.service';

/**
 * Stato del master switch "città degli agenti" (`agentCity.enabled` in
 * .development.yml) per il progetto aperto.
 *
 * Vive in un service condiviso perché i due punti che lo usano sono lontani fra
 * loro: la toolbar, che mostra o nasconde i comandi della città, e le impostazioni
 * di progetto, che lo cambiano. Senza un canale comune la toolbar resterebbe ferma
 * allo stato letto all'apertura del progetto.
 */
@Injectable({ providedIn: 'root' })
export class AgentCityStateService {
  private enabledSubject = new BehaviorSubject<boolean>(false);
  /** Progetto a cui si riferisce lo stato pubblicato: quello aperto nella toolbar. */
  private currentPath: string = '';

  /** true solo quando il progetto aperto ha la città attiva. */
  enabled$: Observable<boolean> = this.enabledSubject.asObservable();

  constructor(private projectSettings: ProjectSettingsService) {}

  /** Rilegge il flag dal backend. Nessun progetto aperto → città spenta, senza chiamata. */
  refresh(projectPath: string): void {
    this.currentPath = projectPath || '';
    if (!this.currentPath) {
      this.enabledSubject.next(false);
      return;
    }
    const asked = this.currentPath;
    this.projectSettings.getAgentCity(asked).subscribe({
      next: cfg => {
        if (asked === this.currentPath) this.enabledSubject.next(!!cfg?.enabled);
      },
      error: err => {
        // Stato ignoto: nascondiamo i comandi della città e lo dichiariamo, invece
        // di mostrarli come se il flag fosse acceso.
        console.warn('[AgentCity] stato non leggibile, comandi della città nascosti:', err);
        if (asked === this.currentPath) this.enabledSubject.next(false);
      },
    });
  }

  /**
   * Allinea lo stato alla risposta di un salvataggio, senza una rilettura.
   * Il path è obbligatorio: le impostazioni si aprono anche su un progetto della
   * lista diverso da quello aperto, e il suo flag non deve toccare la toolbar.
   */
  set(projectPath: string, enabled: boolean): void {
    if (!projectPath || projectPath !== this.currentPath) return;
    this.enabledSubject.next(!!enabled);
  }
}
