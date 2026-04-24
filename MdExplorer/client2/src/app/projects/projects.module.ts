import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OpenRecentComponent } from './open-recent/open-recent.component';
import { ProjectsComponent } from './projects.component';
import { NewProjectComponent } from './new-project/new-project.component';
import { CloneProjectComponent } from './dialogs/clone-project/clone-project.component';
import { ModernCloneProjectComponent } from './dialogs/modern-clone-project/modern-clone-project.component';
import { ProjectCreateConfigDialogComponent } from './dialogs/project-create-config/project-create-config-dialog.component';
import { ProjectEditDialogComponent } from './dialogs/project-edit/project-edit-dialog.component';
import { ProjectSettingsComponent } from './project-settings/project-settings.component';
import { P2PManagerComponent } from './dialogs/p2p-manager/p2p-manager.component';
import { CatalogPickerDialogComponent } from './dialogs/catalog-picker/catalog-picker.component';
import { RouterModule, Routes } from '@angular/router';
import { MaterialModule } from '../shared/material.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { GitModule } from '../git/git.module';
import { ProjectSettingsService } from './services/project-settings.service';
import { ParticipantGemsModule } from '../components/participant-gems/participant-gems.module';

const routes: Routes = [
  {
    path: '', component: ProjectsComponent,
    children: [
      { path: '', redirectTo: 'openrecent', pathMatch: 'full' },
      { path: 'openrecent', component: OpenRecentComponent },
      { path: 'newproject', component: NewProjectComponent },
    ]
  }];


@NgModule({
  declarations: [
    OpenRecentComponent,
    ProjectsComponent,
    NewProjectComponent,
    CloneProjectComponent,
    ModernCloneProjectComponent,
    ProjectCreateConfigDialogComponent,
    ProjectEditDialogComponent,
    ProjectSettingsComponent,
    P2PManagerComponent,
    CatalogPickerDialogComponent
  ],
  imports: [
    RouterModule.forChild(routes),
    CommonModule,
    MaterialModule,
    FormsModule,
    ReactiveFormsModule,
    GitModule,
    TranslateModule,
    ParticipantGemsModule,
  ],
  providers: [
    ProjectSettingsService
  ]
})
export class ProjectsModule {
  constructor() {
    console.log('constructor ProjectsModule');
  }
}
