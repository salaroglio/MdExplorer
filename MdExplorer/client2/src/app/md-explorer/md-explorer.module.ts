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
import { SafePipe } from './pipes/safePipe';
import { SettingsComponent } from './components/settings/settings.component';
import { RenameFileComponent } from './components/refactoring/rename-file/rename-file.component';
import { RulesComponent } from './components/rules/rules.component';
import { NewMarkdownComponent } from './components/new-markdown/new-markdown.component';
import { NewDirectoryComponent } from './components/new-directory/new-directory.component';
import { MdTreeComponent } from './components/md-tree/md-tree.component';
import { GitChangesComponent } from './components/git-changes/git-changes.component';
import { SelectExtendedComponent } from './components/git-changes/select-extended/select-extended.component';
import { OptionExtendedComponent } from './components/git-changes/select-extended/option-extended/option-extended.component';
import { SelectBranchComponent } from './components/git-changes/select-branch/select-branch.component';
import { OptionBranchComponent } from './components/git-changes/select-branch/option-branch/option-branch.component';

 



const routes: Routes = [
  { path: '', component: SidenavComponent},
  {
  path: 'navigation', component: SidenavComponent,
  children: [
    { path: ':path', component: MainContentComponent },
    //{ path: '', component: MainContentComponent },    
  ]
  }];


@NgModule({
  declarations: [
    SidenavComponent,
    
    ToolbarComponent,
    SafePipe,
    MainContentComponent,
    MdExplorerComponent,
    SettingsComponent,
    RenameFileComponent,
    RulesComponent,
    NewMarkdownComponent,
    NewDirectoryComponent,
    MdTreeComponent,
    GitChangesComponent,
    SelectExtendedComponent,
    OptionExtendedComponent,
    SelectBranchComponent,
    OptionBranchComponent,
    
      
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
export class MdExplorerModule {
  constructor() {
    console.log('constructor MdExplorerModule');
  }
}
