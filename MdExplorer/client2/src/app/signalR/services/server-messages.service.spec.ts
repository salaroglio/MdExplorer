import { TestBed } from '@angular/core/testing';

import { ServerMessagesService } from './server-messages.service';

describe('MonitorMDService', () => {
  let service: ServerMessagesService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ServerMessagesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
