import { Directive, ViewContainerRef } from '@angular/core';

/**
 * Directive that provides a placeholder for dynamic component injection.
 *
 * Used in sidenav tabs to dynamically load plugin components at runtime.
 * Compatible with Angular 11 (uses ComponentFactoryResolver instead of newer APIs).
 *
 * Usage:
 * ```html
 * <ng-template appDynamicTabHost></ng-template>
 * ```
 */
@Directive({
  selector: '[appDynamicTabHost]'
})
export class DynamicTabHostDirective {
  constructor(public viewContainerRef: ViewContainerRef) { }
}
