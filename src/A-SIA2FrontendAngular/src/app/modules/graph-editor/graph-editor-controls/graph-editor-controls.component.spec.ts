import { ComponentFixture, TestBed } from '@angular/core/testing';
import { GraphControlSimulationNavigationComponent } from './graph-control-simulation-navigation/graph-control-simulation-navigation.component';
import { GraphControlToolComponent } from './graph-control-tool/graph-control-tool.component';
import { GraphControlZoomComponent } from './graph-control-zoom/graph-control-zoom.component';

import { GraphEditorControlsComponent } from './graph-editor-controls.component';

describe('GraphEditorControlsComponent', () => {
  let component: GraphEditorControlsComponent;
  let fixture: ComponentFixture<GraphEditorControlsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [
        GraphEditorControlsComponent,
        GraphControlSimulationNavigationComponent,
        GraphControlToolComponent,
        GraphControlZoomComponent
      ]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(GraphEditorControlsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
