import { TestBed } from '@angular/core/testing';

import { WaitingDialogService } from './waiting-dialog.service';

describe('WaitingDialogService', () => {
  let service: WaitingDialogService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(WaitingDialogService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
