import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GraphEditorComponent } from './graph-editor.component';
import { SocialNodeComponent } from './social-node/social-node.component';
import { PersonNodeComponent } from './person-node/person-node.component';
import { GroupNodeComponent } from './group-node/group-node.component';
import { ConnectionComponent } from './connection/connection.component';
import { GraphEditorControlsComponent } from './graph-editor-controls/graph-editor-controls.component';
import { GraphControlToolComponent } from './graph-editor-controls/graph-control-tool/graph-control-tool.component';
import { GraphControlZoomComponent } from './graph-editor-controls/graph-control-zoom/graph-control-zoom.component';
import { GraphControlSimulationNavigationComponent } from './graph-editor-controls/graph-control-simulation-navigation/graph-control-simulation-navigation.component';
import { FormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    GraphEditorComponent,
    SocialNodeComponent,
    PersonNodeComponent,
    GroupNodeComponent,
    ConnectionComponent,
    GraphEditorControlsComponent,
    GraphControlToolComponent,
    GraphControlZoomComponent,
    GraphControlSimulationNavigationComponent
  ],
  imports: [CommonModule, FormsModule],
  exports: [GraphEditorComponent]
})
export class GraphEditorModule {}
