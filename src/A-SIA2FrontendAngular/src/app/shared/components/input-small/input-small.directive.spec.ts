import { ComponentFixture, TestBed } from '@angular/core/testing';
import { InputSmallComponent } from './input-small.component';
import { InputSmallDirective } from './input-small.directive';

describe('InputSmallDirective', () => {
  let largeInput: ComponentFixture<InputSmallComponent>;

  /**
   * Test the directive
   * for now we just create a component and check if it worked?
   * usefull??
   * @todo think about proper way to unit test
   */
  beforeEach(() => {
    largeInput = TestBed.configureTestingModule({
      declarations: [InputSmallComponent, InputSmallDirective]
    }).createComponent(InputSmallComponent);
  });

  it('should create an instance', () => {
    expect(largeInput).toBeTruthy();
  });
});
