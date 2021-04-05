import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { ChartsModule } from 'ng2-charts';
import { HttpClientModule } from '@angular/common/http';

import { AppComponent,SafePipe  } from './app.component';

@NgModule({
  declarations: [
    AppComponent,SafePipe
  ],
  imports: [
    BrowserModule,
    ChartsModule,
    HttpClientModule
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
