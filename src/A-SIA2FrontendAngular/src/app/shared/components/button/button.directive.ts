import { Directive, HostBinding, Input } from '@angular/core';

@Directive({
  selector: '[a-sia-button]'
})
export class ButtonDirective {
  @HostBinding('class') elementClass = 'a-sia-button';

  @Input()
  public set buttonStyle(value: 'primary' | 'cancel' | 'create' | 'delete' | 'modify') {
    this.elementClass += ` a-sia-button--${value}`;
  }
}
