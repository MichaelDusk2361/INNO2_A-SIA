import { Component, Input } from '@angular/core';

@Component({
  selector: 'a-sia-input-medium',
  templateUrl: './input-medium.component.html',
  styleUrls: ['./input-medium.component.scss']
})
export class InputMediumComponent {
  @Input() label = 'LABEL';
  @Input() for = '';
}
