import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavigatorComponent } from './components/navigator/navigator.component';
import { ToolbarComponent } from './components/toolbar/toolbar.component';
import { ContentComponent } from './components/content/content.component';
import { MainComponent } from './main.component';
import { MaterialModule } from '../shared/material.module';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [{
    path: '', component: MainComponent,
    children: [
        {path:'',component:ContentComponent}
    ]
}];



@NgModule({
  declarations: [
    NavigatorComponent,
    ToolbarComponent,
    ContentComponent,
    MainComponent
  ],
  imports: [
      CommonModule,
      MaterialModule,
      FormsModule,
      RouterModule.forChild(routes)
  ]
})
export class MainModule { }
