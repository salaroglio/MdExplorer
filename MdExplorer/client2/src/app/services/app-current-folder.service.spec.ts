import { TestBed } from '@angular/core/testing';

import { AppCurrentFolderService } from './app-current-folder.service';

describe('AppCurrentFolderService', () => {
  let service: AppCurrentFolderService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AppCurrentFolderService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
