import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OpenRecentComponent } from './open-recent/open-recent.component';
import { ProjectsComponent } from './projects.component';
import { NewProjectComponent } from './new-project/new-project.component';
import { CloneProjectComponent } from './dialogs/clone-project/clone-project.component';
import { ModernCloneProjectComponent } from './dialogs/modern-clone-project/modern-clone-project.component';
import { ProjectCreateConfigDialogComponent } from './dialogs/project-create-config/project-create-config-dialog.component';
import { ProjectSettingsComponent } from './project-settings/project-settings.component';
import { RouterModule, Routes } from '@angular/router';
import { MaterialModule } from '../shared/material.module';
import { FormsModule } from '@angular/forms'
import { GitModule } from '../git/git.module';
import { ProjectSettingsService } from './services/project-settings.service';

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
    ProjectSettingsComponent
  ],
  imports: [
    RouterModule.forChild(routes),
    CommonModule,
    MaterialModule,
    FormsModule,
    GitModule,
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
