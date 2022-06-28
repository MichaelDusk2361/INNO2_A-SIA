import { ComponentFixture, TestBed } from '@angular/core/testing';
import { InputLargeComponent } from './input-large.component';
import { InputLargeDirective } from './input-large.directive';

describe('InputLargeDirective', () => {
  let largeInput: ComponentFixture<InputLargeComponent>;

  /**
   * Test the directive
   * for now we just create a component and check if it worked?
   * usefull??
   * @todo think about proper way to unit test
   */
  beforeEach(() => {
    largeInput = TestBed.configureTestingModule({
      declarations: [InputLargeComponent, InputLargeDirective]
    }).createComponent(InputLargeComponent);
  });

  it('should create an instance', () => {
    expect(largeInput).toBeTruthy();
  });
});
