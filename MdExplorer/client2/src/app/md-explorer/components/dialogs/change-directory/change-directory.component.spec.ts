import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChangeDirectoryComponent } from './change-directory.component';

describe('ChangeDirectoryComponent', () => {
  let component: ChangeDirectoryComponent;
  let fixture: ComponentFixture<ChangeDirectoryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ChangeDirectoryComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ChangeDirectoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
