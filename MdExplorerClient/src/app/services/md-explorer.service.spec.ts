import { TestBed } from '@angular/core/testing';

import { MdExplorerService } from './md-explorer.service';

describe('MdExplorerService', () => {
  let service: MdExplorerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MdExplorerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
