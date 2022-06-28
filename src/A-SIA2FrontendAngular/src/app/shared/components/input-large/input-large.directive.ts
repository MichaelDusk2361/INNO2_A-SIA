import { Directive, ElementRef, HostBinding } from '@angular/core';

@Directive({
  selector: '[aSiaInputLarge]'
})
export class InputLargeDirective {
  @HostBinding('class')
  elementClass = 'input-large';

  constructor(private element: ElementRef) {}
}
