import { TestBed } from '@angular/core/testing';

import { HrefInterceptorService } from './href-interceptor.service';

describe('HrefInterceptorService', () => {
  let service: HrefInterceptorService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(HrefInterceptorService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
