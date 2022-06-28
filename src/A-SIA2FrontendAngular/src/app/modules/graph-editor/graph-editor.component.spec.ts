import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ConnectionComponent } from './connection/connection.component';
import { GraphControlSimulationNavigationComponent } from './graph-editor-controls/graph-control-simulation-navigation/graph-control-simulation-navigation.component';
import { GraphControlToolComponent } from './graph-editor-controls/graph-control-tool/graph-control-tool.component';
import { GraphControlZoomComponent } from './graph-editor-controls/graph-control-zoom/graph-control-zoom.component';
import { GraphEditorControlsComponent } from './graph-editor-controls/graph-editor-controls.component';

import { GraphEditorComponent } from './graph-editor.component';
import { GroupNodeComponent } from './group-node/group-node.component';
import { PersonNodeComponent } from './person-node/person-node.component';
import { SocialNodeComponent } from './social-node/social-node.component';

describe('GraphEditorComponent', () => {
  let component: GraphEditorComponent;
  let fixture: ComponentFixture<GraphEditorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [
        GraphEditorComponent,
        ConnectionComponent,
        GraphEditorComponent,
        GroupNodeComponent,
        PersonNodeComponent,
        SocialNodeComponent,
        GraphEditorControlsComponent,
        GraphControlToolComponent,
        GraphControlSimulationNavigationComponent,
        GraphControlZoomComponent
      ]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(GraphEditorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
