import { TestBed } from '@angular/core/testing';

import { AppCurrentMetadataService } from './app-current-metadata.service';

describe('AppCurrentMetadataService', () => {
  let service: AppCurrentMetadataService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AppCurrentMetadataService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
