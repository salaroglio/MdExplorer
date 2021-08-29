import { TestBed } from '@angular/core/testing';

import { MdRefactoringService } from './md-refactoring.service';

describe('MdRefactoringService', () => {
  let service: MdRefactoringService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MdRefactoringService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
