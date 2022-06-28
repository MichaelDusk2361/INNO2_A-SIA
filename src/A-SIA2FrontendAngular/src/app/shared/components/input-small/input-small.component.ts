import { Component, Input } from '@angular/core';

@Component({
  selector: 'a-sia-input-small',
  templateUrl: './input-small.component.html',
  styleUrls: ['./input-small.component.scss']
})
export class InputSmallComponent {
  @Input() label = 'LABEL';
  @Input() for = '';
}
