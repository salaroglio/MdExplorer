import { TestBed } from '@angular/core/testing';

import { MonitorMDService } from './monitor-md.service';

describe('MonitorMDService', () => {
  let service: MonitorMDService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MonitorMDService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
