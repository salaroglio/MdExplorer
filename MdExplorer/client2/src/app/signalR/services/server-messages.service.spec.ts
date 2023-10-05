import { TestBed } from '@angular/core/testing';

import { MdServerMessagesService } from './server-messages.service';

describe('MonitorMDService', () => {
  let service: MdServerMessagesService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MdServerMessagesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
