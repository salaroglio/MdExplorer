import { TestBed } from '@angular/core/testing';

import { DocServiceService } from './doc-service.service';

describe('DocServiceService', () => {
  let service: DocServiceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DocServiceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
