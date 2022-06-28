import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GraphControlSimulationNavigationComponent } from './graph-control-simulation-navigation.component';

describe('GraphControlSimulationNavigationComponent', () => {
  let component: GraphControlSimulationNavigationComponent;
  let fixture: ComponentFixture<GraphControlSimulationNavigationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [GraphControlSimulationNavigationComponent]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(GraphControlSimulationNavigationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
