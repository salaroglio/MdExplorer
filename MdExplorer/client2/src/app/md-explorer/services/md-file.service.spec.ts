import { TestBed } from '@angular/core/testing';

import { MdFileService } from './md-file.service';

describe('MdFileService', () => {
  let service: MdFileService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MdFileService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
