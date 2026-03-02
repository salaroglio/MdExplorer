import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { Routes, RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { ConnectionIdInterceptor } from './interceptors/connection-id.interceptor';


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
import { TitleBarComponent } from './components/title-bar/title-bar.component';
import { SearchBoxComponent } from './components/search-box/search-box.component';
import { CompatibilityModeBadgeComponent } from './md-explorer/components/compatibility-mode-badge/compatibility-mode-badge.component';
import { UnifiedSettingsDialogComponent } from './components/unified-settings-dialog/unified-settings-dialog.component';
import { AppStoreSettingsDialogComponent } from './components/app-store-settings-dialog/app-store-settings-dialog.component';


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
    OpeningApplicationComponent,
    TitleBarComponent,
    SearchBoxComponent,
    CompatibilityModeBadgeComponent,
    UnifiedSettingsDialogComponent,
    AppStoreSettingsDialogComponent
  ],
  imports: [
    BrowserModule,
    RouterModule.forRoot(routes),
    MaterialModule,
    FormsModule,
    ReactiveFormsModule,
    BrowserAnimationsModule,
    HttpClientModule,    
    
  ],
  providers: [
    ParsingProjectProvider,
    ConnectionLostProvider,
    PlantumlWorkingProvider,
    OpeningApplicationProvider,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: ConnectionIdInterceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule {
  constructor() {
    console.log('AppModuleConstructor');
  }
}
