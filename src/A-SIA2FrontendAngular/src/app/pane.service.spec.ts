import { TestBed } from '@angular/core/testing';

import { PaneService } from './pane.service';

describe('PaneService', () => {
  let service: PaneService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PaneService);
  });

  it('should have simulation diagram closed', () => {
    expect(service.simulationDiagramIsOpen).toBeFalse();
  });

  it('should have graphEditor opened', () => {
    expect(service.graphEditorIsOpen).toBeTrue();
  });

  it('should have data panel opened', () => {
    expect(service.dataPanelIsOpen).toBeTrue();
  });

  it('should toggle everything respectively', () => {
    service.toggleDataPanel();
    service.toggleGraphEditor();
    service.toggleSimulationDiagram();
    expect(!service.dataPanelIsOpen && service.simulationDiagramIsOpen && !service.graphEditorIsOpen).toBeTrue();
  });
});
