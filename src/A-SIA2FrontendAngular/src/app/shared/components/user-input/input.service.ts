import { Injectable, Predicate, Renderer2, RendererFactory2 } from '@angular/core';
import { Subject } from 'rxjs';
import { Shortcut } from './shortcut';

/**
 * Handles some events globally. Some events (like pointerup, pointermove) need to be global,
 * so that it still notices input if you move your cursor outside the browser.
 *
 * It doesn't do anything else besides forwarding the triggered events to all subscribers.
 *
 * Pointer events are used instead of mouse or touch events, as it is an
 * abstraction and works with both touch and mouse input.
 */
@Injectable({
  providedIn: 'root'
})
export class InputService {
  public isClicked = false;
  public onPointerMove$ = new Subject<PointerEvent>();
  public onPointerUp$ = new Subject<PointerEvent>();
  public onPointerDown$ = new Subject<PointerEvent>();
  public onResize$ = new Subject<UIEvent>();
  public onScroll$ = new Subject<Event>();

  /**
   * Needed because it can bind angular pseudo-events (i.e. keydown.control.z)
   */
  private renderer: Renderer2;

  /**
   * Attaches all events to the document, injects RendererFactory for creating a renderer (can handle keydown pseudo-events)
   */
  constructor(rendererFactory: RendererFactory2) {
    this.renderer = rendererFactory.createRenderer(null, null);
    this.renderer.listen('document', 'pointermove', (e: PointerEvent) => {
      this.onPointerMove$.next(e);
    });
    this.renderer.listen('document', 'pointerup', (e: PointerEvent) => {
      this.onPointerUp$.next(e);
    });
    this.renderer.listen('document', 'pointerdown', (e: PointerEvent) => {
      this.onPointerDown$.next(e);
    });
    this.renderer.listen('window', 'resize', (e: UIEvent) => {
      this.onResize$.next(e);
    });
    this.renderer.listen('window', 'scroll', (e: Event) => {
      this.onScroll$.next(e);
    });
  }

  /**
   * Dictionary containing all shortcuts, and subjects that fire when the shortcut is pressed.
   * The key (string) is the event string created by the {@link Shortcut}, i.e. keycode.control.z
   */
  shortcuts: Map<string, { shortcut: Shortcut; subject: Subject<KeyboardEvent> }> = new Map<
    string,
    { shortcut: Shortcut; subject: Subject<KeyboardEvent> }
  >();

  /**
   * @param shortcut {@link Shortcut}
   * @param predicate predicate that needs to return true if the subject should invoke.
   * @returns the subject that triggers when the shortcut is pressed, and can be immediately subscribed to.
   */
  registerShortcut(shortcut: Shortcut, predicate?: Predicate<KeyboardEvent>): Subject<KeyboardEvent> {
    if (!this.shortcuts.has(shortcut.EventString)) {
      const newSubject = new Subject<KeyboardEvent>();
      this.shortcuts.set(shortcut.EventString, { subject: newSubject, shortcut: shortcut });
      // add shortcut event to document
      this.renderer.listen('document', shortcut.EventString, (event: KeyboardEvent) => {
        // abort if the shortcut is not allowed to be used in a Input, Textarea, or selection (and if not, one of these is also selected)
        if (
          !shortcut.AllowedInInputFields &&
          (event.target instanceof HTMLInputElement ||
            event.target instanceof HTMLTextAreaElement ||
            event.target instanceof HTMLSelectElement)
        )
          return;
        // prevent default if configured
        if (shortcut.PreventDefault) event.preventDefault();

        // check predicate if provided
        if (predicate === undefined || predicate(event as KeyboardEvent)) newSubject.next(event);
      });
    }

    return this.shortcuts.get(shortcut.EventString)?.subject as Subject<KeyboardEvent>;
  }
}
