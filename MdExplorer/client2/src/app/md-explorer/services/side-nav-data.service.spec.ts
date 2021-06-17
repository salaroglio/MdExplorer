import { TestBed } from '@angular/core/testing';

import { SideNavDataService } from './side-nav-data.service';

describe('SideNavDataService', () => {
  let service: SideNavDataService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SideNavDataService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
