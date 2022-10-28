import { TestBed } from '@angular/core/testing';

import { GITServiceService } from './gitservice.service';

describe('GITServiceService', () => {
  let service: GITServiceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GITServiceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
