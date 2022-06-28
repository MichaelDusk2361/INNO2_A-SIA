import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PaneContainerComponent } from '../pane-container.component';

import { PaneDividerComponent } from './pane-divider.component';

describe('PaneDividerComponent', () => {
  let fixture: ComponentFixture<PaneDividerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [PaneDividerComponent, PaneContainerComponent]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PaneDividerComponent);

    fixture.detectChanges();
  });
});
