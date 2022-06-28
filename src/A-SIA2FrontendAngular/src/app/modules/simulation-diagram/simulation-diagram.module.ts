import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SimulationDiagramComponent } from './simulation-diagram.component';
import { DiagramComponent } from './diagram/diagram.component';
import { LegendComponent } from './legend/legend.component';
import { LegendEntryComponent } from './legend/legend-entry/legend-entry.component';
import { DiagramLineComponent } from './diagram/diagram-line/diagram-line.component';

@NgModule({
  declarations: [
    SimulationDiagramComponent,
    DiagramComponent,
    LegendComponent,
    LegendEntryComponent,
    DiagramLineComponent
  ],
  imports: [CommonModule],
  exports: [SimulationDiagramComponent]
})
export class SimulationDiagramModule {}
