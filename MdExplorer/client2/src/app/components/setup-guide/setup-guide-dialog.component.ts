import { Component } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef } from '@angular/material/legacy-dialog';

/**
 * La finestra di «Che cosa installare», aperta dal menu «?» della barra del titolo (20/11/2026). Non ha grafica sua: ospita
 * lo stesso componente che alla prima installazione sta nella colonna di sinistra della pagina dei progetti.
 */
@Component({
  selector: 'app-setup-guide-dialog',
  template: '<app-setup-guide mode="window" (done)="ref.close()"></app-setup-guide>',
})
export class SetupGuideDialogComponent {
  constructor(public ref: MatDialogRef<SetupGuideDialogComponent>) {}
}
