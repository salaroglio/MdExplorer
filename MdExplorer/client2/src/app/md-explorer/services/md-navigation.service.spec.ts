import { TestBed } from '@angular/core/testing';

import { MdNavigationService } from './md-navigation.service';

describe('MdNavigationService', () => {
  let service: MdNavigationService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MdNavigationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
