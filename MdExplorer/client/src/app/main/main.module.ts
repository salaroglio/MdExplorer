import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavigatorComponent } from './components/navigator/navigator.component';
import { ToolbarComponent } from './components/toolbar/toolbar.component';
import { ContentComponent } from './components/content/content.component';



@NgModule({
  declarations: [
    NavigatorComponent,
    ToolbarComponent,
    ContentComponent
  ],
  imports: [
    CommonModule
  ]
})
export class MainModule { }
