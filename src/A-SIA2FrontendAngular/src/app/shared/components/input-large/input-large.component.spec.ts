import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InputLargeComponent } from './input-large.component';

describe('InputLargeComponent', () => {
  let component: InputLargeComponent;
  let fixture: ComponentFixture<InputLargeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [InputLargeComponent]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(InputLargeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
