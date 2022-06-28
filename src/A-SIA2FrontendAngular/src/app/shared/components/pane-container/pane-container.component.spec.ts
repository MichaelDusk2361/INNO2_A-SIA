import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PaneContainerComponent } from './pane-container.component';

describe('PaneContainerComponent', () => {
  let component: PaneContainerComponent;
  let fixture: ComponentFixture<PaneContainerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [PaneContainerComponent]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PaneContainerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
