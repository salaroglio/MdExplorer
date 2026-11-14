import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { MaterialModule } from '../../shared/material.module';
import { LanguageLibraryComponent } from './language-library.component';

/** La libreria dei linguaggi del grafo: nelle impostazioni generali (AppModule) e in quelle del progetto (ProjectsModule). */
@NgModule({
  declarations: [LanguageLibraryComponent],
  imports: [CommonModule, TranslateModule, MaterialModule],
  exports: [LanguageLibraryComponent],
})
export class LanguageLibraryModule { }
