import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InspectorPanelComponent } from './inspector-panel.component';

describe('InspectorPanelComponent', () => {
  let component: InspectorPanelComponent;
  let fixture: ComponentFixture<InspectorPanelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [InspectorPanelComponent]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(InspectorPanelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
