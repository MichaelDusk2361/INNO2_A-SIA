import { Component, EventEmitter, HostBinding, Input, OnInit, Output } from '@angular/core';
import { PaneError } from '../PaneErrors';
import { PaneUnit } from '../PaneUnit';

/**
 * Forwards all to <ng-content>. Contains all the necessary HostBindings for enabling resizing.
 *
 * Some info about the properties:
 * - flexGrow and flexShrink: For letting flex automatically size incase all size are somehow not adding up correctly.
 * - order: Because of how it and the dividers are initialized. See also: {@link PaneContainerComponent}
 * - flexBasis: To actually size them all. Has the advantage of being only one property for width and height,
 * depending on the flex-direction (as supposed to absolute height and width values).
 * - size and initialSize: the initialSize is needed for initialization purposes seen in the {@link PaneContainerComponent}.
 * 
 * @example

 * ```html
 * Resizable with at least 40 units.
 * <a-sia-pane
 *   [minSize]="40"
 *   class="pane-container__data-panel"
 *   *aSiaPaneIf="paneService.dataPanelIsOpen"
 * >
 *   <a-sia-data-panel></a-sia-data-panel>
 * </a-sia-pane>
 * ```
 * Resizable up to 40 units.
 * ```html
 * <a-sia-pane
 *   [maxSize]="40"
 *   class="pane-container__data-panel"
 *   *aSiaPaneIf="paneService.dataPanelIsOpen"
 * >
 *   <a-sia-data-panel></a-sia-data-panel>
 * </a-sia-pane>
 * ```
 * Resizable between 20 to 40 units.
 * ```html
 * <a-sia-pane
 *   [minSize]="20"
 *   [maxSize]="40"
 *   class="pane-container__data-panel"
 *   *aSiaPaneIf="paneService.dataPanelIsOpen"
 * >
 *   <a-sia-data-panel></a-sia-data-panel>
 * </a-sia-pane>
 * ```
 * Resizable up to 40 units, with 20 units initial size (if possible):
 * ```html
 * <a-sia-pane
 *   [size]="20"
 *   [maxSize]="40"
 *   class="pane-container__data-panel"
 *   *aSiaPaneIf="paneService.dataPanelIsOpen"
 * >
 *   <a-sia-data-panel></a-sia-data-panel>
 * </a-sia-pane>
 * ```
 * This won't be resizable:
 * ```html
 * <a-sia-pane
 *   [minSize]="40"
 *   [maxSize]="40"
 *   class="pane-container__data-panel"
 *   *aSiaPaneIf="paneService.dataPanelIsOpen"
 * >
 *   <a-sia-data-panel></a-sia-data-panel>
 * </a-sia-pane>
 * ```
 * For edge cases see {@link PaneModule}.
 */
@Component({
  selector: 'a-sia-pane',
  templateUrl: './pane.component.html',
  styleUrls: ['./pane.component.scss']
})
export class PaneComponent implements OnInit {
  @HostBinding('style.display') _display = 'none';
  public set display(value: 'none' | 'block') {
    this._display = value;
  }

  @HostBinding('style.order') _flexOrder = 0;
  @HostBinding('style.flexBasis') _flexBasis = '0';
  @HostBinding('style.flexGrow') _flexGrow = 1;
  @HostBinding('style.flexShrink') _flexShrink = 1;

  /**
   * This is used for being able to give the pane an initial size. This was separated from this.size,
   * to allow for some flexibility in automatically sizing in {@link PaneContainerComponent}.
   */
  private _initialSize: number | null = null;
  @Input('size')
  public get initialSize(): number | null {
    return this._initialSize;
  }
  /**
   * Is clamped later in ngOnInit, as minSize and maxSize may not be assigned yet.
   */
  public set initialSize(value: number | null) {
    this._initialSize = value;
  }

  ngOnInit(): void {
    if (this._initialSize !== null) this.size = Math.max(Math.min(this.maxSize, this._initialSize), this.minSize); // this is what clamping looks like in JS
  }

  private _unit: PaneUnit = 'px';
  public get unit(): PaneUnit {
    return this._unit;
  }
  public set unit(value: PaneUnit) {
    this._unit = value;
  }

  private _order = 0;
  public get order(): number {
    return this._order;
  }
  public set order(value: number) {
    this._order = value;
  }

  private _size = 0;
  public get size(): number {
    return this._size;
  }
  /**
   * Does not clamp the values between min and max for some flexibility when automatically sizing.
   */
  public set size(value: number) {
    this._size = value;
  }

  private _minSize = 0;
  public get minSize(): number {
    return this._minSize;
  }
  @Input()
  public set minSize(value: number) {
    this._minSize = value;
    if (value > this.maxSize) throw new PaneError(`Panel minSize was set larger than maxSize!`);
  }

  private _maxSize = Infinity;
  public get maxSize(): number {
    return this._maxSize;
  }
  @Input()
  public set maxSize(value: number) {
    this._maxSize = value;
    if (value < this._minSize) throw new PaneError(`Panel maxSize was set smaller than minSize!`);
  }

  /**
   * Notifies when pane is being resized.
   */
  @Output() onResize$ = new EventEmitter<PaneComponent>();
  /**
   * Notifies when pane size is being initialized, is html/css safe (boundingClientRect is correct after invoke).
   * Currently also gets called when a pane gets hidden / shown.
   */
  @Output() onInit$ = new EventEmitter<PaneComponent>();

  /**
   * Applies order and size correctly, by clamping, and assigning the correct unit.
   * It was separated instead of directly binding all attributes, as sizing is calculated in multiple passes,
   * and this function only gets called when all panes have their correct size.
   */
  refreshFlexStyle(): void {
    let flexBasis: number = this._size;

    if (this.size < this.minSize) flexBasis = this.minSize;
    else if (this.size > this.maxSize) flexBasis = this.maxSize;

    switch (this.unit) {
      case 'px':
      case '%':
      case 'rem':
        // Notify if resize occurred
        if (this._flexBasis != `${flexBasis}${this.unit}`) this.onResize$.emit(this);
        this._flexBasis = `${flexBasis}${this.unit}`;
        break;
      default:
        throw new PaneError(`unknown unit ${this.unit} in refreshFlexStyle`);
    }
    this._flexOrder = this._order;
  }
}
