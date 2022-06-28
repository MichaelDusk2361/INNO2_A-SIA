import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class DataPanelNavigationService {
  private _networksPanelIsOpen = false;
  get networksPanelIsOpen(): boolean {
    return this._networksPanelIsOpen;
  }

  private _structurePanelIsOpen = false;
  get structurePanelIsOpen(): boolean {
    return this._structurePanelIsOpen;
  }

  private _inspectorPanelIsOpen = true;
  get inspectorPanelIsOpen(): boolean {
    return this._inspectorPanelIsOpen;
  }

  navigate = (label: string): void => {
    if (label == 'Networks') this.navigateToNetworksPanel();
    if (label == 'Structure') this.navigateToStructurePanel();
    if (label == 'Inspector') this.navigateToInspectorPanel();
  };

  getStatus = (label: string): boolean => {
    if (label == 'Networks') return this._networksPanelIsOpen;
    if (label == 'Structure') return this._structurePanelIsOpen;
    if (label == 'Inspector') return this._inspectorPanelIsOpen;
    return false;
  };

  navigateToNetworksPanel = (): void => {
    this._networksPanelIsOpen = true;
    this._structurePanelIsOpen = false;
    this._inspectorPanelIsOpen = false;
  };

  navigateToStructurePanel = (): void => {
    this._networksPanelIsOpen = false;
    this._structurePanelIsOpen = true;
    this._inspectorPanelIsOpen = false;
  };

  navigateToInspectorPanel = (): void => {
    this._networksPanelIsOpen = false;
    this._structurePanelIsOpen = false;
    this._inspectorPanelIsOpen = true;
  };
}
