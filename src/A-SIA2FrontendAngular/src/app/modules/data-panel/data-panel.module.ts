import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NetworkPanelComponent } from './network-panel/network-panel.component';
import { DataPanelComponent } from './data-panel.component';
import { StructurePanelComponent } from './structure-panel/structure-panel.component';
import { InspectorPanelComponent } from './inspector-panel/inspector-panel.component';
import { DataPanelNavbarComponent } from './data-panel-navbar/data-panel-navbar.component';
import { DataPanelNavbarEntryComponent } from './data-panel-navbar/data-panel-navbar-entry/data-panel-navbar-entry.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { FormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    NetworkPanelComponent,
    DataPanelComponent,
    StructurePanelComponent,
    InspectorPanelComponent,
    DataPanelNavbarComponent,
    DataPanelNavbarEntryComponent
  ],
  imports: [CommonModule, SharedModule, FormsModule],
  exports: [DataPanelComponent]
})
export class DataPanelModule {}
