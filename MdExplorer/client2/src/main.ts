import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';
import { environment } from './environments/environment';

if (environment.production) {
  enableProdMode();
}

// Mark this as the main app to prevent Premium module from bootstrapping
(window as any).__MDEXPLORER_MAIN_APP__ = true;

platformBrowserDynamic().bootstrapModule(AppModule)
  .catch(err => console.error(err));
