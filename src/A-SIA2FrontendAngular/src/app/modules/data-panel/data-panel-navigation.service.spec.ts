import { HttpClient, HttpHandler } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';

import { DataPanelNavigationService } from './data-panel-navigation.service';

describe('DataPanelNavigationService', () => {
  let service: DataPanelNavigationService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [HttpClient, HttpHandler]
    });

    service = TestBed.inject(DataPanelNavigationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
