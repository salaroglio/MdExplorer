import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppComponent, SafePipe } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule } from '@angular/forms';
import { MaterialModule } from "./shared/material.module";
import { FlexLayoutModule } from '@angular/flex-layout';
import { Routes, RouterModule } from '@angular/router';

const routes: Routes = [
    { path: 'main', loadChildren: () => import('./main/main.module').then(m => m.MainModule) },
    {path:'**', redirectTo:'main'}
];


@NgModule({
    declarations: [
        AppComponent, SafePipe
    ],
    imports: [
        BrowserModule,
        BrowserAnimationsModule,
        RouterModule.forRoot(routes),
        FlexLayoutModule,
        MaterialModule,
        FormsModule,

    ],
    providers: [],
    bootstrap: [AppComponent]
})
export class AppModule { }
