import { Component } from '@angular/core';
import { DataPanelNavigationService } from './data-panel-navigation.service';

@Component({
  selector: 'a-sia-data-panel',
  templateUrl: './data-panel.component.html',
  styleUrls: ['./data-panel.component.scss']
})
export class DataPanelComponent {  
  constructor(public navigationService: DataPanelNavigationService){}
}
