import { Directive, ElementRef, HostBinding } from '@angular/core';

@Directive({
  selector: '[aSiaInputMedium]'
})
export class InputMediumDirective {
  @HostBinding('class')
  elementClass = 'input-medium';

  constructor(private element: ElementRef) {}
}
