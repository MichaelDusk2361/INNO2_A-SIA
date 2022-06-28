import { TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ModalService } from '@shared/components/modal/modal.service';
import { AppComponent } from './app.component';
import { DataPanelComponent } from './modules/data-panel/data-panel.component';
import { InspectorPanelComponent } from './modules/data-panel/inspector-panel/inspector-panel.component';
import { NetworkPanelComponent } from './modules/data-panel/network-panel/network-panel.component';
import { StructurePanelComponent } from './modules/data-panel/structure-panel/structure-panel.component';
import { ConnectionComponent } from './modules/graph-editor/connection/connection.component';
import { GraphControlSimulationNavigationComponent } from './modules/graph-editor/graph-editor-controls/graph-control-simulation-navigation/graph-control-simulation-navigation.component';
import { GraphControlToolComponent } from './modules/graph-editor/graph-editor-controls/graph-control-tool/graph-control-tool.component';
import { GraphControlZoomComponent } from './modules/graph-editor/graph-editor-controls/graph-control-zoom/graph-control-zoom.component';
import { GraphEditorControlsComponent } from './modules/graph-editor/graph-editor-controls/graph-editor-controls.component';
import { GraphEditorComponent } from './modules/graph-editor/graph-editor.component';
import { GroupNodeComponent } from './modules/graph-editor/group-node/group-node.component';
import { PersonNodeComponent } from './modules/graph-editor/person-node/person-node.component';
import { SocialNodeComponent } from './modules/graph-editor/social-node/social-node.component';
import { MenubarEntryComponent } from './modules/menubar/menubar-menu/menubar-entry/menubar-entry.component';
import { MenubarMenuComponent } from './modules/menubar/menubar-menu/menubar-menu.component';
import { MenubarComponent } from './modules/menubar/menubar.component';
import { MenubarService } from './modules/menubar/menubar.service';
import { DiagramLineComponent } from './modules/simulation-diagram/diagram/diagram-line/diagram-line.component';
import { DiagramComponent } from './modules/simulation-diagram/diagram/diagram.component';
import { LegendEntryComponent } from './modules/simulation-diagram/legend/legend-entry/legend-entry.component';
import { LegendComponent } from './modules/simulation-diagram/legend/legend.component';
import { SimulationDiagramComponent } from './modules/simulation-diagram/simulation-diagram.component';
import { PaneContainerComponent } from './shared/components/pane-container/pane-container.component';
import { PaneDividerComponent } from './shared/components/pane-container/pane-divider/pane-divider.component';
import { PaneComponent } from './shared/components/pane-container/pane/pane.component';

describe('AppComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RouterTestingModule],
      declarations: [
        AppComponent,
        MenubarComponent,
        MenubarMenuComponent,
        MenubarEntryComponent,
        ConnectionComponent,
        GraphEditorComponent,
        GroupNodeComponent,
        PersonNodeComponent,
        SocialNodeComponent,
        GraphEditorControlsComponent,
        GraphControlToolComponent,
        GraphControlSimulationNavigationComponent,
        GraphControlZoomComponent,
        DataPanelComponent,
        InspectorPanelComponent,
        StructurePanelComponent,
        NetworkPanelComponent,
        SimulationDiagramComponent,
        DiagramComponent,
        DiagramLineComponent,
        LegendComponent,
        LegendEntryComponent,
        PaneContainerComponent,
        PaneComponent,
        PaneDividerComponent
      ],
      providers: [MenubarService, ModalService]
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });
});
