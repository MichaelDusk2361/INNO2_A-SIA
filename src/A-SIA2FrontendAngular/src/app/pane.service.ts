import { Injectable } from '@angular/core';
@Injectable({
  providedIn: 'root'
})
export class PaneService {
  private _simulationDiagramIsOpen = false;
  get simulationDiagramIsOpen(): boolean {
    return this._simulationDiagramIsOpen;
  }

  private _graphEditorIsOpen = true;
  get graphEditorIsOpen(): boolean {
    return this._graphEditorIsOpen;
  }

  private _dataPanelIsOpen = true;
  get dataPanelIsOpen(): boolean {
    return this._dataPanelIsOpen;
  }

  toggleSimulationDiagram = (): void => {
    this._simulationDiagramIsOpen = !this._simulationDiagramIsOpen;
  };

  toggleGraphEditor = (): void => {
    this._graphEditorIsOpen = !this._graphEditorIsOpen;
  };

  toggleDataPanel = (): void => {
    this._dataPanelIsOpen = !this._dataPanelIsOpen;
  };
}
