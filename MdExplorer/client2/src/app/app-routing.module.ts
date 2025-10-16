import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { loadPremiumModule } from './premium-loader';

const routes: Routes = [
  {
    path: 'ai-premium',
    loadChildren: () => loadPremiumModule()
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
