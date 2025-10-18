import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { HttpClient } from '@angular/common/http';

import { AppModule } from './app/app.module';
import { environment } from './environments/environment';
import { SidenavTabProviderService } from './app/services/sidenav-tab-provider.service';
import { loadPremiumModule } from './app/premium-loader';

if (environment.production) {
  enableProdMode();
}

// Mark this as the main app to prevent Premium module from bootstrapping
(window as any).__MDEXPLORER_MAIN_APP__ = true;

platformBrowserDynamic().bootstrapModule(AppModule)
  .then(async moduleRef => {
    // Expose services globally for Premium plugins
    // This enables Zero Knowledge Architecture where plugins can self-register
    const injector = moduleRef.injector;
    const tabProvider = injector.get(SidenavTabProviderService);
    const httpClient = injector.get(HttpClient);

    (window as any).__sidenavTabProvider = tabProvider;
    (window as any).__httpClient = httpClient;
    (window as any).__injector = injector;

    console.log('[MdExplorer] ✅ SidenavTabProviderService exposed globally for plugins');
    console.log('[MdExplorer] ✅ HttpClient and Injector exposed globally for plugins');

    // Eagerly load Premium module to allow tab registration
    // This happens after main app bootstrap so tab provider is available
    try {
      console.log('[MdExplorer] Attempting to load Premium module...');
      const PremiumModule = await loadPremiumModule();

      // Check if it's the stub or actual Premium
      if (PremiumModule.name !== 'AiStubModule') {
        console.log('[MdExplorer] ✅ Premium module loaded successfully');
        // Tab registration happens automatically in Premium main.ts
      } else {
        console.log('[MdExplorer] ℹ️ Running in free mode (stub loaded)');
      }
    } catch (error) {
      console.warn('[MdExplorer] Could not load Premium module:', error);
    }
  })
  .catch(err => console.error(err));
