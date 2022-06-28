import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InputSmallComponent } from './input-small.component';

describe('InputSmallComponent', () => {
  let component: InputSmallComponent;
  let fixture: ComponentFixture<InputSmallComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [InputSmallComponent]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(InputSmallComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
