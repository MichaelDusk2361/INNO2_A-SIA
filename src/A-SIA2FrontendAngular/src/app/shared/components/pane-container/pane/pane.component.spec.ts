import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PaneContainerComponent } from '../pane-container.component';
import { PaneDividerComponent } from '../pane-divider/pane-divider.component';

import { PaneComponent } from './pane.component';

describe('PaneComponent', () => {
  let component: PaneComponent;
  let fixture: ComponentFixture<PaneComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [PaneComponent, PaneContainerComponent, PaneDividerComponent]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PaneComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
