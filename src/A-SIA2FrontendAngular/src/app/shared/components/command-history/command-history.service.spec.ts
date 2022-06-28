import { TestBed } from '@angular/core/testing';

import { CommandHistoryService } from './command-history.service';

describe('CommandHistoryService', () => {
  let service: CommandHistoryService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CommandHistoryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
