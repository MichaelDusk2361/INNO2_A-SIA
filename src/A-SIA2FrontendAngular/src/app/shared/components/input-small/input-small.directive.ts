import { Directive, ElementRef, HostBinding } from '@angular/core';

@Directive({
  selector: '[aSiaInputSmall]'
})
export class InputSmallDirective {
  @HostBinding('class') elementClass = 'input-small';

  constructor() {}
}
