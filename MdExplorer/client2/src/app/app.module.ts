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
import { ShowFileSystemComponent } from './commons/components/show-file-system/show-file-system.component';
import { WaitingDialogComponent } from './commons/waitingdialog/waiting-dialog/waiting-dialog.component';
import { NewDirectoryComponent } from './commons/components/new-directory/new-directory.component';
import { OpeningApplicationComponent } from './signalR/dialogs/opening-application/opening-application.component';
import { OpeningApplicationProvider } from './signalR/dialogs/opening-application/opening-application.provider';


const routes: Routes = [
  {
    path: 'main',
    loadChildren: () => import('./md-explorer/md-explorer.module').then(m => m.MdExplorerModule),
    data: { animation: 'main' }
  },
  {
    path: 'projects', loadChildren: () => import('./projects/projects.module').then(m => m.ProjectsModule),
    data: { animation: 'projects' }
  },
  { path: '**', redirectTo: 'projects', data: { animation: 'projects' } }
];

@NgModule({
  declarations: [
    AppComponent,
    PlantumlWorkingComponent,
    ShowFileSystemComponent,
    WaitingDialogComponent,
    NewDirectoryComponent,
    OpeningApplicationComponent
  ],
  imports: [
    BrowserModule,
    RouterModule.forRoot(routes), //
    FlexLayoutModule,
    MaterialModule,
    FormsModule,
    BrowserAnimationsModule,
    HttpClientModule,    
    
  ],
  providers: [ParsingProjectProvider,
    ConnectionLostProvider,
    PlantumlWorkingProvider,
    OpeningApplicationProvider],
  bootstrap: [AppComponent]
})
export class AppModule {
  constructor() {
    console.log('AppModuleConstructor');
  }
}
