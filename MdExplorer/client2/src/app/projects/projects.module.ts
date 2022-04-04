import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OpenRecentComponent } from './open-recent/open-recent.component';
import { ProjectsComponent } from './projects.component';
import { NewProjectComponent } from './new-project/new-project.component';
import { CloneProjectComponent } from './clone-project/clone-project.component';
import { RouterModule, Routes } from '@angular/router';
import { MaterialModule } from '../shared/material.module';
import { TreeDynamicExampleComponent } from './tree-dynamic-example/tree-dynamic-example.component';

const routes: Routes = [
  {
    path: '', component: ProjectsComponent,
    children: [
      {path:'',redirectTo:'openrecent',pathMatch:'full'},
      { path: 'openrecent', component: OpenRecentComponent },
      { path: 'newproject', component: NewProjectComponent },
      { path: 'tree', component: TreeDynamicExampleComponent },
    ]
  }];


@NgModule({
  declarations: [
    OpenRecentComponent,
    ProjectsComponent,
    NewProjectComponent,
    CloneProjectComponent,
    TreeDynamicExampleComponent,    
  ],
  imports: [
    RouterModule.forChild(routes),
    CommonModule,
    MaterialModule,
    
  ]
})
export class ProjectsModule { }
