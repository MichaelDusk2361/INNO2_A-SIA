import { Component, Input} from '@angular/core';
import { DataPanelNavigationService } from '../../data-panel-navigation.service';

@Component({
  selector: 'a-sia-data-panel-navbar-entry',
  templateUrl: './data-panel-navbar-entry.component.html',
  styleUrls: ['./data-panel-navbar-entry.component.scss']
})
export class DataPanelNavbarEntryComponent{

  /**
 * The text to be displayed in the navbar entry e.g. Networks, Structure, Inspector
 */
  @Input() label = 'Panel';
  constructor(public navigationService: DataPanelNavigationService) {}

}
