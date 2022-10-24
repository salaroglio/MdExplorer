import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { Routes, RouterModule } from '@angular/router';
import { FlexLayoutModule } from '@angular/flex-layout';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';


//import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { MaterialModule } from "./shared/material.module";
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ParsingProjectProvider } from './signalR/dialogs/parsing-project/parsing-project.provider';
import { ConnectionLostProvider } from './signalR/dialogs/connection-lost/connection-lost.provider';
import { PlantumlWorkingComponent } from './signalR/dialogs/plantuml-working/plantuml-working.component';
import { PlantumlWorkingProvider } from './signalR/dialogs/plantuml-working/plantuml-working.provider';

const routes: Routes = [
  { path: 'main', loadChildren: () => import('./md-explorer/md-explorer.module').then(m => m.MdExplorerModule) },
  { path: 'projects', loadChildren: () => import('./projects/projects.module').then(m => m.ProjectsModule) },
  { path: '**', redirectTo: 'projects' }
];

@NgModule({
  declarations: [
    AppComponent,
    PlantumlWorkingComponent,
    
  ],
  imports: [
    BrowserModule,
    RouterModule.forRoot(routes ), //
    FlexLayoutModule,
    MaterialModule,
    FormsModule,
    BrowserAnimationsModule,
    HttpClientModule,    
    
  ],
  providers: [ParsingProjectProvider,
               ConnectionLostProvider, PlantumlWorkingProvider],
  bootstrap: [AppComponent]
})
export class AppModule {
  constructor() {
    console.log('AppModuleConstructor');
  }
}
