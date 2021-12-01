import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OpenNewFolderComponent } from './open-new-folder.component';

describe('OpenNewFolderComponent', () => {
  let component: OpenNewFolderComponent;
  let fixture: ComponentFixture<OpenNewFolderComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ OpenNewFolderComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(OpenNewFolderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
