import { Directive } from '@angular/core';
import { ButtonDirective } from './button.directive';

@Directive({
  selector: '[a-sia-button-small]'
})
export class ButtonSmallDirective extends ButtonDirective {
  constructor() {
    super();
    this.elementClass += ` a-sia-button-small`;
  }
}
