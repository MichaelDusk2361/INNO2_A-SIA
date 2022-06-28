import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PaneContainerComponent } from './pane-container.component';
import { PaneDividerComponent } from './pane-divider/pane-divider.component';
import { PaneComponent } from './pane/pane.component';
import { PaneIfDirective } from './pane-if.directive';

/**
 * # Capabilities
 *
 * @example
 * ```html
 * <a-sia-pane-container direction="row" unit="rem" class="pane-container">
 *   <a-sia-pane
 *     [minSize]="10"
 *     class="pane-container__simulation-diagram"
 *     *aSiaPaneIf="paneService.simulationDiagramIsOpen"
 *   >
 *     <a-sia-simulation-diagram></a-sia-simulation-diagram>
 *   </a-sia-pane>
 *   <a-sia-pane [minSize]="10" *aSiaPaneIf="paneService.graphEditorIsOpen">
 *     <a-sia-graph-editor></a-sia-graph-editor>
 *   </a-sia-pane>
 *   <a-sia-pane
 *     [minSize]="40"
 *     [maxSize]="40"
 *     class="pane-container__data-panel"
 *     *aSiaPaneIf="paneService.dataPanelIsOpen"
 *   >
 *     <a-sia-data-panel></a-sia-data-panel>
 *   </a-sia-pane>
 * </a-sia-pane-container>
 * ```
 * A pane-container has the ability of working in two different direction (default row, and column), and can handle multiple units (default px, and rem, %).
 * When a unit gets assigned to the container, all contained panes work with this unit. A pane can have restrictions, like minSize, maxSize and a default size.
 * Per default, it will be sized automatically depending on the other panes sizes. Be careful, setting the total size of panes is larger or smaller than the container
 * (through setting minSizes and maxSizes too large / small), will lead to undefined behavior.
 *
 * Dividers are generated between each panes, except if the first or last pane has both minSize and maxSize (and is therefore not resizable).
 * When moving a divider, the panes to the left and right of the divider are resized accordingly. If the size of a pane gets to it's minSize / maxSize while resizing,
 * the pane will get get "pulled" or "pushed", and the pane next to it will be resized instead. This effect may propagate until the end of the container, after which it stops resizing.
 *
 * Panes can get toggled with the *aSiaPaneIf structural directive. You could also use *ngIf, but this will lead to more flickering due to lifecycle and change detection.
 *
 * It also works with touch. The direction can be bound to a variable. If the direction changes, it will automatically resize everything again, relative to [size] and restrictions.
 *
 * # Structure
 *
 * The {@link PaneContainerComponent} is the main overseeing component, handling all top level work,
 * such as resizing, initialization, different units (%, px, rem), etc.
 *
 * The {@link PaneComponent} is the only component that should be in the <a-sia-pane-container>.
 * The Pane itself has a minSize, maxSize, and size attribute, and can be toggled with the {@link PaneIfDirective}.
 *
 * The {@link PaneDividerComponent} is mainly visual, and doesn't contain much logic.
 *
 * # Functionality
 *
 * Resizing was approached by utilizing css flex. (using flex-direction, flex-basis, grow, shrink, and order.
 * [MDN for flex]{@link https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_Flexible_Box_Layout/Controlling_Ratios_of_Flex_Items_Along_the_Main_Ax})
 *
 * For further description see the individual components.
 */
@NgModule({
  declarations: [PaneContainerComponent, PaneDividerComponent, PaneComponent, PaneIfDirective],
  imports: [CommonModule],
  exports: [PaneContainerComponent, PaneComponent, PaneIfDirective]
})
export class PaneModule {}
