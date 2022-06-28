import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InputMediumComponent } from './input-medium.component';

describe('InputMediumComponent', () => {
  let component: InputMediumComponent;
  let fixture: ComponentFixture<InputMediumComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [InputMediumComponent]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(InputMediumComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
