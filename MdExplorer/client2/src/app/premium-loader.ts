/**
 * Dynamic loader for AI Premium module with automatic fallback to stub.
 *
 * This loader attempts to load the Premium Angular module from the embedded
 * assets extracted by the Premium DLL. If the module is not available (free user),
 * it falls back gracefully to the stub module.
 */

/**
 * Load AI Premium module with automatic fallback.
 *
 * Angular 11 generates webpack bundles that aren't ES6 modules,
 * so we load them as script tags and access via window object.
 *
 * @returns Promise that resolves to either AiPremiumModule or AiStubModule
 */
export async function loadPremiumModule() {
  try {
    // Angular 11 generates separate ES2015 and ES5 bundles
    // Webpack requires runtime.js to be loaded FIRST
    const runtimeFiles = [
      '/client2/premium/runtime-es2015.js',
      '/client2/premium/runtime-es5.js',
      '/client2/premium/runtime.js'
    ];

    const bundleFiles = [
      '/client2/premium/main-es2015.js',
      '/client2/premium/main-es5.js',
      '/client2/premium/main.js' // Fallback for future single bundle
    ];

    let runtimePath = null;
    let premiumPath = null;

    // Find first available runtime (check without cache buster)
    for (const file of runtimeFiles) {
      try {
        const checkResponse = await fetch(file, { method: 'HEAD' });
        if (checkResponse.ok) {
          runtimePath = file;
          console.log(`[Premium Loader] Found runtime: ${file}`);
          break;
        }
      } catch (e) {
        // Continue to next file
      }
    }

    // Find first available bundle (check without cache buster)
    for (const file of bundleFiles) {
      try {
        const checkResponse = await fetch(file, { method: 'HEAD' });
        if (checkResponse.ok) {
          premiumPath = file;
          console.log(`[Premium Loader] Found bundle: ${file}`);
          break;
        }
      } catch (e) {
        // Continue to next file
      }
    }

    if (!premiumPath) {
      throw new Error('Premium bundle not found (tried: main-es2015.js, main-es5.js, main.js)');
    }

    // Add cache buster to force reload (only for script loading, not for HEAD check)
    const cacheBuster = `?v=${Date.now()}`;

    // Load runtime first (required for webpack to work)
    if (runtimePath) {
      console.log(`[Premium Loader] Loading runtime from: ${runtimePath}`);
      await new Promise((resolve, reject) => {
        const script = document.createElement('script');
        script.src = runtimePath + cacheBuster;  // Add cache buster here
        script.async = true;
        script.onload = () => {
          console.log('[Premium Loader] Runtime loaded successfully');
          resolve(undefined);
        };
        script.onerror = (error) => {
          console.error('[Premium Loader] Runtime load error:', error);
          reject(new Error(`Failed to load runtime: ${runtimePath}`));
        };
        document.head.appendChild(script);
      });

      // Wait for webpack to initialize
      await new Promise(resolve => setTimeout(resolve, 50));
    }

    // Now load the main bundle
    console.log(`[Premium Loader] Loading Premium module from: ${premiumPath}`);

    await new Promise((resolve, reject) => {
      const script = document.createElement('script');
      script.src = premiumPath + cacheBuster;  // Add cache buster here
      script.async = true;
      script.onload = () => {
        console.log('[Premium Loader] Script loaded successfully');
        resolve(undefined);
      };
      script.onerror = (error) => {
        console.error('[Premium Loader] Script load error:', error);
        reject(new Error(`Failed to load script: ${premiumPath}`));
      };
      document.head.appendChild(script);
    });

    // Wait a bit for webpack to process the bundle
    await new Promise(resolve => setTimeout(resolve, 100));

    // Access the module from window (webpack bundle exports globally)
    const win = window as any;
    if (win.AiPremiumModule) {
      console.log('✅ AI Premium module loaded successfully');
      return win.AiPremiumModule;
    }

    throw new Error('AiPremiumModule not found on window after script load');

  } catch (error) {
    // Premium not available - load stub
    console.warn('ℹ️ AI Premium module not available, loading stub');
    console.debug('Premium load error:', error);

    // Import stub module from main app
    const stubModule = await import('./ai-stub/ai-stub.module');
    return stubModule.AiStubModule;
  }
}
