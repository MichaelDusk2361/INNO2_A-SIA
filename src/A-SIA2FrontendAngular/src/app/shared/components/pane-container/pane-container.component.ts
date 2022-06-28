import {
  Component,
  ContentChildren,
  ElementRef,
  HostBinding,
  Input,
  OnChanges,
  OnInit,
  QueryList,
  SimpleChanges,
  ViewChildren,
  ViewEncapsulation
} from '@angular/core';
import { PaneComponent } from './pane/pane.component';
import { InputService } from '@shared/components/user-input/input.service';
import { PaneDividerComponent } from './pane-divider/pane-divider.component';
import { PaneContainerError } from './PaneErrors';
import { PaneUnit } from './PaneUnit';

/**
 * Handles sizing and initialization of children panes.
 *
 * Relevant information for the template: Dividers get initialized with the order depending
 * on the count of displayed panes. The horrendous ngIf is there, so that the last divider is not shown
 * (it would be right of / below the last pane). It also hides a divider if the last or first pane is not
 * resizable (maxSize == minSize).
 *
 * See also: {@link https://angular.io/api/common/NgForOf}.
 *
 * @example
 * ```html
 * <a-sia-pane-container direction="row" unit="rem" class="pane-container">
 *  <a-sia-pane
 *    [minSize]="10"
 *    class="pane-container__simulation-diagram"
 *    *aSiaPaneIf="paneService.simulationDiagramIsOpen"
 *  >
 *    <a-sia-simulation-diagram></a-sia-simulation-diagram>
 *  </a-sia-pane>
 *  <a-sia-pane [minSize]="10" *aSiaPaneIf="paneService.graphEditorIsOpen">
 *    <a-sia-graph-editor></a-sia-graph-editor>
 *  </a-sia-pane>
 *  <a-sia-pane
 *    [minSize]="40"
 *    [maxSize]="40"
 *    class="pane-container__data-panel"
 *    *aSiaPaneIf="paneService.dataPanelIsOpen"
 *  >
 *    <a-sia-data-panel></a-sia-data-panel>
 *  </a-sia-pane>
 *</a-sia-pane-container>
 *```
 */
@Component({
  selector: 'a-sia-pane-container',
  templateUrl: './pane-container.component.html',
  styleUrls: ['./pane-container.component.scss'],
  encapsulation: ViewEncapsulation.Emulated
})
export class PaneContainerComponent implements OnInit, OnChanges {
  @Input() @HostBinding('style.flexDirection') direction: 'row' | 'column' = 'row';

  @Input() unit: PaneUnit = 'px';
  /**
   * This is always in px.
   */
  private _boundingClientRect!: DOMRect;
  /**
   * Gets boundingClientRect with the correct unit in mind.
   * (Converts px to rem, and sets width & height to 100 for %).
   */
  public get boundingClientRect(): DOMRect {
    if (this.unit == 'rem')
      return new DOMRect(
        this.getRemFromPx(this._boundingClientRect.x),
        this.getRemFromPx(this._boundingClientRect.y),
        this.getRemFromPx(this._boundingClientRect.width),
        this.getRemFromPx(this._boundingClientRect.height)
      );
    if (this.unit == '%') return new DOMRect(this._boundingClientRect.x, this._boundingClientRect.y, 100, 100);
    if (this.unit == 'px') return this._boundingClientRect;
    throw new PaneContainerError(`unknown unit ${this.unit} in DomRect getter`);
  }

  /**
   * Subscribes to global move, up and resize
   * @param element {ElementRef} For getting BoundingClientRect
   * @param inputService {InputService} For reacting to pointermove and window resize events
   */
  constructor(private element: ElementRef, private inputService: InputService) {
    this.inputService.onResize$.subscribe(() => {
      this.onResizeWindow();
    });
    this.inputService.onPointerMove$.subscribe((e) => {
      if (this._clickedPaneDivider == null) return;
      this.moveDivider(e);
    });
    this.inputService.onPointerUp$.subscribe(() => {
      this._clickedPaneDivider = null;
    });
  }

  /**
   *
   * @param changes when direction changes, reset to default sizing proportions
   */
  ngOnChanges(changes: SimpleChanges): void {
    if (!changes['direction'].firstChange) {
      this.initializePaneSizes();
    }
  }

  ngOnInit(): void {
    this.updateBoundingClientRect();
  }

  /**
   * Should be called sparingly, and only when necessary (i.e. when the bounds change, and not onMouseMove).
   */
  updateBoundingClientRect(): void {
    this._boundingClientRect = this.element.nativeElement.getBoundingClientRect(); // expensive
    // boundingClientRect is relative to the window, and may have to be offset by the scroll position.
    this._boundingClientRect.x += window.scrollX;
    this._boundingClientRect.y += window.scrollY;
  }

  /**
   * @returns height or width of boundingClientRect, depending on direction
   */
  getRelevantBoundingSize(): number {
    return this.direction == 'row' ? this.boundingClientRect.width : this.boundingClientRect.height;
  }

  /**
   * gets buffered at start as getComputedStyle() is expensive
   */
  rootFontSize = parseFloat(getComputedStyle(document.documentElement).fontSize.replace('px', ''));
  /**
   * @param px {number} The px that should get converted to Rem
   * @returns px / root font-size
   */
  getRemFromPx(px: number): number {
    return px / this.rootFontSize;
  }

  _displayedPanes!: QueryList<PaneComponent>;
  /**
   * Gets called every time when ng-content changes. So it will also be called, if a pane is toggled with *aSiaPaneIf.
   */
  @ContentChildren(PaneComponent, {}) set displayedPanes(v: QueryList<PaneComponent>) {
    this._displayedPanes = v;
    /// setTimeout is used to delay initialization for one tick, so that I can avoid spending 30h with angular's manual change detection.
    /// (child of parent changes after child's was initialized, and change detection had already ran => [NG0100]{@link https://angular.io/errors/NG0100})
    setTimeout(() => {
      this.initializePanes();
      this.initializePaneSizes();
    });
  }

  /**
   * Sets correct order, unit and display.
   * Divider order gets initialized in template.
   */
  initializePanes(): void {
    this._displayedPanes.forEach((pane, index) => {
      pane.display = 'block';
      pane.order = 2 * index;
      pane.unit = this.unit;
    });
  }

  /**
   * Sets correct sizes. It tries to fulfill the pane's [size], but may not always be able to if the [size] is too small.
   */
  initializePaneSizes(): void {
    const panesToAutomaticallySize: PaneComponent[] = [];
    let distributedSize = 0;

    // The first pass of sizing:
    // Sets sizes of all custom [size] set panes
    this._displayedPanes.forEach((pane) => {
      if (pane.initialSize === null) {
        panesToAutomaticallySize.push(pane);
      } else distributedSize += pane.initialSize;
    });

    // distributes everything else evenly
    panesToAutomaticallySize.forEach((pane) => {
      pane.size =
        ((this.direction == 'row' ? this.boundingClientRect.width : this.boundingClientRect.height) - distributedSize) /
        panesToAutomaticallySize.length;
    });

    // The second pass of sizing:
    distributedSize = 0;
    const panesToResizeAfterFixingMinMaxViolations: PaneComponent[] = [];
    // clamp all panes that are violating the sizing restrictions, and store everything else
    this._displayedPanes.forEach((pane) => {
      if (pane.size < pane.minSize) {
        // pane to small
        pane.size = pane.maxSize;
        distributedSize += pane.size;
      } else if (pane.size > pane.maxSize) {
        // pane to big
        pane.size = pane.maxSize;
        distributedSize += pane.size;
      } else if (pane.initialSize !== null) {
        // pane has a custom size set
        distributedSize += pane.size;
      } else {
        panesToResizeAfterFixingMinMaxViolations.push(pane); // everything else gets dumped here
      }
    });

    /// Distribute all remaining sizes. This might cause a pane that was previously not violating a minMax rule,
    /// to actually pop over their restriction, but you would have to try very hard to produce that edge case.
    /// Also may be the case in onResizeWindow().
    panesToResizeAfterFixingMinMaxViolations.forEach((pane) => {
      pane.size =
        ((this.direction == 'row' ? this.boundingClientRect.width : this.boundingClientRect.height) - distributedSize) /
        panesToResizeAfterFixingMinMaxViolations.length;
    });

    this._displayedPanes.forEach((pane) => {
      pane.refreshFlexStyle();
    });

    // notify all panes that their sizes were initialized, after a timeout so that the html/css has time to catch up.
    setTimeout(() => {
      this._displayedPanes.forEach((pane) => {
        pane.onInit$.emit(pane);
      });
    });
  }

  /**
   * Get all divider for temporarily hiding them whenever a pane is toggled with *aSiaPaneIf
   */
  @ViewChildren('divider') displayedDivider!: QueryList<PaneDividerComponent>;

  /**
   * Hides panes and dividers before triggering PaneIfDirective, acts as a "debounce",
   * to reduce most flashing due to flex order that takes a tick to be corrected.
   */
  public hideChildrenBeforeNgIf(): void {
    this._displayedPanes?.forEach((pane) => {
      pane.display = 'none';
    });
    this.displayedDivider?.forEach((divider) => {
      divider.hide();
    });
  }
  /**
   * Shows panes and dividers after triggering PaneIfDirective, acts as a "debounce",
   * to reduce most flashing due to flex order that takes a tick to be corrected.
   */
  public showChildrenAfterNgIf(): void {
    setTimeout(() => {
      this._displayedPanes?.forEach((pane) => {
        pane.display = 'block';
      });
      this.displayedDivider?.forEach((divider) => {
        divider.show();
      });
    });
  }

  /**
   * Whenever the window gets resized, the container does its best to keep the panes proportions.
   * This means checking if the window has been shrunk or expanded, and resizing panels accordingly,
   * while paying attention to all min/max sizes.
   */
  onResizeWindow(): void {
    if (this.unit == '%') return; // % doesn't care about container size.
    const containerSizeBeforeResize = this.getRelevantBoundingSize(); // size before window resize
    this.updateBoundingClientRect();
    const containerSizeAfterResize = this.getRelevantBoundingSize(); // size after resize
    // factor == 1: no relevant change, factor < 1: window is smaller, factor > 1: window is larger
    const resizeFactor = containerSizeAfterResize / containerSizeBeforeResize;
    if (resizeFactor == 1) return; // no relevant change, no resize necessary (i.e. if height changes, but the container is in row mode)
    let distributedSize = 0; // Will be needed for knowing the remaining budget
    const panesToAutomaticallySizeWithoutMaxMinPanes: PaneComponent[] = [];

    // First sizing pass for possibly violation of min/max sizes
    this._displayedPanes.forEach((pane) => {
      const newPaneSize = pane.size * resizeFactor; // e.g. 400px pane, with 0.5 resize factor becomes 200px pane
      if (resizeFactor < 1) {
        // window got smaller => only minSizes have to be checked
        if (newPaneSize < pane.minSize) {
          // if a new Size would violate minSize, it will clamp to its minSite
          pane.size = pane.minSize;
          distributedSize += pane.minSize;
        } else {
          panesToAutomaticallySizeWithoutMaxMinPanes.push(pane); // store for automatic sizing
        }
      } else if (resizeFactor > 1) {
        // window got larger => only maxSizes have to be checked
        if (newPaneSize > pane.maxSize) {
          // if a new Size would violate maxSize, it will clamp to its maxSite
          pane.size = pane.maxSize;
          distributedSize += pane.maxSize;
        } else {
          panesToAutomaticallySizeWithoutMaxMinPanes.push(pane); // store for automatic sizing
        }
      }
    });

    let totalSizeOfNonMaxMinPanes = 0;
    const paneSizesRelativeToContainer: number[] = []; // container for proportions of remaining panes to their total sum
    /// get the total size of the remaining un-resized panes
    /// (can probably be calculated with boundingSize - distributedSize, but it works and I won't touch it.)
    panesToAutomaticallySizeWithoutMaxMinPanes.forEach((pane) => {
      totalSizeOfNonMaxMinPanes += pane.size;
    });
    // populate the container with proportions relative to the total remaining size
    panesToAutomaticallySizeWithoutMaxMinPanes.forEach((pane) => {
      // avoids division by 0 in else
      if (totalSizeOfNonMaxMinPanes == 0) paneSizesRelativeToContainer.push(0);
      else paneSizesRelativeToContainer.push(pane.size / totalSizeOfNonMaxMinPanes);
    });

    // distribute the panes that didn't violate their min/max sizes
    panesToAutomaticallySizeWithoutMaxMinPanes.forEach((pane, index) => {
      pane.size = (containerSizeAfterResize - distributedSize) * paneSizesRelativeToContainer[index];
    });

    // push changes to css
    this._displayedPanes.forEach((pane) => {
      pane.refreshFlexStyle();
    });
  }

  private _panesBeforeClickedPaneDivider: PaneComponent[] = [];
  private _panesAfterClickedPaneDivider: PaneComponent[] = [];

  private _clickedPaneDivider!: PaneDividerComponent | null;
  public get clickedPaneDivider(): PaneDividerComponent | null {
    return this._clickedPaneDivider;
  }
  /**
   * Gets set by a divider that has the parent PaneContainerComponent injected, and is clicked.
   */
  public set clickedPaneDivider(value: PaneDividerComponent | null) {
    this._clickedPaneDivider = value;
    if (this._clickedPaneDivider == null) return; // will probably never happen

    this.updateBoundingClientRect(); // to be safe, will probably not always be necessary.
    // Reset arrays, and fill them with panes before and after the divider. This can be found out by looking at the order.
    this._panesBeforeClickedPaneDivider = [];
    this._panesAfterClickedPaneDivider = [];
    this._displayedPanes.forEach((pane) => {
      if (pane.order < (this._clickedPaneDivider as PaneDividerComponent).order)
        this._panesBeforeClickedPaneDivider.push(pane);
      else this._panesAfterClickedPaneDivider.push(pane);
    });
  }

  /**
   * Tries to resize the panel left and right of the divider, in order to "move" the divider.
   * The divider itself doesn't directly have a position, it's just set between two correctly resizing panes.
   * If the pane before or after the divider isn't able to be resized (due to minMax size) it skips it,
   * and tries to resize the pane next to it, and so on.
   * @param event event containing new divider position (i.e. mouse position)
   */
  moveDivider(event: PointerEvent): void {
    const directionallyRelevantMousePos = this.getDirectionalRelevantMousePosition(event, this.unit, this.direction);

    let panelsWereResized = false; // bool for breaking out of while, and knowing when to apply changes afterwards
    let beforeIndexToApplyResize = this._panesBeforeClickedPaneDivider.length - 1; // index of the pane directly before the divider
    let afterIndexToApplyResize = 0; // index of the pane directly after the divider
    let newSize = { beforePane: 0, afterPane: 0 }; // new sizes
    let beforePaneToResize!: PaneComponent;
    let afterPaneToResize!: PaneComponent;

    /// Try to resize the panel left and right of the divider. If it can't be resized, skip it, and try to resize the next one.
    /// Repeat this until either all panes before / after the divider can't be resized, or they were successfully resized.
    do {
      beforePaneToResize = this._panesBeforeClickedPaneDivider[beforeIndexToApplyResize];
      // magic math
      newSize.beforePane =
        directionallyRelevantMousePos -
        this.getSizeUntilBeginningOfPane(beforeIndexToApplyResize) -
        this.getSizeOfSkippedBeforePanes(beforeIndexToApplyResize);

      afterPaneToResize = this._panesAfterClickedPaneDivider[afterIndexToApplyResize];
      // more magic math
      newSize.afterPane =
        this.getSizeUntilBeginningOfPane(this._panesBeforeClickedPaneDivider.length + afterIndexToApplyResize) +
        afterPaneToResize.size -
        directionallyRelevantMousePos -
        this.getSizeOfSkippedAfterPanes(afterIndexToApplyResize);

      newSize = this.getClampedSizesIfPaneNotAtMaxOrMin(beforePaneToResize, afterPaneToResize, newSize);

      // check if the pane before the divider violates its minMax sizes, if yes, try the next one
      if (beforePaneToResize.maxSize < newSize.beforePane || beforePaneToResize.minSize > newSize.beforePane) {
        beforeIndexToApplyResize--;
        continue;
      }
      // check if the pane after the divider violates its minMax sizes, if yes, try the next one
      if (afterPaneToResize.maxSize < newSize.afterPane || afterPaneToResize.minSize > newSize.afterPane) {
        afterIndexToApplyResize++;
        continue;
      }
      // if both resizes were within boundaries, leave.
      panelsWereResized = true;
    } while (
      !panelsWereResized &&
      beforeIndexToApplyResize >= 0 &&
      afterIndexToApplyResize < this._panesAfterClickedPaneDivider.length
    );

    // only apply changes if the panels could be resized
    if (panelsWereResized) {
      this._panesBeforeClickedPaneDivider[beforeIndexToApplyResize].size = newSize.beforePane as number;
      this._panesAfterClickedPaneDivider[afterIndexToApplyResize].size = newSize.afterPane as number;
      this._panesBeforeClickedPaneDivider[beforeIndexToApplyResize].refreshFlexStyle();
      this._panesAfterClickedPaneDivider[afterIndexToApplyResize].refreshFlexStyle();
    }
  }

  /**
   *
   * @param event event containing mouse position
   * @param unit unit that the mouse position should be in
   * @param direction direction the container is in
   * @returns a mouse position in the correct unit, and relative to the correct side (left or right, depending on row or column)
   */
  getDirectionalRelevantMousePosition(event: PointerEvent, unit: PaneUnit, direction: 'row' | 'column'): number {
    let mousePos!: { x: number; y: number };

    switch (unit) {
      case 'px':
        mousePos = {
          x: event.x - this._boundingClientRect.left,
          y: event.y - this._boundingClientRect.top
        };
        break;
      case '%':
        mousePos = {
          x: ((event.x - this._boundingClientRect.left) / this._boundingClientRect.width) * 100,
          y: ((event.y - this._boundingClientRect.top) / this._boundingClientRect.height) * 100
        };
        break;
      case 'rem':
        mousePos = {
          x: this.getRemFromPx(event.x - this._boundingClientRect.left),
          y: this.getRemFromPx(event.y - this._boundingClientRect.top)
        };
        break;
      default:
        throw new PaneContainerError(`unknown unit ${unit} in getDirectionalRelevantMousePosition`);
    }
    return direction == 'row' ? mousePos.x : mousePos.y;
  }

  /**
   * @returns the total size of all panes before the divider
   */
  getSizeBeforeDivider(): number {
    let sizeBeforeDivider = 0;
    this._panesBeforeClickedPaneDivider.forEach((pane) => {
      sizeBeforeDivider += pane.size;
    });
    return sizeBeforeDivider;
  }

  /**
   * @param paneIndex index of pane until which to get the size
   * @returns total size until beginning of pane with the index paneIndex
   */
  getSizeUntilBeginningOfPane(paneIndex: number): number {
    if (paneIndex < 0 || paneIndex >= this._displayedPanes.length)
      throw new PaneContainerError(`Cannot get Pane at index ${paneIndex}`);
    let sizeUntilBeginningOfPane = 0;
    this._displayedPanes.forEach((pane, index) => {
      if (index < paneIndex) {
        sizeUntilBeginningOfPane += pane.size;
      }
    });
    return sizeUntilBeginningOfPane;
  }

  /**
   * Gets the size of all panes before the divider that were skipped due to minMax restraints.
   * @param beforeIndexToApplyResize index of a pane before the divider
   * @returns the sizes of all panes that are between the beforeIndexToApplyResize pane and the divider
   */
  getSizeOfSkippedBeforePanes(beforeIndexToApplyResize: number): number {
    if (beforeIndexToApplyResize < 0 || beforeIndexToApplyResize >= this._panesBeforeClickedPaneDivider.length)
      throw new PaneContainerError(`Cannot get BeforePane at index ${beforeIndexToApplyResize}`);

    let sizeOfSkippedBeforePanes = 0;
    this._panesBeforeClickedPaneDivider.forEach((pane, index) => {
      if (index > beforeIndexToApplyResize) {
        sizeOfSkippedBeforePanes += pane.size;
      }
    });
    return sizeOfSkippedBeforePanes;
  }
  /**
   * Gets the size of all panes after the divider that were skipped due to minMax restraints.
   * @param afterIndexToApplyResize index of a pane before the divider
   * @returns the sizes of all panes that are between the afterIndexToApplyResize pane and the divider
   */
  getSizeOfSkippedAfterPanes(afterIndexToApplyResize: number): number {
    if (afterIndexToApplyResize < 0 || afterIndexToApplyResize >= this._panesAfterClickedPaneDivider.length)
      throw new PaneContainerError(`Cannot get AfterPane at index ${afterIndexToApplyResize}`);
    let sizeOfSkippedAfterPanes = 0;
    this._panesAfterClickedPaneDivider.forEach((pane, index) => {
      if (index < afterIndexToApplyResize) {
        sizeOfSkippedAfterPanes += pane.size;
      }
    });
    return sizeOfSkippedAfterPanes;
  }

  /**
   *
   * @param beforePaneToResize beforePane to eventually clamp
   * @param afterPaneToResize afterPane to eventually clamp
   * @param newSize Sizes that the before and after should get
   * @returns Eventually clamped sizes. Possibly the same as newSize if no minMax violations occurred.
   */
  getClampedSizesIfPaneNotAtMaxOrMin(
    beforePaneToResize: PaneComponent,
    afterPaneToResize: PaneComponent,
    newSize: { beforePane: number; afterPane: number }
  ): { beforePane: number; afterPane: number } {
    const clampedSize = newSize;
    // check if beforePane violates maxSize, clamp if yes
    if (beforePaneToResize.maxSize < clampedSize.beforePane && beforePaneToResize.maxSize > beforePaneToResize.size) {
      clampedSize.afterPane += clampedSize.beforePane - beforePaneToResize.maxSize;
      clampedSize.beforePane = beforePaneToResize.maxSize;
    } // check if beforePane violates minSize, clamp if yes
    else if (
      beforePaneToResize.minSize > clampedSize.beforePane &&
      beforePaneToResize.minSize < beforePaneToResize.size
    ) {
      clampedSize.afterPane += clampedSize.beforePane - beforePaneToResize.minSize;
      clampedSize.beforePane = beforePaneToResize.minSize;
    }

    // check if afterPane violates maxSize, clamp if yes
    if (afterPaneToResize.maxSize < clampedSize.afterPane && afterPaneToResize.maxSize > afterPaneToResize.size) {
      clampedSize.beforePane += clampedSize.afterPane - afterPaneToResize.maxSize;
      clampedSize.afterPane = afterPaneToResize.maxSize;
    } // check if afterPane violates minSize, clamp if yes
    else if (afterPaneToResize.minSize > clampedSize.afterPane && afterPaneToResize.minSize < afterPaneToResize.size) {
      clampedSize.beforePane += clampedSize.afterPane - afterPaneToResize.minSize;
      clampedSize.afterPane = afterPaneToResize.minSize;
    }
    return clampedSize;
  }
}
