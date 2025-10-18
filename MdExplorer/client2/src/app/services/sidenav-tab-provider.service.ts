import { Injectable, Type, StaticProvider } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

/**
 * Configuration for a dynamically registered sidenav tab.
 * Plugins (like AI Premium) can register tabs to appear in the sidenav.
 */
export interface TabConfig {
  /** Unique identifier for the tab */
  id: string;

  /** Display label for the tab */
  label: string;

  /** Material icon name */
  icon: string;

  /** Angular component type to render in the tab */
  component: Type<any>;

  /** Optional order for sorting tabs (lower numbers appear first) */
  order?: number;

  /** Optional condition function - tab is visible only if this returns true */
  condition?: () => boolean;

  /** Optional inputs to pass to the component */
  inputs?: { [key: string]: any };

  /** Optional providers for dependency injection (e.g. plugin-specific services) */
  providers?: StaticProvider[];
}

/**
 * Service Provider Pattern for dynamic sidenav tab registration.
 *
 * This service allows plugins (like AI Premium addon) to register UI components
 * that should appear as tabs in the main sidenav, maintaining Zero Knowledge
 * Architecture where the open source core has no hardcoded knowledge of Premium features.
 *
 * Inspired by VS Code extension API.
 *
 * Example usage:
 * ```typescript
 * // In a plugin module
 * constructor(private tabProvider: SidenavTabProviderService) {
 *   this.tabProvider.registerTab({
 *     id: 'my-plugin',
 *     label: 'My Plugin',
 *     icon: 'extension',
 *     component: MyPluginComponent,
 *     order: 100
 *   });
 * }
 * ```
 */
@Injectable({
  providedIn: 'root'
})
export class SidenavTabProviderService {
  private tabsSubject = new BehaviorSubject<TabConfig[]>([]);

  /**
   * Observable stream of registered tabs.
   * Subscribe to this to reactively render tabs in the UI.
   */
  public readonly tabs$: Observable<TabConfig[]> = this.tabsSubject.asObservable();

  constructor() {
    console.log('[SidenavTabProvider] Service initialized');
  }

  /**
   * Register a new tab to appear in the sidenav.
   *
   * @param config Tab configuration
   * @throws Error if a tab with the same id already exists
   */
  registerTab(config: TabConfig): void {
    // Validate required fields
    if (!config.id || !config.label || !config.component) {
      console.error('[SidenavTabProvider] Invalid tab config:', config);
      throw new Error('Tab config must have id, label, and component');
    }

    // Check for duplicate id
    const currentTabs = this.tabsSubject.value;
    if (currentTabs.some(tab => tab.id === config.id)) {
      console.warn(`[SidenavTabProvider] Tab with id '${config.id}' already registered, skipping`);
      return;
    }

    // Add tab and sort by order
    const newTabs = [...currentTabs, config].sort((a, b) => {
      const orderA = a.order ?? 999;
      const orderB = b.order ?? 999;
      return orderA - orderB;
    });

    this.tabsSubject.next(newTabs);
    console.log(`[SidenavTabProvider] ✅ Registered tab: '${config.label}' (id: ${config.id})`);
  }

  /**
   * Unregister a tab by its id.
   *
   * @param id Tab identifier to remove
   */
  unregisterTab(id: string): void {
    const currentTabs = this.tabsSubject.value;
    const filtered = currentTabs.filter(tab => tab.id !== id);

    if (filtered.length === currentTabs.length) {
      console.warn(`[SidenavTabProvider] Tab '${id}' not found for unregistration`);
      return;
    }

    this.tabsSubject.next(filtered);
    console.log(`[SidenavTabProvider] 🔄 Unregistered tab: ${id}`);
  }

  /**
   * Get a tab configuration by id.
   *
   * @param id Tab identifier
   * @returns Tab config if found, undefined otherwise
   */
  getTabById(id: string): TabConfig | undefined {
    return this.tabsSubject.value.find(tab => tab.id === id);
  }

  /**
   * Get current snapshot of all registered tabs.
   *
   * @returns Array of tab configurations
   */
  getTabs(): TabConfig[] {
    return [...this.tabsSubject.value];
  }

  /**
   * Clear all registered tabs.
   * Useful for testing or reset scenarios.
   */
  clearAll(): void {
    this.tabsSubject.next([]);
    console.log('[SidenavTabProvider] 🗑️  All tabs cleared');
  }
}
