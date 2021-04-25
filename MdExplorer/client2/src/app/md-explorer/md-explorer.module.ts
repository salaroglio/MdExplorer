import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SidenavComponent } from './components/sidenav/sidenav.component';
import { ToolbarComponent } from './components/toolbar/toolbar.component';
import { MainContentComponent } from './components/main-content/main-content.component';
import { MaterialModule } from '../shared/material.module';
import { FormsModule } from '@angular/forms';
import { MdExplorerComponent } from './md-explorer.component';
import { Routes, RouterModule } from '@angular/router';
import { MdFileService } from './services/md-file.service';
import { HttpClientModule } from '@angular/common/http';


const routes: Routes = [{
  path: '', component: MdExplorerComponent,
  children: [
    { path: ':path', component: MainContentComponent },
    { path: '', component: MainContentComponent }
  ]
}];


@NgModule({
  declarations: [
    SidenavComponent,
    ToolbarComponent,
    
    MainContentComponent,
    MdExplorerComponent    
  ],
  imports: [
    CommonModule,
    MaterialModule,
    HttpClientModule,
    FormsModule,
    RouterModule.forChild(routes)
  ],
  providers: [
    MdFileService
  ]
})
export class MdExplorerModule { }
