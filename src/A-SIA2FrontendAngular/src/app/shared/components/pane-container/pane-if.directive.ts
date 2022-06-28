import { Directive, Host, Input, TemplateRef, ViewContainerRef } from '@angular/core';
import { PaneContainerComponent } from './pane-container.component';

/**
 * This is almost a 1:1 implementation of the *ngIf directive, except that it calls a function before and after.
 *
 * [See also: Angular Custom Structural Directives]{@link https://angular.io/guide/structural-directives}
 *
 * In this case it hides all children before hiding / showing a new element. If this isn't done, the [order] binding
 * in the {@link PaneContainerComponent} template will cause some flashing artifacts, as the order is not correct for one frame.
 * The solution was hiding everything through the parent (injected in constructor),
 * waiting a tick, and then showing everything again. (The tick is done through a setTimeout in the parent showChildrenAfterNgIf)
 * This also means that it can only be used if the parent is a {@link PaneContainerComponent}.
 *
 * This still leads to some flickering, but is easier than a diploma in angular's change detection.
 * @example
 * ```html
 * <a-sia-pane *aSiaPaneIf="BOOLEAN EXPRESSION">CONTENT</a-sia-pane>
 * ```
 */
@Directive({
  selector: '[aSiaPaneIf]'
})
export class PaneIfDirective {
  private hasView = false;

  constructor(
    @Host() private paneContainer: PaneContainerComponent,
    private templateRef: TemplateRef<unknown>,
    private viewContainer: ViewContainerRef
  ) {}

  /**
   *  The function that gets called when using *aSiaPaneIf="BOOLEAN EXPRESSION"
   */
  @Input() set aSiaPaneIf(condition: boolean) {
    this.paneContainer.hideChildrenBeforeNgIf(); // the thing that makes this different from *ngIf
    if (condition && !this.hasView) {
      this.viewContainer.createEmbeddedView(this.templateRef);
      this.hasView = true;
    } else if (!condition && this.hasView) {
      this.viewContainer.clear();
      this.hasView = false;
    }
    this.paneContainer.showChildrenAfterNgIf(); // the other thing that makes this different from *ngIf
  }
}
