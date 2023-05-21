import { TestBed } from '@angular/core/testing';

import { GITService } from './gitservice.service';

describe('GITService', () => {
  let service: GITService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GITService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
