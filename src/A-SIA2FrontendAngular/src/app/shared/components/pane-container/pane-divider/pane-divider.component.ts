import { Component, Host, HostBinding, Input, OnDestroy, OnInit, Renderer2 } from '@angular/core';
import { Subscription } from 'rxjs';
import { InputService } from '@shared/components/user-input/input.service';
import { PaneContainerComponent } from '../pane-container.component';
import { PaneDividerError } from '../PaneErrors';

/**
 * This element is only used internally, and generated automatically. If a bordering (at the start or end of container)
 * pane has both minSize and maxSize (so can't be resized) there will be no divider next to it.
 * The PaneDividerComponent's host element actually has no size. It is a 0 width / height element,
 * that has an absolutely positioned element centered over it. It having no width makes the whole Pane sizing process a whole lot easier.
 *
 * You might notice that the cursor changes when hovering / resizing. This had to be done globally, by adding and removing
 * a css class to the body (ns-resize or ew-resize, you can see them (in code) in the global styling file styles.scss).
 * See also: {@link https://stackoverflow.com/questions/10750582/global-override-of-mouse-cursor-with-javascript}
 *
 */
@Component({
  selector: 'a-sia-pane-divider',
  templateUrl: './pane-divider.component.html',
  styleUrls: ['./pane-divider.component.scss']
})
export class PaneDividerComponent implements OnInit, OnDestroy {
  opacity = 0;
  width!: string;
  height!: string;
  transform!: string;
  cursor!: string;
  private _direction!: 'row' | 'column';
  @Input() set direction(value: 'row' | 'column') {
    if (value == 'row') {
      this._direction = value;
      this.height = '100%';
      this.transform = 'translateX(-50%)'; // shift so that it is centered around the "0-size" parent
      this.width = '0.5rem';
    } else if (value == 'column') {
      this._direction = value;
      this.height = '0.5rem';
      this.transform = 'translateY(-50%)'; // shift so that it is centered around the "0-size" parent
      this.width = '100%';
    } else {
      throw new PaneDividerError(`Unknown direction ${value} received from parent`);
    }
  }

  /**
   * Used by the {@link PaneContainerComponent} through the {@link PaneIfDirective},
   * to temporarily hide the divider when toggling a pane. For more info see {@link PaneIfDirective}.
   */
  visibility: 'hidden' | 'visible' = 'hidden';

  @Input() @HostBinding('style.order') order!: number;

  /**
   * Subscription holding all global input observers. See {@link InputService}
   */
  globalInputSubscription = new Subscription();

  isClicked = false;

  /**
   * @param inputService Needed for handling opacity, cursor styling, and "moving" the divider
   * @param renderer Manages the cursor styling. Sadly this has to be done globally,
   * by setting the cursor-styling on the body element.
   * @param parent Receives the clicked divider, and then uses it to initialize the whole resizing process.
   */
  constructor(
    private inputService: InputService,
    private renderer: Renderer2,
    @Host() private parent: PaneContainerComponent
  ) {
    this.globalInputSubscription.add(
      this.inputService.onPointerUp$.subscribe(() => {
        this.onGlobalPointerUp();
      })
    );
  }

  /**
   * Subscribes to global onPointerUp, and assigns CSS depending on orientation (row or column)
   */
  ngOnInit(): void {
    this.show();
    this.setDefaultOpacity();
  }

  /**
   * Changes opacity, and assigns itself to a property in the parent.
   * This will later be used to build separate arrays of Panes left and right from the divider,
   * which in turn is used when moving the divider.
   */
  onPointerDown(event: PointerEvent): void {
    event.stopPropagation();
    this.isClicked = true;
    this.parent.clickedPaneDivider = this;
    this.setClickOpacity();
  }

  /**
   * Changes opacity and cursor styling
   */
  onPointerEnter(): void {
    if (!this.isClicked) this.setHoverOpacity();
    this.setResizeCursor();
  }

  /**
   * Changes opacity and cursor styling
   */
  onPointerLeave(): void {
    if (!this.isClicked) {
      this.setDefaultCursor();
      this.setDefaultOpacity();
    }
  }

  /**
   * Reset cursor and opacity
   */
  onGlobalPointerUp(): void {
    this.isClicked = false;
    this.setDefaultOpacity();
    this.renderer.removeClass(document.body, 'ew-resize');
    this.renderer.removeClass(document.body, 'ns-resize');
  }

  /**
   * Opacity when divider is just chilling
   */
  setDefaultOpacity(): void {
    this.opacity = 0;
  }
  /**
   * Opacity when divider is hovered
   */
  setHoverOpacity(): void {
    this.opacity = 0.3;
  }
  /**
   * Opacity when divider is clicked
   */
  setClickOpacity(): void {
    this.opacity = 0.7;
  }

  /**
   * Resets cursor globally
   */
  setDefaultCursor(): void {
    this.renderer.removeClass(document.body, 'ns-resize');
    this.renderer.removeClass(document.body, 'ew-resize');
    this.cursor = 'default';
  }

  /**
   * Sets cursor globally when hovered / clicked / moved
   */
  setResizeCursor(): void {
    if (this._direction == 'row') {
      this.renderer.addClass(document.body, 'ew-resize');
      this.renderer.removeClass(document.body, 'ns-resize');
    } else if (this._direction == 'column') {
      this.renderer.addClass(document.body, 'ns-resize');
      this.renderer.removeClass(document.body, 'ew-resize');
    } else {
      throw new PaneDividerError(`Unknown direction ${this._direction} received from parent`);
    }
  }

  hide(): void {
    this.visibility = 'hidden';
  }
  show(): void {
    this.visibility = 'visible';
  }

  /**
   * Gets destroyed when surrounding Panes get closed by the {@link PaneIfDirective}.
   * Therefore needs to unsubscribe all global events.
   */
  ngOnDestroy(): void {
    this.globalInputSubscription.unsubscribe();
  }
}
