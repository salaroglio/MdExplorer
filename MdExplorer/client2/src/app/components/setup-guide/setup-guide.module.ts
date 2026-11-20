import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { MaterialModule } from '../../shared/material.module';
import { SetupGuideComponent } from './setup-guide.component';
import { SetupGuideDialogComponent } from './setup-guide-dialog.component';

/** «Che cosa installare»: il componente si usa nella pagina dei progetti (ProjectsModule) e nella finestra (AppModule). */
@NgModule({
  declarations: [SetupGuideComponent, SetupGuideDialogComponent],
  imports: [CommonModule, TranslateModule, MaterialModule],
  exports: [SetupGuideComponent, SetupGuideDialogComponent],
})
export class SetupGuideModule {}
