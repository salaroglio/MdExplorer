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
 * @returns Promise that resolves to either AiPremiumModule or AiStubModule
 */
export async function loadPremiumModule() {
  try {
    // Attempt to check if Premium is available first
    // This avoids TypeScript/Webpack trying to resolve the path at compile time
    const premiumCheckResponse = await fetch('/client2/premium/main.js', { method: 'HEAD' });

    if (!premiumCheckResponse.ok) {
      throw new Error('Premium module not available');
    }

    // Premium is available - load it dynamically using runtime path construction
    // This prevents Webpack from trying to resolve the import at compile time
    const premiumPath = '/client2/premium/main.js';

    // Use Function constructor to create truly dynamic import
    const loadModule = new Function('path', 'return import(path)');
    const premiumModule = await loadModule(premiumPath);

    if (!premiumModule.AiPremiumModule) {
      throw new Error('AiPremiumModule not found in premium bundle');
    }

    console.log('✅ AI Premium module loaded successfully');
    return premiumModule.AiPremiumModule;

  } catch (error) {
    // Premium not available - load stub
    console.warn('ℹ️ AI Premium module not available, loading stub');
    console.debug('Premium load error:', error);

    // Import stub module from main app
    const stubModule = await import('./ai-stub/ai-stub.module');
    return stubModule.AiStubModule;
  }
}
