import { InputMediumDirective } from './input-medium.directive';
import { InputMediumComponent } from './input-medium.component';
import { ComponentFixture, TestBed } from '@angular/core/testing';

describe('InputMediumDirective', () => {
  let mediumInput: ComponentFixture<InputMediumComponent>;

  beforeEach(() => {
    mediumInput = TestBed.configureTestingModule({
      declarations: [InputMediumComponent, InputMediumDirective]
    }).createComponent(InputMediumComponent);
  });

  it('should create an instance', () => {
    expect(mediumInput).toBeTruthy();
  });
});
