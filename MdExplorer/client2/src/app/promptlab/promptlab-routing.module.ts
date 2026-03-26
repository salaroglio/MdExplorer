import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PromptLabComponent } from './components/promptlab/promptlab.component';

const routes: Routes = [
  { path: '', component: PromptLabComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PromptLabRoutingModule { }
