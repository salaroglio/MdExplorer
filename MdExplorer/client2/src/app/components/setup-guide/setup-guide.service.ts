import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MatLegacyDialog as MatDialog } from '@angular/material/legacy-dialog';
import { firstValueFrom } from 'rxjs';
import { SetupGuideDialogComponent } from './setup-guide-dialog.component';

/**
 * «Che cosa installare» (19/11/2026): alla prima installazione, prima del tour di Mark, la pagina dice se ci sono un motore
 * per MarkAgent, Java 21 e Pandoc, e come installarli. Il segno «già mostrata» sta nel DB del servizio (Setting
 * SetupGuideShown), non nel localStorage: l'app desktop cambia porta, e quindi origine, a ogni avvio.
 */
@Injectable({ providedIn: 'root' })
export class SetupGuideService {
  private firstRunCheck: Promise<void> | null = null;

  constructor(private http: HttpClient, private dialog: MatDialog) {}

  /**
   * Alla prima installazione apre la pagina e risolve quando la si chiude; se si è già aperta da sola, risolve subito.
   * Il tour di Mark aspetta questa promessa: prima si installa, poi si impara. Chiamata più volte, apre una pagina sola.
   */
  showIfFirstRun(): Promise<void> {
    if (!this.firstRunCheck) this.firstRunCheck = this.runFirstRun();
    return this.firstRunCheck;
  }

  /** Dal menu delle impostazioni: si riapre quando si vuole, e non tocca il segno «già mostrata». */
  open(): Promise<void> {
    const ref = this.dialog.open(SetupGuideDialogComponent, {
      width: '880px',
      maxWidth: '94vw',
      maxHeight: '92vh',
      panelClass: 'setup-guide-panel',
      autoFocus: false,
    });
    return firstValueFrom(ref.afterClosed()).then(() => undefined);
  }

  private async runFirstRun(): Promise<void> {
    let shown: boolean;
    try {
      shown = (await firstValueFrom(this.http.get<{ shown: boolean }>('../api/SetupGuide/Shown'))).shown;
    } catch (e) {
      // Il servizio non risponde: la pagina non si apre (il tour di Mark parte lo stesso), ma lo si dice nella console.
      console.error('[SetupGuide] impossibile sapere se la pagina si è già aperta:', e);
      return;
    }
    if (shown) return;
    await this.open();
    try {
      await firstValueFrom(this.http.post('../api/SetupGuide/MarkShown', {}));
    } catch (e) {
      console.error('[SetupGuide] segno «già mostrata» non salvato: si riaprirà al prossimo avvio', e);
    }
  }
}
