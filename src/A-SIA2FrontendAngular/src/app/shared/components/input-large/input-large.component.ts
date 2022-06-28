import { Component, Input } from '@angular/core';

@Component({
  selector: 'a-sia-input-large',
  templateUrl: './input-large.component.html',
  styleUrls: ['./input-large.component.scss']
})
export class InputLargeComponent {
  @Input() label = 'LABEL';
  @Input() for = '';
}
