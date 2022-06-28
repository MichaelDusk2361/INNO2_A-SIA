import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DiagramLineComponent } from './diagram/diagram-line/diagram-line.component';
import { DiagramComponent } from './diagram/diagram.component';
import { LegendEntryComponent } from './legend/legend-entry/legend-entry.component';
import { LegendComponent } from './legend/legend.component';

import { SimulationDiagramComponent } from './simulation-diagram.component';

describe('SimulationDiagramComponent', () => {
  let component: SimulationDiagramComponent;
  let fixture: ComponentFixture<SimulationDiagramComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [
        SimulationDiagramComponent,
        DiagramComponent,
        DiagramLineComponent,
        LegendComponent,
        LegendEntryComponent
      ]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SimulationDiagramComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
